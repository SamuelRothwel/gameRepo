using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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
        List<Guid> selectedUnits = new List<Guid>();
        Dictionary<string, List<Guid>> selectedTypes;
        public event EventHandler unitSelected;
		public int UnitSelectedSubscriberCount => unitSelected?.GetInvocationList().Length ?? 0;
        string _selectedType;
        public string selectedType {get {return _selectedType;} set {_selectedType = value; unitSelected?.Invoke(this, new EventArgs());}}
        team activeTeam;
        Key activeKey = Key.None;
        float minZoom = 0.5f;
        float maxZoom = 5.5f;
        float targetZoom;
        float currentZoom;
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
            currentZoom = 1;
            targetZoom = currentZoom;
        }
        public override void _Process(double delta)
        {
            // Cameras belong to a game session.  Once that session is closed,
            // its camera has been freed and must not be used by this manager.
            if (camera == null || !GodotObject.IsInstanceValid(camera))
            {
                return;
            }

            if (currentZoom < targetZoom)
            {
                currentZoom = MathF.Min((float)(currentZoom + ((targetZoom - currentZoom)*0.5f + 5)*delta), targetZoom);
                setCameraScale(currentZoom);
            } else if (currentZoom > targetZoom)
            {
                currentZoom = MathF.Max((float)(currentZoom + ((targetZoom - currentZoom)*2 - 5)*delta), targetZoom);
                setCameraScale(currentZoom);
            }
        }
        public void setCamera(Camera2D cam)
        {
            camera = cam;
            // Game teams are loaded per session; replace the team instance from
            // the previous run before selection commands are processed.
            activeTeam = mAccess.teamManager?.teams != null && mAccess.teamManager.teams.Length > 0
                ? mAccess.teamManager.teams[0]
                : null;
            selectedUnits.Clear();
            selectedTypes = new Dictionary<string, List<Guid>>();
            _selectedType = "";
            activeKey = Key.None;
            selecting = false;
            dragging = false;

            // The pen is parented to the session camera and is therefore freed
            // with the scene.  Recreate it when a new scene supplies its camera.
            if (pen == null || !GodotObject.IsInstanceValid(pen))
            {
                pen = mAccess.entityManager.getEntity("pen") as pen;
            }

            if (pen != null && pen.GetParent() != camera)
            {
                pen.GetParent()?.RemoveChild(pen);
                camera.AddChild(pen);
            }

            cameraOffset = new Vector2(-640, -360);    
        }
        public void sessionStopped(GameSession session)
        {
            bool cameraWasInSession = camera != null && GodotObject.IsInstanceValid(camera) && session.WorldRoot.IsAncestorOf(camera);
            bool penWasInSession = pen != null && GodotObject.IsInstanceValid(pen) && session.WorldRoot.IsAncestorOf(pen);
            if (cameraWasInSession)
            {
                camera = null;
            }
            if (penWasInSession)
            {
                pen = null;
            }
            if (cameraWasInSession || penWasInSession)
            {
                activeTeam = null;
                selectedUnits.Clear();
                selectedTypes = new Dictionary<string, List<Guid>>();
                _selectedType = "";
                activeKey = Key.None;
                selecting = false;
                dragging = false;
            }
        }
        public void zoomCamera()
        {
        }
        public void setCameraScale(float scale)
        {
            cameraOffset.X = -640 / scale;
            cameraOffset.Y = -360 / scale;     
            camera.Zoom = new Vector2(scale, scale);
        }
        public Vector2 scaleCoords(Vector2 coords)
        {
            coords = getGameWindowPosition(coords);
            return coords/camera.Zoom + cameraOffset + camera.Position;
        }
        public void SetActiveTeam(int teamIndex)
        {
            if (mAccess.teamManager?.teams == null || teamIndex < 0 || teamIndex >= mAccess.teamManager.teams.Length)
            {
                activeTeam = null;
                return;
            }

            activeTeam = mAccess.teamManager.teams[teamIndex];
        }

        // These methods are the game-type-facing form of the normal selection and target-command flow.
        // UI input calls the same command dispatch below; gameplay tests can use them without mouse coordinates.
        public bool TrySelectPlayerUnits(IEnumerable<Guid> unitIds)
        {
            return setSelectedUnits(unitIds);
        }

        // Coordinates are in game-world space.  Mouse input and gameplay tests
        // both use this method so they exercise exactly the same selection path.
        public bool TryBoxSelectPlayerUnits(Vector2 firstCorner, Vector2 secondCorner)
        {
            if (!prepareActiveTeam()) return false;

            // A scene can be clicked before TeamManagement's next process tick.
            // Build the spatial index now so first-frame selection is valid.
            mAccess.teamManager.UpdateTeamVisions();
            List<Guid> candidates = new List<Guid>();
            mAccess.teamManager.searchBVH(activeTeam.BVH, ref candidates, math.getMinMax(firstCorner, secondCorner));
            return setSelectedUnits(candidates);
        }

        public bool BeginBoxSelection(Vector2 windowPosition)
        {
            if (!HasGameCapability("unitControl") || camera == null || !GodotObject.IsInstanceValid(camera)) return false;
            startSelect = windowPosition;
            selecting = true;
            return true;
        }

        public void UpdateBoxSelection(Vector2 windowPosition)
        {
            if (!selecting || !HasGameCapability("unitControl") || pen == null || !GodotObject.IsInstanceValid(pen)) return;
            pen.drawRectangle(scaleLocalCoords(startSelect), scaleLocalCoords(windowPosition));
        }

        public bool CompleteBoxSelection(Vector2 windowPosition)
        {
            if (!selecting) return false;
            selecting = false;
            bool selected = TryBoxSelectPlayerUnits(scaleCoords(startSelect), scaleCoords(windowPosition));
            if (pen != null && GodotObject.IsInstanceValid(pen)) pen.erase();
            return selected;
        }

        public bool SelectionOverlayVisible => pen != null && GodotObject.IsInstanceValid(pen) && pen.HasRectangle;
		public Color SelectionOverlayColor => pen != null && GodotObject.IsInstanceValid(pen)
			? pen.VisibleColor
			: new Color(0, 0, 0, 0);

        bool prepareActiveTeam()
        {
            if (!HasGameCapability("unitControl")) return false;
            SetActiveTeam(0);
            return activeTeam != null;
        }

        bool setSelectedUnits(IEnumerable<Guid> unitIds)
        {
            if (!prepareActiveTeam()) return false;

            foreach (Guid id in selectedUnits)
            {
                if (mAccess.unitManager.units.TryGetValue(id, out unitControler oldUnit) && GodotObject.IsInstanceValid(oldUnit))
                {
                    oldUnit.selected = false;
                }
            }

            selectedUnits = unitIds
                .Distinct()
                .Where(id => activeTeam.units.Contains(id) &&
                    mAccess.unitManager.units.TryGetValue(id, out unitControler unit) && GodotObject.IsInstanceValid(unit))
                .ToList();
            selectedTypes = new Dictionary<string, List<Guid>>();
            string newSelectedType = "";
            int maxPriority = int.MinValue;
            foreach (Guid id in selectedUnits)
            {
                unitControler unit = mAccess.unitManager.units[id];
                unit.selected = true;
                if (!selectedTypes.TryGetValue(unit.type, out List<Guid> unitsOfType))
                {
                    unitsOfType = new List<Guid>();
                    selectedTypes[unit.type] = unitsOfType;
                }
                unitsOfType.Add(id);
                if (unit.priority >= maxPriority)
                {
                    maxPriority = unit.priority;
                    newSelectedType = unit.type;
                }
            }

            selectedType = newSelectedType;
            return selectedUnits.Count > 0;
        }

        public bool TryIssuePlayerTargetCommand(Key key, Vector2 position)
        {
            if (!HasGameCapability("unitControl") || selectedTypes == null || selectedTypes.Count == 0 ||
                string.IsNullOrEmpty(selectedType) || !mAccess.unitManager.commandSets.TryGetValue(selectedType, out var commandSet) ||
                !commandSet.Item2.TryGetValue(key, out var commandInstruction) || string.IsNullOrEmpty(commandInstruction.Item2))
            {
                return false;
            }

            command com = new command(commandInstruction.Item2);
            getTargets(commandInstruction, ref com, position);
            sendCommands(com, commandInstruction, key);
            return true;
        }
        public Vector2 scaleLocalCoords(Vector2 coords)
        {
            coords = getGameWindowPosition(coords);
            return coords/camera.Zoom + cameraOffset;
        }
		public Vector2 worldToWindowCoords(Vector2 worldCoordinates)
		{
			Vector2 gameWindowCoordinates = (worldCoordinates - cameraOffset - camera.Position) * camera.Zoom;
			return mAccess.uiManager == null
				? gameWindowCoordinates
				: gameWindowCoordinates + new Vector2(0, mAccess.uiManager.getReservedTopbarHeight());
		}
        Vector2 getGameWindowPosition(Vector2 coords)
        {
            return mAccess.uiManager == null ? coords : mAccess.uiManager.toGameWindowPosition(coords);
        }
		bool HasGameCapability(string capability)
		{
			return mAccess.sceneManager != null && mAccess.sceneManager.HasGameCapability(capability);
		}
        public override void _UnhandledInput(InputEvent inp)
        {
            
            if (inp is InputEventMouseMotion motion)
            {
                if (dragging)
                {
					if (HasGameCapability("moveCamera"))
                    {
                        camera.Position -= motion.Relative/camera.Zoom;
                    }
                }
                if (selecting)
                {
					if (HasGameCapability("unitControl"))
                    {
                        UpdateBoxSelection(motion.GlobalPosition);
                    }
                }
            }
            else if (inp is InputEventMouseButton mouse)
            {
                switch (mouse.ButtonIndex)
                {
                    case MouseButton.Left:
						// A left-button release always finishes an in-progress selection,
						// even if a command key became active while dragging.
						if (!mouse.Pressed && selecting)
						{
							CompleteBoxSelection(mouse.GlobalPosition);
							break;
						}
                        if (activeKey != Key.None)
                        {
                            if (mouse.Pressed)
                            {
								if (HasGameCapability("unitControl"))
                                {
                                    sendTargetCommand(scaleCoords(mouse.GlobalPosition));
                                    activeKey = Key.None;
								} else if (HasGameCapability("draw"))
                                {
                                    
                                }
                            }
                        } else
                        {
                            if (mouse.Pressed)
                            {
								if (HasGameCapability("unitControl"))
                                {
									BeginBoxSelection(mouse.GlobalPosition);
								} else if (HasGameCapability("draw"))
                                {
                                    mAccess.spriteCreatorManager.click(scaleCoords(mouse.GlobalPosition));
                                }
							}
                        }
                        break;
                    case MouseButton.Right:
                        if (mouse.Pressed)
                        {
							if (HasGameCapability("unitControl"))
                            {
                                activeKey = Key.None;
                                sendTargetCommand(scaleCoords(mouse.GlobalPosition));
                            }
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
						if (HasGameCapability("moveCamera"))
                        {
                            targetZoom = MathF.Min(targetZoom + 0.5f, maxZoom);
                        }
                        break;
                    case MouseButton.WheelDown:
						if (HasGameCapability("moveCamera"))
                        {
                            targetZoom = MathF.Max(targetZoom - 0.5f, minZoom);
                        }
                        break;
                }
            }
            else if (inp is InputEventKey key)
            {
                if (string.IsNullOrEmpty(selectedType) || !mAccess.unitManager.commandSets.ContainsKey(selectedType)) return;
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
        public void sendTargetCommand(Vector2 position)
        {
            TryIssuePlayerTargetCommand(activeKey, position);
        }
        public void getTargets((string[], string) commandInstruction, ref command com, Vector2 position)
        {
            List<Guid> targets = new List<Guid>();
            foreach (string target in commandInstruction.Item1)
            {
                switch (target)
                {
                    case "ground":
                        com.coordinates = position;
                        break;
                    case "team":
                        mAccess.teamManager.searchBVH(activeTeam.BVH, ref targets, math.getMinMax(position, 0));
                        break;
                    case "ally":
                        foreach (team ally in activeTeam.allies)
                        {
                            mAccess.teamManager.searchBVH(ally.BVH, ref targets,  math.getMinMax(position, 0));
                        }
                        break;
                    case "enemy":
                        foreach (team enemy in activeTeam.enemies)
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
