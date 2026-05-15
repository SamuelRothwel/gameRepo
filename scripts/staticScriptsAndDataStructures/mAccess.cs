using System.Runtime.InteropServices;
using coolbeats.scripts.managerScripts;
using Godot;

public static class mAccess
{
	public static sceneManagment sceneManager;
	public static LayerManagement layerManager;
	public static UIManagement uiManager;
	public static EntityManagement entityManager;
	public static LogicManagement logicManager;
	public static AnimationManagement animationManager;
	public static LifetimeManagement lifetimeManager;
	public static SpriteManagement spriteManager;
	public static GameManagement gameManager;
	public static DamageManagement damageManager;
	public static UnitManagement unitManager;
	public static TeamManagement teamManager;
	public static InputManagement inputManager;
	public static RecycleManagement recycleManager;
	public static SpriteCreatorManagement spriteCreatorManager;
	public static UnitCreatorManagement unitCreatorManager;
	public static ColorManagement colorManager;
	public static StyleManagement styleManager;
	public static FileManagement fileManager;
	public static EntityFrameworkManagement entityFrameworkManager;
	public static WindowManagement windowManager;

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
					logicManager = manager as LogicManagement;
					break;
				case "AnimationManager":
					animationManager = manager as AnimationManagement;
					break;
				case "LifetimeManager":
					lifetimeManager = manager as LifetimeManagement;
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
				case "RecyclerManager":
					recycleManager = manager as RecycleManagement;
					break;
				case "SpriteCreatorManager":
					spriteCreatorManager = manager as SpriteCreatorManagement;
					break;
				case "UnitCreatorManager":
					unitCreatorManager = manager as UnitCreatorManagement;
					break;
				case "ColorManager":
					colorManager = manager as ColorManagement;
					break;
				case "StyleManager":
					styleManager = manager as StyleManagement;
					break;
				case "FileManager":
					fileManager = manager as FileManagement;
					break;
				case "EntityFrameworkManager":
					entityFrameworkManager = manager as EntityFrameworkManagement;
					break;
				case "WindowManager":
					windowManager = manager as WindowManagement;
					break;
				default:
					GD.Print("invalid manager: " + manager.Name);
					break;
			}
		}
		lifetimeManager.setup();
		animationManager.setup();
		spriteManager.setup();
		damageManager.setup();
		logicManager.setup();
		sceneManager.setup();
		layerManager.setup();
		entityManager.setup();
		recycleManager.setup();
		fileManager.setup();
		entityFrameworkManager.setup();
		uiManager.setup();
		colorManager.setup();
		styleManager.setup();
		windowManager.setup();
		spriteCreatorManager.setup();
		unitCreatorManager.setup();
		gameManager.setup();
		unitManager.setup();
		teamManager.setup();
		inputManager.setup();
	}
}
