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
	public bool inGame = false;
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
    }

	public void startGame()
	{
		GetTree().ChangeSceneToPacked(gameScene);
		inGame = true;
		mAccess.unitManager.createUnit("marine", 0);
		mAccess.unitManager.createUnit("marine", 1);
		mAccess.uiManager.changeUI("game");
		mAccess.entityManager.spawnEntity("player");
		mAccess.entityManager.spawnEntity("playerCamera");
		
        mAccess.teamManager.UpdateTeamVisions();
	}
	public void startMenu()
	{
		GetTree().ChangeSceneToPacked(menuScene);
		inGame = false;
		mAccess.uiManager.changeUI("main");
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
		mAccess.logicManagement.entityCreation(node);
		if (node is AnimationPlayer player)
		{
			foreach (string group in node.GetGroups())
			{
				player.AddAnimationLibrary(group, mAccess.animationManagement.animationSets[group]);
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
