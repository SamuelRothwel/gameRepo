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
        public override void setup()
        {
            _commandSets = new List<(string, (string, (Key, string[], string)[]))>();
            commandSets = new Dictionary<string, (int, Dictionary<Key, (string[], string)>)>();
            _commandSets.Add(("", ("", new (Godot.Key, string[], string)[0])));
            _commandSets.Add(("commandable", ("", new (Godot.Key, string[], string)[] {(Key.Backspace, new string[] {"active"}, "idle")} )));
            _commandSets.Add(("rallyable", ("commandable", new (Godot.Key, string[], string)[] {(Key.None, new string[] {"ground", "team", "ally", "enemy"}, "move"), (Key.P, new string[] {"ground", "team", "ally", "enemy"}, "patrol"), (Key.H, new string[] {"active"}, "holdPosition")})));
            _commandSets.Add(("attacker", ("rallyable", new (Godot.Key, string[], string)[] { (Key.A,  new string[] {"ground"}, "attackMove")} )));
            _commandSets.Add(("barracks", ("commandable", new (Godot.Key, string[], string)[] { (Key.A,  new string[] {"active"}, "train")} )));
            for (int i = 0; i < _commandSets.Count; i++)
            {
                (string, (string, (Godot.Key, string[], string)[])) set = _commandSets[i];
                Dictionary<Godot.Key, (string[], string)> newSet = new Dictionary<Godot.Key, (string[], string)>();
                if (set.Item2.Item1 != "")
                {
                    foreach (KeyValuePair<Godot.Key, (string[], string)> com in commandSets[set.Item2.Item1].Item2)
                    {
                        newSet[com.Key] = com.Value;
                    }
                }
                (Godot.Key, string[], string)[] commandSet = set.Item2.Item2;
                for (int j = 0; j < commandSet.Length; j++)
                {
                    newSet[commandSet[j].Item1] = (commandSet[j].Item2, commandSet[j].Item3);
                }
                commandSets[set.Item1] = (i, newSet);
            }
            setupUnitDefinitions();
            setupUnitVariableMetadata();
        }
        public void setupUnitDefinitions()
        {
            unitDefinitionProviders = DiscoverUnitDefinitionProviders();
            foreach (IUnitDefinitionProvider provider in unitDefinitionProviders)
            {
                foreach (UnitDefinition definition in provider.CreateDefinitions(CreateKnownBehaviors))
                {
                    UnitDefinition normalizedDefinition = NormalizeUnitDefinition(definition);
                    RegisterUnitDefinition(normalizedDefinition);
                    mAccess.entityFrameworkManager?.EnsureUnitDefinition(normalizedDefinition);
                    EnsureComponentDefinitions(normalizedDefinition.ComponentAttachments);
                }
            }

            foreach (UnitDefinition storedDefinition in mAccess.entityFrameworkManager?.GetUnitDefinitions(CreateKnownBehaviors) ?? new List<UnitDefinition>())
            {
                if (storedDefinition.DefinitionKind != "component")
                {
                    RegisterUnitDefinition(NormalizeUnitDefinition(storedDefinition));
                }
            }
        }
        void EnsureComponentDefinitions(IEnumerable<UnitComponentAttachmentData> components)
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
                mAccess.entityFrameworkManager?.EnsureUnitDefinition(definition);
                EnsureComponentDefinitions(component.ChildComponents);
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
        }
        public void remove(Guid ID)
        {
            mAccess.teamManager?.removeUnit(ID);
            units.Remove(ID);
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
