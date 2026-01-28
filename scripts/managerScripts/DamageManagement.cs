using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DamageManagement : managerNode
{
	// colour health resistance type
	Dictionary<int, (int, int)>  colorValues;
    Dictionary<string, Dictionary<string, (int, int)[][,]>> healthMaps;
    public override void setup()
	{
		colorValues = new Dictionary<int, (int, int)>();
		for (int i = 0; i < 10; i++)
		{
			colorValues[i] = (1, 1);
		}
		healthMaps = mAccess.spriteManager.spriteMaps
		.ToDictionary(x => x.Key, x => x.Value
		.ToDictionary(y => y.Key, y => y.Value
		.Select(z => z.Select(u => colorValues[u])).ToArray()));
	}
	public (int, int)[,] GetHealthMap(string name)
	{
		return healthMaps[name].Values.First().First();
	}
}
