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
UnitManagement, TeamManagement, and InputManagement form the RTS-style unit selection and command system.
SpriteManagement, AnimationManagement, DamageManagement, LifetimeManagement, RecycleManagement, UIManagement, and CreatorManagement provide supporting systems.