using Godot;
using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class trail : BackgroundLogicNode
{
	Godot.Collections.Array<Node> wheels = new Godot.Collections.Array<Node>();
	List<Line2D> lines = new List<Line2D>();
	int maxPoints = 10;
	public override void setup()
	{
		creationFlags = new string[] {"wheel"};
	}

	public override void creationFlag(Node node)
	{
		wheels.Add(node);
		Line2D line = new Line2D
		{
			Width = 10,
			Antialiased = true,
			JointMode = Line2D.LineJointMode.Round,
			Gradient = new Gradient(),
			TopLevel = true
		};
		line.Gradient.SetColor(0, new Color(0, 0, 0, 0));
		line.Gradient.SetColor(1, new Color(0, 0, 0, 1));
		lines.Add(line);
		line.AddToGroup("trail");

		mAccess.entityManager.defferedAddChild(line, node);
    }

	public override void _Process(double delta)
	{
		for (int i = 0; i < wheels.Count; i++)
		{
			Node current = wheels[i];
			if (current is Node2D node2D)
			{
				Vector2 coordinates = node2D.GlobalPosition;
				lines[i].AddPoint(coordinates);
				if (lines[i].Points.Length > maxPoints)
				{
					lines[i].RemovePoint(0);
				}
			}
		}
	}
}
