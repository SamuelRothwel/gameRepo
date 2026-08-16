using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace coolbeats.scripts.managerScripts
{
    public partial class UnitManagement : managerNode
    {
        public Dictionary<Guid, unitControler> units = new Dictionary<Guid, unitControler>();
        public Dictionary<string, UnitDefinition> unitDefinitions = new Dictionary<string, UnitDefinition>();
        public Dictionary<string, StoredUnitComponentType> unitVariableMetadata = new Dictionary<string, StoredUnitComponentType>();
        public List<IUnitDefinitionProvider> unitDefinitionProviders = new List<IUnitDefinitionProvider>();
        public List<(string, (string, (Godot.Key, string[], string)[]))> _commandSets;
        public Dictionary<string, (int, Dictionary<Godot.Key, (string[], string)>)> commandSets;
        public List<Guid> selectedUnit = new List<Guid>();
		public Guid activeGameId { get; private set; }
        public override void setup()
        {
            setupUnitVariableMetadata();
			LoadGameDefinitions(EntityFrameworkManagement.DefaultGameId);
        }

		public void LoadGameDefinitions(Guid gameId)
		{
			activeGameId = gameId;
			setupCommandSets(gameId);
			unitDefinitions.Clear();
			setupUnitDefinitions(gameId);
		}

		void setupCommandSets(Guid gameId)
		{
			commandSets = new Dictionary<string, (int, Dictionary<Key, (string[], string)>)>();
			List<StoredGameCommandBinding> bindings = mAccess.entityFrameworkManager.GetGameCommandBindings(gameId);
			Dictionary<string, List<StoredGameCommandBinding>> grouped = bindings.GroupBy(binding => binding.CommandType).ToDictionary(group => group.Key, group => group.ToList());
			HashSet<string> building = new HashSet<string>();

			(int, Dictionary<Key, (string[], string)>) Build(string commandType)
			{
				if (commandSets.TryGetValue(commandType, out var existing)) return existing;
				if (!grouped.TryGetValue(commandType, out List<StoredGameCommandBinding> current)) return (0, new Dictionary<Key, (string[], string)>());
				if (!building.Add(commandType)) throw new InvalidOperationException("Command binding inheritance cycle: " + commandType);
				string parent = current.Select(binding => binding.ParentCommandType).FirstOrDefault(value => !string.IsNullOrEmpty(value)) ?? "";
				var inherited = string.IsNullOrEmpty(parent) ? (0, new Dictionary<Key, (string[], string)>()) : Build(parent);
				Dictionary<Key, (string[], string)> actions = new Dictionary<Key, (string[], string)>(inherited.Item2);
				foreach (StoredGameCommandBinding binding in current)
				{
					if (string.IsNullOrEmpty(binding.CommandName)) continue;
					string[] targets;
					try { targets = JsonSerializer.Deserialize<string[]>(binding.TargetTypesJson) ?? Array.Empty<string>(); }
					catch { targets = Array.Empty<string>(); }
					actions[(Key)binding.KeyCode] = (targets, binding.CommandName);
				}
				var built = (inherited.Item1 + (string.IsNullOrEmpty(parent) ? 0 : 1), actions);
				commandSets[commandType] = built;
				building.Remove(commandType);
				return built;
			}

			foreach (string commandType in grouped.Keys) Build(commandType);
		}

        public void setupUnitDefinitions(Guid gameId)
        {
            unitDefinitionProviders = DiscoverUnitDefinitionProviders();
            foreach (IUnitDefinitionProvider provider in unitDefinitionProviders)
            {
                foreach (UnitDefinition definition in provider.CreateDefinitions(CreateKnownBehaviors))
                {
                    UnitDefinition normalizedDefinition = NormalizeUnitDefinition(definition);
                    RegisterUnitDefinition(normalizedDefinition);
					mAccess.entityFrameworkManager?.EnsureUnitDefinition(normalizedDefinition, gameId);
					EnsureComponentDefinitions(normalizedDefinition.ComponentAttachments, gameId);
                }
            }

			foreach (UnitDefinition storedDefinition in mAccess.entityFrameworkManager?.GetUnitDefinitions(CreateKnownBehaviors, gameId) ?? new List<UnitDefinition>())
            {
                if (storedDefinition.DefinitionKind != "component")
                {
                    RegisterUnitDefinition(NormalizeUnitDefinition(storedDefinition));
                }
            }
        }
		void EnsureComponentDefinitions(IEnumerable<UnitComponentAttachmentData> components, Guid gameId)
        {
            foreach (UnitComponentAttachmentData component in components)
            {
                UnitDefinition definition = new UnitDefinition
                {
                    Name = component.Name,
                    DefinitionKind = "component",
                    CommandType = "",
                    MaxHP = 1,
                    SpriteAttachments = component.SpriteAttachments,
                    ComponentAttachments = component.ChildComponents
                };
                definition.DescriptiveTraits["componentType"] = component.TypeName;
                foreach (UnitDataTrait trait in component.Traits)
                {
                    try
                    {
                        if (trait.ValueType == "number")
                        {
                            definition.NumericalTraits[trait.Key] = JsonSerializer.Deserialize<float>(trait.ValueJson);
                        }
                        else if (trait.ValueType == "text")
                        {
                            definition.DescriptiveTraits[trait.Key] = JsonSerializer.Deserialize<string>(trait.ValueJson) ?? "";
                        }
                    }
                    catch
                    {
                    }
                }
				mAccess.entityFrameworkManager?.EnsureUnitDefinition(definition, gameId);
				EnsureComponentDefinitions(component.ChildComponents, gameId);
            }
        }
        List<IUnitDefinitionProvider> DiscoverUnitDefinitionProviders()
        {
            GD.Print("callin");
            return typeof(unitControler).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(IUnitDefinitionProvider).IsAssignableFrom(type))
                .Select(type => Activator.CreateInstance(type) as IUnitDefinitionProvider)
                .Where(provider => provider != null)
                .OrderBy(provider => provider.GetType().FullName)
                .ToList();
        }
        public UnitDefinition NormalizeUnitDefinition(UnitDefinition definition, Func<Guid, UnitDefinition> getUnitDefinition = null)
        {
            if (definition == null)
            {
                return null;
            }

            getUnitDefinition ??= GetRegisteredUnitDefinition;
            foreach (IUnitDefinitionProvider provider in unitDefinitionProviders)
            {
                definition = provider.NormalizeDefinition(definition, getUnitDefinition);
            }
            return definition;
        }
        UnitDefinition GetRegisteredUnitDefinition(Guid unitId)
        {
            return unitDefinitions.Values.FirstOrDefault(definition => definition.Id == unitId);
        }
        public IEnumerable<IUnitBehavior> CreateKnownBehaviors()
        {
            return new IUnitBehavior[]
            {
                new ScanAttackBehavior(),
                new ScanChaseBehavior(),
                new AttackTargetBehavior(),
                new ChaseTargetBehavior()
            };
        }
        public void RegisterUnitDefinition(UnitDefinition definition)
        {
            unitDefinitions[definition.Name] = definition;
        }
        public void setupUnitVariableMetadata()
        {
            List<Type> unitComponentTypes = UnitVariableMetadataScanner.GetUnitAndComponentTypes(typeof(unitControler).Assembly);
            unitVariableMetadata = mAccess.entityFrameworkManager?
                .EnsureUnitVariableMetadata(unitComponentTypes)
                .ToDictionary(type => type.TypeName) ?? new Dictionary<string, StoredUnitComponentType>();
        }
        public IReadOnlyList<StoredUnitComponentVariable> GetStoredVariables(Type type)
        {
            if (type == null)
            {
                return Array.Empty<StoredUnitComponentVariable>();
            }
            if (!unitVariableMetadata.TryGetValue(type.FullName ?? type.Name, out StoredUnitComponentType storedType))
            {
                return Array.Empty<StoredUnitComponentVariable>();
            }

            return storedType.Variables;
        }
        public Guid createUnit(string name, int team)
        {
            unitControler unit = mAccess.entityManager.spawnEntity(name) as unitControler;
            add(unit, team);
            return unit.ID;
        }
        public void add(unitControler unit, int team)
        {
            string unitKey = string.IsNullOrEmpty(unit.unitKey) ? unit.Name.ToString() : unit.unitKey;
            if (unitDefinitions.TryGetValue(unitKey, out UnitDefinition definition))
            {
                definition.ApplyTo(unit);
            }
            unit.priority = commandSets[unit.type].Item1;
            units[unit.ID] = unit;
            mAccess.teamManager.addUnit(unit.ID, team);
			mAccess.gameSessionManager?.Current?.TrackUnit(unit.ID);
        }
        public void remove(Guid ID)
        {
            mAccess.teamManager?.removeUnit(ID);
            units.Remove(ID);
			mAccess.gameSessionManager?.Current?.UntrackUnit(ID);
        }
    }
    public class command
    {
        public command(string name)
        {
            state = name;
        }
        public string state;
        public Vector2 coordinates;
        public Guid unit;
    }
}
