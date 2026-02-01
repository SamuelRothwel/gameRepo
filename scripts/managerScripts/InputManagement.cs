using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;

namespace coolbeats.scripts.managerScripts
{
    public partial class InputManagement : managerNode
    {
        bool dragging;
        bool selecting;
        Vector2 startSelect;
        Vector2 cameraOffset;
        List<(string, Key)> mappings;
        public Camera2D camera;
        Rect2 selectBox;
        pen pen;
        List<Guid> selectedUnits;
        Dictionary<string, List<Guid>> selectedTypes;
        public event EventHandler unitSelected;
        string _selectedType;
        public string selectedType {get {return _selectedType;} set {_selectedType = value; unitSelected.Invoke(this, new EventArgs());}}
        team activeTeam;
        Key activeKey = Key.None;
        public override void setup()
        {
            //mappings = new List<(string, Key)>();
            //mappings.Add(("Fire", Key.Space));
            //InputEventKey e = new InputEventKey();
            //foreach ((string, Key) mapping in mappings)
            //{
            //    e.Keycode = mapping.Item2;
            //    InputMap.ActionAddEvent(mapping.Item1, e);
            //}
            activeTeam = mAccess.teamManager.teams[0];
            pen = mAccess.entityManager.getEntity("pen") as pen;
        }
        public void setCamera(Camera2D cam)
        {
            camera = cam;
            camera.AddChild(pen);   
            cameraOffset = new Vector2(-640, -360);    
        }
        public void setCameraScale(float scale)
        {
            cameraOffset.X = -640 / scale;
            cameraOffset.Y = -360 / scale;     
            camera.Zoom = new Vector2(scale, scale);
        }
        public Vector2 scaleCoords(Vector2 coords)
        {
            return coords/camera.Zoom + cameraOffset + camera.Position;
        }
        public override async void _UnhandledInput(InputEvent inp)
        {
            if (mAccess.sceneManager.inGame)
            {
                if (inp is InputEventMouseMotion motion)
                {
                    if (dragging)
                    {
                        camera.Position -= motion.Relative;
                    }
                    if (selecting)
                    {
                        pen.drawRectangle(startSelect*camera.Scale + cameraOffset, motion.Position*camera.Scale + cameraOffset);
                    }
                }
                else if (inp is InputEventMouseButton mouse)
                {
                    switch (mouse.ButtonIndex)
                    {
                        case MouseButton.Left:
                            if (activeKey != Key.None)
                            {
                                if (mouse.Pressed)
                                {
                                    sendTargetCommand(scaleCoords(mouse.GlobalPosition));
                                    activeKey = Key.None;
                                }
                            } else
                            {
                                if (mouse.Pressed)
                                {
                                    startSelect = mouse.Position;
                                    selecting = true;
                                } else
                                {
                                    if (selecting)
                                    {
                                        selecting = false;
                                        mAccess.teamManager.UpdateTeamVisions();
                                        selectedUnits = new List<Guid>();
                                        mAccess.teamManager.searchBVH(activeTeam.BVH, ref selectedUnits, math.getMinMax(scaleCoords(startSelect), scaleCoords(mouse.GlobalPosition)));
                                        int maxPriority = 0;
                                        selectedTypes = new Dictionary<string, List<Guid>>();
                                        for (int i = 0; i < selectedUnits.Count; i++)
                                        {
                                            unitControler unit = mAccess.unitManager.units[selectedUnits[i]];
                                            if (!selectedTypes.ContainsKey(unit.type))
                                            {
                                                selectedTypes[unit.type] = new List<Guid>();
                                            }
                                            selectedTypes[unit.type].Add(unit.ID);
                                            if (maxPriority < unit.priority)
                                            {
                                                selectedType = unit.type;
                                            }
                                        }
                                        pen.erase();
                                    }
                                }
                            }
                            break;
                        case MouseButton.Right:
                            if (mouse.Pressed)
                            {
                                activeKey = Key.None;
                                sendTargetCommand(scaleCoords(mouse.GlobalPosition));
                            }
                            break;
                        case MouseButton.Middle:
                            if (mouse.Pressed)
                            {
                                dragging = true;
                            } else
                            {
                                dragging = false;
                            }
                            break;
                        case MouseButton.WheelUp:
                            setCameraScale(0.5f);
                            break;
                        case MouseButton.WheelDown:
                            setCameraScale(1f);
                            break;
                    }
                }
                else if (inp is InputEventKey key)
                {
                    (string[], string) commandInstruction;
                    bool hasCommand = mAccess.unitManager.commandSets[selectedType].Item2.TryGetValue(key.Keycode, out commandInstruction);
                    if (hasCommand)
                    {
                        if (commandInstruction.Item1.FirstOrDefault() == "active")
                        {
                            command com = new command(commandInstruction.Item2);
                            sendCommands(com, commandInstruction, key.Keycode);
                        } else
                        {
                            activeKey = key.Keycode;
                        }
                    }
                }
            }
        }
        public void sendTargetCommand(Vector2 position)
        {
            (string[], string) commandInstruction;
            commandInstruction = mAccess.unitManager.commandSets[selectedType].Item2[activeKey];
            command com = new command(commandInstruction.Item2);
            getTargets(commandInstruction, ref com, position);
            sendCommands(com, commandInstruction, activeKey);
        }
        public void getTargets((string[], string) commandInstruction, ref command com, Vector2 position)
        {
            List<Guid> targets = new List<Guid>();
            foreach (string target in commandInstruction.Item1)
            {
                switch (target)
                {
                    case "ground":
                    GD.Print("mouse at", position);
                        com.coordinates = position;
                        break;
                    case "team":
                        GD.Print("num", targets.Count());
                        mAccess.teamManager.searchBVH(activeTeam.BVH, ref targets, math.getMinMax(position, 0));
                        GD.Print("numm", targets.Count());
                        break;
                    case "ally":
                        foreach (team ally in activeTeam.allies)
                        {
                            mAccess.teamManager.searchBVH(ally.BVH, ref targets,  math.getMinMax(position, 0));
                        }
                        break;
                    case "enemy":
                        foreach (team enemy in activeTeam.allies)
                        {
                            mAccess.teamManager.searchBVH(enemy.BVH, ref targets,  math.getMinMax(position, 0));
                        }
                        break;
                    default:
                        //targets.Where()
                        break;
                }
            }
            com.unit = targets.FirstOrDefault();
        }
        public void sendCommands(command com, (string[], string) commandInstruction, Key key)
        {
            (string[], string) groupCommand;
            
            if (Input.IsKeyPressed(Key.Shift))
            {
                foreach (string type in selectedTypes.Keys)
                {
                    bool hasCommand = mAccess.unitManager.commandSets[type].Item2.TryGetValue(key, out groupCommand);
                    if (hasCommand)
                    {
                        if (groupCommand.Item2 == commandInstruction.Item2)
                        {
                            foreach (Guid unit in selectedTypes[type])
                            {
                                mAccess.unitManager.units[unit].queueCommand(com);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (string type in selectedTypes.Keys)
                {
                    bool hasCommand = mAccess.unitManager.commandSets[type].Item2.TryGetValue(key, out groupCommand);
                    if (hasCommand)
                    {
                        if (groupCommand.Item2 == commandInstruction.Item2)
                        {
                            foreach (Guid unit in selectedTypes[type])
                            {
                                mAccess.unitManager.units[unit].sendCommand(com);
                            }
                        }
                    }
                }
            }
        }
    }
}