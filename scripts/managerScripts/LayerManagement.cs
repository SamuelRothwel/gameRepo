using Godot;
using Godot.Collections;
using System;

public partial class LayerManagement : managerNode
{
	Dictionary<string, string> groupLayers;
	Dictionary<string, int> layers;
	Dictionary<string, string> groupMasks;
	Dictionary<string, int> collisionMasks;
	Dictionary<string, string> groupCollision;
	Dictionary<string, int[]> collisionLayers;
	public override void setup()
	{
		layers = new Dictionary<string, int>
		{
			{ "Background", 0 },
			{ "Ground", 1 },
			{ "Surface", 2 },
			{ "Object1", 3 },
			{ "Object2", 4 },
			{ "Object3", 5 },
			{ "SubUI", 6 },
			{ "UI1", 7 },
			{ "UI2", 8 },
			{ "UI3", 9 }
		};
		groupLayers = new Dictionary<string, string>
		{
			{ "trail", "Surface" },
			{ "wheel", "Object1" },
			{ "car", "Object2" },
			{ "inGameUI", "UI1" },
			{ "camera", "UI3" },
			{ "pen", "SubUI" },
			{ "bullet", "Object2" }
		};
		groupMasks = new Dictionary<string, string>
		{
			
		};
		collisionMasks = new Dictionary<string, int>
		{
			{ "environmentalHit", 1 },
			{ "enemyHit", 2 },
			{ "playerHit", 3 },
			{ "enemyDetect", 4 }
		};
		groupCollision = new Dictionary<string, string>
		{
			{ "enemyCrawler", "enemy" }
		};
		collisionLayers = new Dictionary<string, int[]>
		{
			{ "player", new int[] {1, 2, 4} },
			{ "enemy", new int[] {1, 3} },
		};
	}
	
	public void addLayer(CanvasItem node)
	{
		try
		{
			string group = node.GetGroups()[0];
			if (int.TryParse(group, out int i))
			{
				node.ZIndex = i;
			}
			else
			{
				node.ZAsRelative = false;
				node.ZIndex = layers[groupLayers[group.ToString()]];
			}
		}
		catch
		{
			var groups = node.GetGroups();
			if (groups.Count < 1)
			{
				//GD.Print(node + " is not assigned to group");
			}
			else
			{
				//GD.Print(node.GetGroups()[0] + " does not have assigned layer");
			}
		}
	}
}
