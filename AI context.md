It’s a Godot 4.2 C# project using an autoloaded manager scene:

project.godot autoloads Scenes/setScenes/managers.tscn as Managers.
Scenes/setScenes/managers.tscn contains manager nodes in the admin group.
sceneManagment._EnterTree() calls mAccess.setup(GetTree().GetNodesInGroup("admin")).
mAccess is the static service locator that stores references like mAccess.unitManager, mAccess.entityManager, mAccess.logicManager, etc.
Each manager inherits from [managerNode.cs](C:/Users/Samuel/Documents/cool beats - Copy/scripts/managerScripts/managerNode.cs:4), whose main contract is setup().
The core flow looks like this:

LogicManagement loads background logic from Scenes/setScenes/logic.tscn, registers creation flags, and preprocesses entity scenes.
EntityManagement loads packed entities from Scenes/setScenes/entities.tscn, preprocesses them through logic, then acts as a factory via spawnEntity() / getEntity().
sceneManagment tracks all nodes added to the tree, applies layers, runs creation logic, and attaches animation libraries to AnimationPlayers.
sceneManagment.gameStates is a FallbackDictionary whose default is menu, so non-menu states only list true/overridden flags; omitted flags fall back to menu's false values.
UnitManagement, TeamManagement, and InputManagement form the RTS-style unit selection and command system.
SpriteManagement, AnimationManagement, DamageManagement, LifetimeManagement, RecycleManagement, UIManagement, and SpriteCreatorManagement provide supporting systems.

Testing practice:
All project code that should be unit tested lives under scripts/.
Unit tests should be written with xUnit.
Tests should live in a test folder inside the same directory as the code they test, so each scripts subdirectory keeps its own nearby tests.
Tests must be run separately from the Godot game/runtime flow and must not change runtime behavior or project functionality.
When adding or changing testable code, identify and prompt for relevant edge cases so they can be covered explicitly.

Responsibilities and manager dependency order:
Managers should keep data/model ownership separate from presentation ownership. Lower-level managers may expose state and operations, while higher-level managers compose UI or scene-facing behavior.

Recommended setup/dependency order:
1. Core runtime services: lifetime, animation, sprite, damage, logic, scene, layer, entity, recycle, file.
2. Persistence and data services: EntityFrameworkManagement owns database setup, stored sprites, stored units, animations, and saved game settings data.
3. ColorManagement owns named colors, saved/recent color lists, active color state, and color picker math only. It should not instantiate UI scenes or open windows.
4. StyleManagement owns universal fonts, text styles, button/panel/window styles, and style refresh. It may read saved appearance data after persistence is ready.
5. WindowManagement owns managed in-game windows and positioning.
6. UIManagement owns UI scene packing/switching, application chrome/topbar, safe-area reservation, and UI windows such as the color picker.
7. Feature managers such as SpriteCreatorManagement and UnitCreatorManagement coordinate feature workflows and call lower-level services, but should avoid owning global style, color, window, or persistence infrastructure.

Dependency rule of thumb:
Data managers should not depend on UI managers. UI managers can depend on data managers to display or edit their state. If a manager needs to show a window, prefer placing that window creation in UIManagement or WindowManagement and keep the original manager focused on state changes.
