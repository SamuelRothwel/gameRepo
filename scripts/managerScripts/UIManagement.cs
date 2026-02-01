using Godot;
using System;
using System.Collections.Generic;

public partial class UIManagement : managerNode
{
	Node CurrentUI;
	public Dictionary<string, PackedScene> scenes = new Dictionary<string, PackedScene>();
	[Export] public PackedScene UIScene;
	
	public override void setup()
	{
		Godot.Collections.Array<Node> entities = UIScene.Instantiate().GetChildren();
		foreach (Node entity in entities)
		{
			PackedScene packedEntity = new PackedScene();
			packedEntity.Pack(entity);
			scenes.Add(entity.Name, packedEntity);
		}
		changeUI("main");
	}
	public void changeUI(string name)
	{
		if (CurrentUI != null)
		{
			CurrentUI.QueueFree();
		}
		GD.Print();
		CurrentUI = scenes[name].Instantiate();
		AddChild(CurrentUI);
	}
}
