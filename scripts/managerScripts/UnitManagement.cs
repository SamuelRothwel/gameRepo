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
        public Dictionary<Guid, Stack<command>> commands = new Dictionary<Guid, Stack<command>>();
        public List<(string, (string, (Godot.Key, bool, string)[]))> _commandSets;
        public Dictionary<string, (int, Dictionary<Godot.Key, (bool, string)>)> commandSets;
        public List<Guid> selectedUnit = new List<Guid>();
        public override void setup()
        {
            _commandSets.Add(("commandable", ("", new (Godot.Key, bool, string)[] {(Key.Backspace, true, "stop")} )));
            _commandSets.Add(("rallyable", ("commandable", new (Godot.Key, bool, string)[] {(Key.None, false, "rally"), (Key.P, false, "patrol"), (Key.H, true, "holdPosition")})));
            _commandSets.Add(("aggressive", ("rallyable", new (Godot.Key, bool, string)[] { (Key.A, false, "attackMove")} )));
            _commandSets.Add(("test", ("aggressive", new (Godot.Key, bool, string)[] { (Key.Space, true, "printSomething")} )));
            for (int i = 0; i < _commandSets.Count; i++)
            {
                (string, (string, (Godot.Key, bool, string)[])) set = _commandSets[i];
                Dictionary<Godot.Key, (bool, string)> newSet = new Dictionary<Godot.Key, (bool, string)>();
                if (set.Item2.Item1 != "")
                {
                    foreach (KeyValuePair<Godot.Key, (bool, string)> com in commandSets[set.Item2.Item1].Item2)
                    {
                        newSet[com.Key] = com.Value;
                    }
                }
                (Godot.Key, bool, string)[] commandSet = set.Item2.Item2;
                for (int j = 0; j < commandSet.Length; i++)
                {
                    newSet[commandSet[j].Item1] = (commandSet[j].Item2, commandSet[j].Item3);
                }
                commandSets[set.Item1] = (i, newSet);
            }
        }
        public void command(Guid unit, command command, bool stacking = false)
        {
            if (!stacking)
            {
                commands[unit].Clear();
            }
            if (commands.Any())
            {
                commands[unit].Push(command);
            } else
            {
                commands[unit].Push(command);
                units[unit].state = command.state;
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
            commands[unit.ID] = new Stack<command>();
            mAccess.teamManager.addUnit(unit.ID, team);
        }
        public void remove(Guid ID)
        {
            units.Remove(ID);
            commands.Remove(ID);
        }
    }
    public class command
    {
        public command(string name)
        {
            state = name;
        }
        public string state;
        public Vector2? coordinates;
        public Guid? unit;
    }
}