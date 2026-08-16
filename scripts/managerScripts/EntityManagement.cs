using coolbeats.scripts.logicScripts.BackgroundLogic;
using coolbeats.scripts.logicScripts.Bases;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class EntityManagement : managerNode
{
	[Export] public PackedScene entitiesScene;
	public Dictionary<string, PackedScene> packedEntities = new Dictionary<string, PackedScene>();
	public Dictionary<string, Dictionary<string, Image[]>> spriteImages;
	public override void setup()
	{
		Godot.Collections.Array<Node> entities = entitiesScene.Instantiate().GetChildren();
		foreach (Node entity in entities)
		{
			PackedScene packedEntity = new PackedScene();
			mAccess.logicManager.preProcess(entity);
			packedEntity.Pack(entity);
			packedEntities.Add(entity.Name, packedEntity);
		}
		spriteImages = fileSearch.getImages();
	}
	public Node spawnEntity(string name)
	{
		Node newEntity = packedEntities[name].Instantiate();
		Node parent = mAccess.gameSessionManager?.Current?.WorldRoot ?? this;
		parent.AddChild(newEntity);
		return newEntity;
	}
	public Node getEntity(string name)
	{
		Node newEntity = packedEntities[name].Instantiate();
		return newEntity;
	}
	public void defferedAddChild(Node child, Node parent)
	{
		CallDeferred(nameof(addChild), child, parent);
	}
	public void addChild(Node child, Node parent)
	{
		if (!GodotObject.IsInstanceValid(child) || !GodotObject.IsInstanceValid(parent) || child.IsQueuedForDeletion())
		{
			return;
		}
		if (child.GetParent() != null)
		{
			return;
		}
		parent.AddChild(child);
	}
}
