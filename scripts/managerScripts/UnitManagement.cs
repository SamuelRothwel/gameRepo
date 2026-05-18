using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.AttachedLogic.Components;
using Godot;

namespace coolbeats.scripts.managerScripts
{
    public partial class UnitManagement : managerNode
    {
        public Dictionary<Guid, unitControler> units = new Dictionary<Guid, unitControler>();
        public Dictionary<string, UnitDefinition> unitDefinitions = new Dictionary<string, UnitDefinition>();
        public Dictionary<string, StoredUnitComponentType> unitVariableMetadata = new Dictionary<string, StoredUnitComponentType>();
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
            UnitDefinition marine = createDefaultMarineDefinition();
            RegisterUnitDefinition(marine);
            mAccess.entityFrameworkManager?.EnsureUnitDefinition(marine);

            foreach (UnitDefinition storedDefinition in mAccess.entityFrameworkManager?.GetUnitDefinitions(CreateKnownBehaviors) ?? new List<UnitDefinition>())
            {
                RegisterUnitDefinition(storedDefinition);
            }
        }
        UnitDefinition createDefaultMarineDefinition()
        {
            UnitBehaviorProfile marineBehaviors = new UnitBehaviorProfile();
            marineBehaviors.SetCommand("move", "chaseTarget");
            marineBehaviors.SetCommand("attack", "attackTarget", "chaseTarget");
            marineBehaviors.SetCommand("idle", "scanAttack", "scanChase");
            marineBehaviors.SetCommand("holdPosition", "scanAttack", "attackTarget");
            marineBehaviors.SetCommand("attackMove", "scanAttack", "scanChase", "chaseTarget");

            UnitDefinition marine = new UnitDefinition
            {
                Name = "marine",
                CommandType = "attacker",
                Radius = 30,
                DetectionRadius = 150,
                MaxHP = 50,
                BehaviorProfile = marineBehaviors,
                BehaviorFactory = CreateKnownBehaviors
            };
            marine.NumericalTraits["speed"] = 1;
            marine.NumericalTraits["attackRange"] = 0;
            marine.DescriptiveTraits["role"] = "attacker";
            marine.SpriteAttachments.Add(new UnitSpriteAttachmentData
            {
                Name = "body",
                SpriteSetKey = "Marine",
                Order = 0,
                Traits = new List<UnitDataTrait>
                {
                    new UnitDataTrait { Key = "damageable", ValueType = "bool", ValueJson = "true" },
                    new UnitDataTrait { Key = "hitboxEnabled", ValueType = "bool", ValueJson = "true" }
                }
            });
            marine.ComponentAttachments.Add(CreateMarineGunComponent());
            return marine;
        }
        UnitComponentAttachmentData CreateMarineGunComponent()
        {
            UnitComponentAttachmentData gun = new UnitComponentAttachmentData
            {
                Name = "gun",
                TypeName = typeof(componentGun).FullName ?? nameof(componentGun),
                Position = new Vector2(13, -10),
                Order = 0,
                Traits = new List<UnitDataTrait>
                {
                    new UnitDataTrait { Key = "role", ValueType = "text", ValueJson = JsonSerializer.Serialize("weapon") },
                    new UnitDataTrait { Key = "automaticBehaviour", ValueType = "text", ValueJson = JsonSerializer.Serialize("none") },
                    new UnitDataTrait { Key = "positionX", ValueType = "number", ValueJson = JsonSerializer.Serialize(13f) },
                    new UnitDataTrait { Key = "positionY", ValueType = "number", ValueJson = JsonSerializer.Serialize(-10f) }
                }
            };

            gun.ChildComponents.Add(new UnitComponentAttachmentData
            {
                Name = "circular sprites",
                TypeName = typeof(GunComponentRing).FullName ?? nameof(GunComponentRing),
                Order = 0,
                Traits = new List<UnitDataTrait>
                {
                    new UnitDataTrait { Key = "spriteIterator", ValueType = "text", ValueJson = JsonSerializer.Serialize("circular") },
                    new UnitDataTrait { Key = "spriteCount", ValueType = "number", ValueJson = JsonSerializer.Serialize(3f) }
                },
                SpriteAttachments = new List<UnitSpriteAttachmentData>
                {
                    new UnitSpriteAttachmentData
                    {
                        Name = "barrel",
                        SpriteSetKey = "GunBarrel",
                        Order = 0
                    },
                    new UnitSpriteAttachmentData
                    {
                        Name = "barrel",
                        SpriteSetKey = "GunBarrel",
                        Order = 1
                    },
                    new UnitSpriteAttachmentData
                    {
                        Name = "barrel",
                        SpriteSetKey = "GunBarrel",
                        Order = 2
                    }
                }
            });

            return gun;
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
