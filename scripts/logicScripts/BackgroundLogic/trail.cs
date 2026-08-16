using Godot;
using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class trail : BackgroundLogicNode
{
	readonly List<Node> wheels = new List<Node>();
	readonly List<Line2D> lines = new List<Line2D>();
	int maxPoints = 10;
	public override void setup()
	{
		creationFlags = new string[] {"wheel"};
	}

	public override void creationFlag(Node node)
	{
		GameSession session = mAccess.gameSessionManager?.Current;
		if (session == null || node is not Node2D || !session.WorldRoot.IsAncestorOf(node))
		{
			return;
		}

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

	public override void sessionStopped(GameSession session)
	{
		for (int i = wheels.Count - 1; i >= 0; i--)
		{
			Node wheel = wheels[i];
			if (GodotObject.IsInstanceValid(wheel) && session.WorldRoot.IsAncestorOf(wheel))
			{
				removeTrailAt(i);
			}
		}
	}

	public override void _Process(double delta)
	{
		for (int i = wheels.Count - 1; i >= 0; i--)
		{
			Node current = wheels[i];
			Line2D line = lines[i];
			if (!GodotObject.IsInstanceValid(current) || !GodotObject.IsInstanceValid(line) || current is not Node2D node2D)
			{
				removeTrailAt(i);
				continue;
			}

			Vector2 coordinates = node2D.GlobalPosition;
			line.AddPoint(coordinates);
			if (line.Points.Length > maxPoints)
			{
				line.RemovePoint(0);
			}
		}
	}

	void removeTrailAt(int index)
	{
		Line2D line = lines[index];
		if (GodotObject.IsInstanceValid(line) && !line.IsQueuedForDeletion())
		{
			line.QueueFree();
		}
		wheels.RemoveAt(index);
		lines.RemoveAt(index);
	}
}
