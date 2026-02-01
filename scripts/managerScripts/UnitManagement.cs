using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace coolbeats.scripts.managerScripts
{
    public partial class UnitManagement : managerNode
    {
        public Dictionary<Guid, unitControler> units = new Dictionary<Guid, unitControler>();
        public List<(string, (string, (Godot.Key, string[], string)[]))> _commandSets;
        public Dictionary<string, (int, Dictionary<Godot.Key, (string[], string)>)> commandSets;
        public List<Guid> selectedUnit = new List<Guid>();
        public override void setup()
        {
            _commandSets = new List<(string, (string, (Key, string[], string)[]))>();
            commandSets = new Dictionary<string, (int, Dictionary<Key, (string[], string)>)>();

            _commandSets.Add(("commandable", ("", new (Godot.Key, string[], string)[] {(Key.Backspace, new string[] {"active"}, "idle")} )));
            _commandSets.Add(("rallyable", ("commandable", new (Godot.Key, string[], string)[] {(Key.None, new string[] {"ground", "team", "ally", "enemy"}, "move"), (Key.P, new string[] {"ground", "team", "ally", "enemy"}, "patrol"), (Key.H, new string[] {"active"}, "holdPosition")})));
            _commandSets.Add(("attacker", ("rallyable", new (Godot.Key, string[], string)[] { (Key.A,  new string[] {"ground"}, "attackMove")} )));
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
        }
        public Guid createUnit(string name, int team)
        {
            unitControler unit = mAccess.entityManager.spawnEntity(name) as unitControler;
            add(unit, team);
            return unit.ID;
        }
        public void add(unitControler unit, int team)
        {
            unit.priority = commandSets[unit.type].Item1;
            units[unit.ID] = unit;
            mAccess.teamManager.addUnit(unit.ID, team);
        }
        public void remove(Guid ID)
        {
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