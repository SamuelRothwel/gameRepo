using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public partial class LogicManagement : managerNode
{
	[Export] public PackedScene packedLogicScene;
	public Node logicScene;
	public Dictionary<string, BackgroundLogicNode> backgroundLogic;
	public Dictionary<string, List<BackgroundLogicNode>> creationFlags;
	public override void setup()
	{
		logicScene = packedLogicScene.Instantiate();
		AddChild(logicScene);

		backgroundLogic = new Dictionary<string, BackgroundLogicNode>();
		creationFlags = new Dictionary<string, List<BackgroundLogicNode>>();

		foreach (Node node in logicScene.GetChildren())
		{
			foreach (BackgroundLogicNode script in node.GetChildren())
			{
				string logicType = script.Name;
				script.setup();
				backgroundLogic.Add(logicType, script);
				foreach (string flag in script.creationFlags)
				{
					if (!creationFlags.ContainsKey(flag))
					{
						creationFlags.Add(flag, new List<BackgroundLogicNode>());
					}
					creationFlags[flag].Add(script);
				}
			}
		}
	}
	public void preProcess(Node scene)
	{
		List<Node> nodes = mAccess.sceneManager.GetNodeList(scene);
		for (int i = 0; i < nodes.Count(); i++)
		{
			foreach (string group in nodes[i].GetGroups())
			{
				if (creationFlags.ContainsKey(group))
				{
					foreach (BackgroundLogicNode logicNode in creationFlags[group])
					{
						logicNode.preProcess(nodes[i]);
					}
				}
			}
		}
	}
	public void entityCreation(Node node)
	{
		foreach (string group in node.GetGroups())
		{
			if (creationFlags.ContainsKey(group))
			{
				foreach (BackgroundLogicNode logicNode in creationFlags[group])
				{
					logicNode.creationFlag(node);
				}
			}
		}
	}
}
