using coolbeats.scripts.managerScripts;
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

public partial class sceneManagment : managerNode
{
	[Export] public PackedScene menuScene;
	[Export] public PackedScene gameScene;
	[Export] public PackedScene sandboxScene;
	public List<Node> sceneNodes;
	public Node gameNode;
	public FallbackDictionary<string, bool> gameStates;
	public override void _EnterTree()
	{
		mAccess.setup(GetTree().GetNodesInGroup("admin"));
	}

    public override void _Ready()
    {
		sceneNodes = GetNodeList(GetTree().Root);
		for (int i = 0; i < sceneNodes.Count; i++)
		{
			InitialiseNode(sceneNodes[i]);
		}
		GetTree().NodeAdded += InitialiseNode;
		GetTree().NodeRemoved += TerminateNode;
		gameStates = new FallbackDictionary<string, bool>();
		gameStates.Add("menu", new Dictionary<string, bool>
		{
			{"gameActive", false},
			{"moveCamera", false},
			{"unitControl", false},
			{"draw", false},
		});
		gameStates.Add("gameActive", new Dictionary<string, bool>
		{
			{"gameActive", true},
		});
		gameStates.Add("inGame", new Dictionary<string, bool>
		{
			{"moveCamera", true},
			{"unitControl", true},
		}, "gameActive");
		gameStates.Add("spriteCreator", new Dictionary<string, bool>
		{
			{"moveCamera", true},
			{"draw", true},
		});
		gameStates.Add("unitCreator", new Dictionary<string, bool>
		{
			{"moveCamera", true},
		});
		gameStates.SetDefault("menu");
    }

	public void startGame()
	{
		changeSceneAfterUnsavedCheck(() =>
		{
			startStoredGameScene(EntityFrameworkManagement.DefaultGameId, "tactical");
		});
	}

	public void showGameTypeSelection()
	{
		VBoxContainer content = new VBoxContainer { CustomMinimumSize = new Vector2(320, 150) };
		content.AddChild(new Label { Text = "Choose game type" });
		foreach (StoredGame game in mAccess.entityFrameworkManager.GetStartGameTypes())
		{
			Button button = new Button { Text = game.Id == EntityFrameworkManagement.DefaultGameId ? "RTS — " + game.Name : game.Name };
			button.Pressed += () =>
			{
				mAccess.windowManager.closeWindow("Game Type", false);
				startStoredGameScene(game.Id, game.DefaultSceneKey);
			};
			content.AddChild(button);
		}
		mAccess.windowManager.openWindow("Game Type", content, "staticMenu", false);
	}

	public void startStoredGameScene(Guid gameId, string sceneKey)
	{
		StoredGameScene definition = mAccess.entityFrameworkManager.GetGameScene(gameId, sceneKey)
			?? throw new InvalidOperationException("Game scene was not found: " + sceneKey);
		PackedScene packedScene = GD.Load<PackedScene>(definition.SceneResourcePath)
			?? throw new InvalidOperationException("Game scene resource was not found: " + definition.SceneResourcePath);

		GetTree().ChangeSceneToPacked(packedScene);
		mAccess.gameSessionManager.Start(gameId, sceneKey);
		gameStates.Switch(stateNameFor(definition.SceneTypeId));
		mAccess.uiManager.changeUI(definition.UiKey);
	}

	public void loadStoredGameSession(Guid gameId, string saveName)
	{
		StoredSaveSlot save = mAccess.entityFrameworkManager.GetGameSession(gameId, saveName)
			?? throw new InvalidOperationException("Save slot was not found: " + saveName);
		StoredGameScene definition = mAccess.entityFrameworkManager.GetGameScene(gameId, save.SceneKey)
			?? throw new InvalidOperationException("Saved scene was not found: " + save.SceneKey);
		PackedScene packedScene = GD.Load<PackedScene>(definition.SceneResourcePath)
			?? throw new InvalidOperationException("Saved scene resource was not found: " + definition.SceneResourcePath);

		GetTree().ChangeSceneToPacked(packedScene);
		mAccess.gameSessionManager.Load(gameId, saveName);
		gameStates.Switch(stateNameFor(definition.SceneTypeId));
		mAccess.uiManager.changeUI(definition.UiKey);
	}
	public void startMenu()
	{
		changeSceneAfterUnsavedCheck(() =>
		{
			startMenuImmediately();
		});
	}
	// Automated scenarios must always restore their starting state without an
	// unsaved-object dialog interrupting test completion.
	public void startMenuForTests()
	{
		startMenuImmediately();
	}
	void startMenuImmediately()
	{
		mAccess.gameSessionManager.End();
		GetTree().ChangeSceneToPacked(menuScene);
		gameStates.Switch("menu");
		mAccess.uiManager.changeUI("main");
	}
	public void spriteCreator()
	{
		changeSceneAfterUnsavedCheck(() =>
		{
			startStoredGameScene(EntityFrameworkManagement.DefaultGameId, "spriteCreator");
		});
	}
	public void unitCreator()
	{
		changeSceneAfterUnsavedCheck(() =>
		{
			startStoredGameScene(EntityFrameworkManagement.DefaultGameId, "unitCreator");
		});
	}
	public void sandbox()
	{
		changeSceneAfterUnsavedCheck(() =>
		{
			startStoredGameScene(EntityFrameworkManagement.DefaultGameId, "sandbox");
		});
	}

	public bool HasGameCapability(string capability)
	{
		GameSession session = mAccess.gameSessionManager?.Current;
		return session == null ? gameStates[capability] : session.HasCapability(capability);
	}

	string stateNameFor(string sceneTypeId)
	{
		return sceneTypeId switch
		{
			"tactical-selection" => "inGame",
			"sprite-authoring" => "spriteCreator",
			"unit-authoring" => "unitCreator",
			"sandbox" => "gameActive",
			"moba-projectile-arena" => "gameActive",
			_ => "menu"
		};
	}

	void changeSceneAfterUnsavedCheck(Action changeScene)
	{
		if (mAccess.entityFrameworkManager == null)
		{
			changeScene();
			return;
		}

		mAccess.entityFrameworkManager.CheckUnsavedObjects(canChange =>
		{
			if (canChange)
			{
				changeScene();
			}
		});
	}

	private void TerminateNode(Node node)
	{
		sceneNodes.Remove(node);
	}

	private void InitialiseNode(Node node)
	{
		if (mAccess.styleManager != null && mAccess.styleManager.isSetupComplete)
		{
			mAccess.styleManager.applyUniversalStyleTree(node);
		}
		if (node is CanvasItem canvasItem)
		{
			mAccess.layerManager.addLayer(canvasItem);
		}
		mAccess.logicManager.entityCreation(node);
		if (node is AnimationPlayer player)
		{
			foreach (string group in node.GetGroups())
			{
				player.AddAnimationLibrary(group, mAccess.animationManager.animationSets[group]);
			}
		}
	}
	
	public List<Node> GetNodeList(Node root)
	{
		List<Node> result = new();
		Traverse(root, result);
		return result;
	}

	private void Traverse(Node current, List<Node> result)
	{
		result.Add(current);

		foreach (Node child in current.GetChildren())
		{
			Traverse(child, result);
		}
	}
}
