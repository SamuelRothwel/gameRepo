using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using coolbeats.scripts.managerScripts;

public sealed class GameSession
{
	readonly List<IGameTypeModule> activeModules = new();
	readonly HashSet<Guid> unitIds = new();
	readonly HashSet<string> capabilities = new(StringComparer.OrdinalIgnoreCase);
	readonly HashSet<string> rules = new(StringComparer.OrdinalIgnoreCase);

	public GameSession(StoredGame game, StoredGameScene scene, bool spawnInitialEntities)
	{
		Game = game;
		Scene = scene;
		WorldRoot = new Node { Name = "GameSessionWorld" };
		SpawnInitialEntities = spawnInitialEntities;
		ReadRules(scene.ModuleConfigJson);
	}

	public StoredGame Game { get; }
	public StoredGameScene Scene { get; }
	public Node WorldRoot { get; }
	public bool SpawnInitialEntities { get; }
	public IReadOnlyCollection<string> Capabilities => capabilities;
	public IReadOnlyCollection<Guid> UnitIds => unitIds;

	public bool HasCapability(string capability) => capabilities.Contains(capability);
	public bool HasRule(string rule) => rules.Contains(rule);
	public void AddCapability(string capability) => capabilities.Add(capability);
	public void TrackUnit(Guid id) => unitIds.Add(id);
	public void UntrackUnit(Guid id) => unitIds.Remove(id);
	internal void AddModule(IGameTypeModule module) => activeModules.Add(module);
	void ReadRules(string configuration)
	{
		try
		{
			using JsonDocument document = JsonDocument.Parse(string.IsNullOrWhiteSpace(configuration) ? "{}" : configuration);
			if (document.RootElement.TryGetProperty("Rules", out JsonElement ruleObject) && ruleObject.ValueKind == JsonValueKind.Object)
			{
				foreach (JsonProperty rule in ruleObject.EnumerateObject())
				{
					if (rule.Value.ValueKind == JsonValueKind.True) rules.Add(rule.Name);
				}
			}
		}
		catch (JsonException)
		{
			// Invalid scene config leaves optional rules disabled.
		}
	}

	public void Dispose()
	{
		for (int i = activeModules.Count - 1; i >= 0; i--)
		{
			activeModules[i].Stop(this);
		}
		activeModules.Clear();
		mAccess.inputManager?.sessionStopped(this);
		mAccess.logicManager?.sessionStopped(this);
		mAccess.damageManager?.sessionStopped(this);

		foreach (Guid id in unitIds.ToArray())
		{
			mAccess.unitManager?.remove(id);
		}
		unitIds.Clear();
		WorldRoot.QueueFree();
	}
}

public interface IGameTypeModule
{
	string Id { get; }
	IEnumerable<string> Requires { get; }
	void Start(GameSession session);
	void Stop(GameSession session);
}

public sealed class GameTypeModuleRegistry
{
	readonly Dictionary<string, IGameTypeModule> modules = new(StringComparer.OrdinalIgnoreCase);

	public void Register(IGameTypeModule module) => modules[module.Id] = module;

	public IReadOnlyList<IGameTypeModule> Resolve(string sceneTypeId)
	{
		List<IGameTypeModule> result = new();
		HashSet<string> visiting = new(StringComparer.OrdinalIgnoreCase);
		HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);
		Visit(sceneTypeId, visiting, visited, result);
		return result;
	}

	void Visit(string id, HashSet<string> visiting, HashSet<string> visited, List<IGameTypeModule> result)
	{
		if (visited.Contains(id))
		{
			return;
		}
		if (!modules.TryGetValue(id, out IGameTypeModule module))
		{
			throw new InvalidOperationException("Game type module is not registered: " + id);
		}
		if (!visiting.Add(id))
		{
			throw new InvalidOperationException("Game type module dependency cycle: " + id);
		}
		foreach (string dependency in module.Requires)
		{
			Visit(dependency, visiting, visited, result);
		}
		visiting.Remove(id);
		visited.Add(id);
		result.Add(module);
	}
}

public partial class GameSessionManagement : managerNode
{
	public GameTypeModuleRegistry gameTypes { get; private set; }
	public GameSession Current { get; private set; }

