using coolbeats.scripts.managerScripts;
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

public partial class sceneManagment : managerNode
{
	[Export] public PackedScene menuScene;
	[Export] public PackedScene gameScene;
	public List<Node> sceneNodes;
	public Node gameNode;
	public FallbackDictionary<string, bool> gameStates;
	public override void _EnterTree()
	{
		mAccess.setup(GetTree().GetNodesInGroup("admin"));
	}

    public override void _Ready()
    {
		sceneNodes = GetNodeList(GetTree().Root);
		for (int i = 0; i < sceneNodes.Count; i++)
		{
			InitialiseNode(sceneNodes[i]);
		}
		GetTree().NodeAdded += InitialiseNode;
		GetTree().NodeRemoved += TerminateNode;
		gameStates = new FallbackDictionary<string, bool>();
		gameStates.Add("menu", new Dictionary<string, bool>
		{
			{"gameActive", false},
			{"moveCamera", false},
			{"unitControl", false},
			{"draw", false},
		});
		gameStates.Add("gameActive", new Dictionary<string, bool>
		{
			{"gameActive", true},
		});
		gameStates.Add("inGame", new Dictionary<string, bool>
		{
			{"moveCamera", true},
			{"unitControl", true},
		}, "gameActive");
		gameStates.Add("spriteCreator", new Dictionary<string, bool>
		{
			{"moveCamera", true},
			{"draw", true},
		});
		gameStates.Add("unitCreator", new Dictionary<string, bool>
		{
			{"moveCamera", true},
		});
		gameStates.SetDefault("menu");
    }

	public void startGame()
	{
		changeSceneAfterUnsavedCheck(() =>
		{
			GetTree().ChangeSceneToPacked(gameScene);
			gameStates.Switch("inGame");
			//mAccess.unitManager.createUnit("marine", 0);
			mAccess.unitManager.createUnit("marine", 0);
			//mAccess.unitManager.createUnit("barracks", 0);
			mAccess.unitManager.createUnit("marine", 1);
			mAccess.uiManager.changeUI("game");
			mAccess.entityManager.spawnEntity("playerCamera");
		});
	}
	public void startMenu()
	{
		changeSceneAfterUnsavedCheck(() =>
		{
			GetTree().ChangeSceneToPacked(menuScene);
			gameStates.Switch("menu");
			mAccess.uiManager.changeUI("main");
		});
	}
	public void spriteCreator()
	{
		changeSceneAfterUnsavedCheck(() =>
		{
			GetTree().ChangeSceneToPacked(menuScene);
			gameStates.Switch("spriteCreator");
			mAccess.uiManager.changeUI("spriteCreator");
			mAccess.entityManager.spawnEntity("playerCamera");
		});
	}
	public void unitCreator()
	{
		changeSceneAfterUnsavedCheck(() =>
		{
			GetTree().ChangeSceneToPacked(menuScene);
			gameStates.Switch("unitCreator");
			mAccess.uiManager.changeUI("unitCreator");
			mAccess.entityManager.spawnEntity("playerCamera");
		});
	}

	void changeSceneAfterUnsavedCheck(Action changeScene)
	{
		if (mAccess.entityFrameworkManager == null)
		{
			changeScene();
			return;
		}

		mAccess.entityFrameworkManager.CheckUnsavedObjects(canChange =>
		{
			if (canChange)
			{
				changeScene();
			}
		});
	}

	private void TerminateNode(Node node)
	{
		sceneNodes.Remove(node);
	}

	private void InitialiseNode(Node node)
	{
		if (node is CanvasItem canvasItem)
		{
			mAccess.layerManager.addLayer(canvasItem);
		}
		mAccess.logicManager.entityCreation(node);
		if (node is AnimationPlayer player)
		{
			foreach (string group in node.GetGroups())
			{
				player.AddAnimationLibrary(group, mAccess.animationManager.animationSets[group]);
			}
		}
	}
	
	public List<Node> GetNodeList(Node root)
	{
		List<Node> result = new();
		Traverse(root, result);
		return result;
	}

	private void Traverse(Node current, List<Node> result)
	{
		result.Add(current);

		foreach (Node child in current.GetChildren())
		{
			Traverse(child, result);
		}
	}
}
