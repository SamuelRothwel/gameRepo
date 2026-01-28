using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        string selectedType;
        int team = 0;
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
            cameraOffset.X = -scale * 640;
            cameraOffset.Y = -scale * 360;     
            camera.Zoom = new Vector2(scale, scale);
        }
        public Vector2 scaleCoords(Vector2 coords)
        {
            return coords*camera.Scale + cameraOffset;
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
                            if (mouse.Pressed)
                            {
                                startSelect = mouse.Position;
                                selecting = true;
                            } else
                            {
                                selecting = false;
                                mAccess.teamManager.UpdateTeamVisions();
                                (float, float, float, float) minMax = math.getMinMax(scaleCoords(startSelect), scaleCoords(mouse.Position));
                                mAccess.teamManager.searchBVH(mAccess.teamManager.teams[team].BVH, ref selectedUnits, minMax.Item1, minMax.Item2, minMax.Item3, minMax.Item4);
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
                            break;
                        case MouseButton.Right:

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
                    }
                }
                else if (inp is InputEventKey key)
                {
                    (bool, string) commandInstruction;
                    bool hasCommand = mAccess.unitManager.commandSets[selectedType].Item2.TryGetValue(key.Keycode, out commandInstruction);
                    if (hasCommand)
                    {
                        if (commandInstruction.Item1)
                        {
                            command com = new command(commandInstruction.Item2);
                            sendCommands(com, commandInstruction, key.Keycode);
                        }
                    }
                }
            }
        }
        public void sendCommands(command com, (bool, string) commandInstruction, Key key)
        {
            (bool, string) groupCommand;
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