	public override void setup()
	{
		gameTypes = new GameTypeModuleRegistry();
		gameTypes.Register(new TacticalWorldGameType());
		gameTypes.Register(new TacticalSelectionGameType());
		gameTypes.Register(new SpriteAuthoringGameType());
		gameTypes.Register(new UnitAuthoringGameType());
		gameTypes.Register(new SandboxGameType());
		gameTypes.Register(new MobaProjectileArenaGameType());
	}

	public GameSession Start(Guid gameId, string sceneKey, bool spawnInitialEntities = true)
	{
		StoredGame game = mAccess.entityFrameworkManager.GetGame(gameId)
			?? throw new InvalidOperationException("Game was not found: " + gameId);
		StoredGameScene scene = mAccess.entityFrameworkManager.GetGameScene(gameId, sceneKey)
			?? throw new InvalidOperationException("Scene was not found: " + sceneKey);

		End();
		mAccess.unitManager.LoadGameDefinitions(game.Id);
		mAccess.teamManager.LoadGameTeams(game.Id);
		Current = new GameSession(game, scene, spawnInitialEntities);
		AddChild(Current.WorldRoot);
		foreach (IGameTypeModule module in gameTypes.Resolve(scene.SceneTypeId))
		{
			module.Start(Current);
			Current.AddModule(module);
		}
		return Current;
	}

	public int SaveCurrent(string name, int version = 0)
	{
		if (Current == null)
		{
			throw new InvalidOperationException("There is no active game session to save.");
		}
		GameSessionSnapshot snapshot = new GameSessionSnapshot();
		foreach (Guid id in Current.UnitIds)
		{
			if (!mAccess.unitManager.units.TryGetValue(id, out unitControler unit) || !GodotObject.IsInstanceValid(unit))
			{
				continue;
			}
				snapshot.Units.Add(new SavedUnitState
				{
					RuntimeId = unit.ID,
					DefinitionName = unit.unitKey,
				TeamIndex = mAccess.teamManager.GetTeamIndex(id),
				PositionX = unit.Position.X,
				PositionY = unit.Position.Y,
				Health = unit.HP,
				CommandName = unit.activeCommand.state,
				CommandX = unit.activeCommand.coordinates.X,
				CommandY = unit.activeCommand.coordinates.Y,
				CommandUnit = unit.activeCommand.unit
			});
		}
		return mAccess.entityFrameworkManager.SaveGameSession(
			Current.Game.Id,
			name,
			Current.Scene.SceneKey,
			version,
			JsonSerializer.Serialize(snapshot));
	}

	public GameSession Load(Guid gameId, string name)
	{
		StoredSaveSlot slot = mAccess.entityFrameworkManager.GetGameSession(gameId, name)
			?? throw new InvalidOperationException("Save slot was not found: " + name);
		GameSession session = Start(gameId, slot.SceneKey, false);
		GameSessionSnapshot snapshot = JsonSerializer.Deserialize<GameSessionSnapshot>(slot.StateJson) ?? new GameSessionSnapshot();
		Dictionary<Guid, unitControler> restoredUnits = new();
		foreach (SavedUnitState savedUnit in snapshot.Units)
		{
			Guid id = mAccess.unitManager.createUnit(savedUnit.DefinitionName, savedUnit.TeamIndex);
			if (!mAccess.unitManager.units.TryGetValue(id, out unitControler unit))
			{
				continue;
			}
			unit.Position = new Vector2(savedUnit.PositionX, savedUnit.PositionY);
			unit.HP = savedUnit.Health;
			restoredUnits[savedUnit.RuntimeId] = unit;
		}
		foreach (SavedUnitState savedUnit in snapshot.Units)
		{
			if (!restoredUnits.TryGetValue(savedUnit.RuntimeId, out unitControler unit))
			{
				continue;
			}
			command restoredCommand = new command(savedUnit.CommandName)
			{
				coordinates = new Vector2(savedUnit.CommandX, savedUnit.CommandY),
				unit = restoredUnits.TryGetValue(savedUnit.CommandUnit, out unitControler target) ? target.ID : Guid.Empty
			};
			unit.activateCommand(restoredCommand);
		}
		return session;
	}

	public void End()
	{
		Current?.Dispose();
		Current = null;
	}
}

sealed class TacticalWorldGameType : IGameTypeModule
{
	public string Id => "tactical-world";
	public IEnumerable<string> Requires => Array.Empty<string>();
	public void Start(GameSession session)
	{
		session.AddCapability("gameActive");
		session.AddCapability("moveCamera");
	}
	public void Stop(GameSession session) { }
}

