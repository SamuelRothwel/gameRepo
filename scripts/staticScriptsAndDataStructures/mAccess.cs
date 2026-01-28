using System.Runtime.InteropServices;
using coolbeats.scripts.managerScripts;
using Godot;

public static class mAccess
{
	public static sceneManagment sceneManager;
	public static LayerManagement layerManager;
	public static UIManagement uiManager;
	public static EntityManagement entityManager;
	public static LogicManagement logicManagement;
	public static AnimationManagement animationManagement;
	public static LifetimeManagement lifetimeManager;
	public static ProjectileManagement projectileManager;
	public static SpriteManagement spriteManager;
	public static GameManagement gameManager;
	public static DamageManagement damageManager;
	public static UnitManagement unitManager;
	public static TeamManagement teamManager;
	public static InputManagement inputManager;
	//public static PlayerManagement playerManager;

	public static void setup(Godot.Collections.Array<Node> managers)
	{
		foreach (Node manager in managers)
		{
			switch (manager.Name.ToString())
			{
				case "LayerManager":
					layerManager = manager as LayerManagement;
					break;
				case "UIManager":
					uiManager = manager as UIManagement;
					break;
				case "SceneManager":
					sceneManager = manager as sceneManagment;
					break;
				case "EntityManager":
					entityManager = manager as EntityManagement;
					break;
				case "LogicManager":
					logicManagement = manager as LogicManagement;
					break;
				case "AnimationManager":
					animationManagement = manager as AnimationManagement;
					break;
				case "LifetimeManager":
					lifetimeManager = manager as LifetimeManagement;
					break;
				case "ProjectileManager":
					projectileManager = manager as ProjectileManagement;
					break;
				case "SpriteManager":
					spriteManager = manager as SpriteManagement;
					break;
				case "GameManager":
					gameManager = manager as GameManagement;
					break;
				case "DamageManager":
					damageManager = manager as DamageManagement;
					break;
				case "UnitManager":
					unitManager = manager as UnitManagement;
					break;
				case "TeamManager":
					teamManager = manager as TeamManagement;
					break;
				case "InputManager":
					inputManager = manager as InputManagement;
					break;
				//case "PlayerManager":
				//	playerManager = manager as PlayerManagement;
				//	break;
				default:
					GD.Print("invalid manager: " + manager.Name);
					break;
			}
		}
		lifetimeManager.setup();
		animationManagement.setup();
		spriteManager.setup();
		//playerManager.setup();
		damageManager.setup();
		logicManagement.setup();
		sceneManager.setup();
		layerManager.setup();
		uiManager.setup();
		entityManager.setup();
		projectileManager.setup();
		gameManager.setup();
		unitManager.setup();
		teamManager.setup();
		inputManager.setup();
	}
}