sealed class TacticalSelectionGameType : IGameTypeModule
{
	public string Id => "tactical-selection";
	public IEnumerable<string> Requires => new[] { "tactical-world" };
	public void Start(GameSession session)
	{
		session.AddCapability("unitControl");
		mAccess.inputManager.SetActiveTeam(0);
		SpawnConfiguredEntities(session, session.SpawnInitialEntities);
	}
	public void Stop(GameSession session) { }

	static void SpawnConfiguredEntities(GameSession session, bool spawnUnits)
	{
		SceneSpawnConfiguration configuration;
		try
		{
			configuration = JsonSerializer.Deserialize<SceneSpawnConfiguration>(session.Scene.InitialEntitiesJson) ?? new SceneSpawnConfiguration();
		}
		catch
		{
			configuration = new SceneSpawnConfiguration();
		}

		if (spawnUnits)
		{
			foreach (SceneUnitSpawn spawn in configuration.Units)
			{
				Guid id = mAccess.unitManager.createUnit(spawn.DefinitionName, spawn.TeamIndex);
				if (mAccess.unitManager.units.TryGetValue(id, out unitControler unit))
				{
					unit.Position = new Vector2(spawn.PositionX, spawn.PositionY);
				}
			}
		}
		if (configuration.SpawnPlayerCamera)
		{
			mAccess.entityManager.spawnEntity("playerCamera");
		}
	}
}

sealed class SpriteAuthoringGameType : IGameTypeModule
{
	public string Id => "sprite-authoring";
	public IEnumerable<string> Requires => Array.Empty<string>();
	public void Start(GameSession session)
	{
		session.AddCapability("moveCamera");
		session.AddCapability("draw");
		mAccess.entityManager.spawnEntity("playerCamera");
	}
	public void Stop(GameSession session) { }
}

sealed class UnitAuthoringGameType : IGameTypeModule
{
	public string Id => "unit-authoring";
	public IEnumerable<string> Requires => Array.Empty<string>();
	public void Start(GameSession session)
	{
		session.AddCapability("moveCamera");
		mAccess.entityManager.spawnEntity("playerCamera");
	}
	public void Stop(GameSession session) { }
}

sealed class SandboxGameType : IGameTypeModule
{
	public string Id => "sandbox";
	public IEnumerable<string> Requires => Array.Empty<string>();
	public void Start(GameSession session) { }
	public void Stop(GameSession session) { }
}

// MOBA-specific input and simulation deliberately live outside RTS selection
// and command logic, while still using the common game-session lifecycle.
sealed class MobaProjectileArenaGameType : IGameTypeModule
{
	public string Id => "moba-projectile-arena";
	public IEnumerable<string> Requires => Array.Empty<string>();
	public void Start(GameSession session)
	{
		session.AddCapability("gameActive");
		session.AddCapability("mobaControl");
		DestructibleAssemblyDefinition enemyDefinition = null;
		if (mAccess.unitManager.unitDefinitions.TryGetValue("moba_test_enemy", out UnitDefinition storedEnemy) && storedEnemy.DescriptiveTraits.TryGetValue("destructibleParts", out string serializedDefinition))
		{
			try { enemyDefinition = JsonSerializer.Deserialize<DestructibleAssemblyDefinition>(serializedDefinition); }
			catch (JsonException) { }
		}
		session.WorldRoot.AddChild(new MobaProjectileArena { TestEnemyDefinition = enemyDefinition });
	}
	public void Stop(GameSession session) { }
}

public sealed class SceneSpawnConfiguration
{
	public bool SpawnPlayerCamera { get; set; }
	public List<SceneUnitSpawn> Units { get; set; } = new();
}

public sealed class SceneUnitSpawn
{
	public string DefinitionName { get; set; } = "";
	public int TeamIndex { get; set; }
	public float PositionX { get; set; }
	public float PositionY { get; set; }
}

public sealed class GameSessionSnapshot
{
	public List<SavedUnitState> Units { get; set; } = new();
}

public sealed class SavedUnitState
{
	public Guid RuntimeId { get; set; }
	public string DefinitionName { get; set; } = "";
	public int TeamIndex { get; set; }
	public float PositionX { get; set; }
	public float PositionY { get; set; }
	public float Health { get; set; }
	public string CommandName { get; set; } = "idle";
	public float CommandX { get; set; }
	public float CommandY { get; set; }
	public Guid CommandUnit { get; set; }
}
