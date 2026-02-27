

/*



using Godot;
using System;
using System.Collections.Generic;
using coolbeats.scripts.logicScripts.Bases;
using coolbeats.scripts.logicScripts.BackgroundLogic;

1 everything
2 walls
3 enemies
4 player

public partial class depProjectile : CharacterBody2D, timedObject
{
	public static Dictionary<string, deprecatedProjectile> projectiles;
	public Vector2 directionalVelocity;
	public string type;
	public bool active;
	public CollisionShape2D collider;
	public Line2D trail;
	public Vector2 trailCoordinate;
	public bool trailMoving;
	public int trailCount;
	public List<(timedFlag, Node)> exceptions;
	public Vector2 distance;
	public void setup()
	{
		active = false;
		TopLevel = true;
		collider = GetChild<CollisionShape2D>(0);
		trail = GetNode<Line2D>("trail");
	}
	public override void _EnterTree()
	{
		trailCoordinate = this.GlobalPosition;
		trailMoving = false;
    }
	public void remove()
	{
		SetProcess(false);
		foreach (Node exception in GetCollisionExceptions())
		{
			RemoveCollisionExceptionWith(exception);
		}
		active = false;
		mAccess.lifetimeManager.Remove(this);
	}
	public void fire(string newType, uint target = 6)
	{
		active = true;
		CollisionMask = target;
		type = newType;
		collider.Shape = projectiles[type].shape;
		collider.Shape.CustomSolverBias = 0;
		trailCount = projectiles[type].trailColour.Length;
		trail.Gradient = new Gradient();
		for (int i = 0; i < trailCount; i++)
		{
			trail.Gradient.SetColor(i / trailCount, projectiles[type].trailColour[i]);
		}
		directionalVelocity = math.createVector(projectiles[type].velocity, Rotation);
		mAccess.lifetimeManager.queueTimer(this, projectiles[type].lifetime);
		SetProcess(true);
	}
	public void timeout()
	{
		remove();
	}
	public void removeException(Node node)
	{
		RemoveCollisionExceptionWith(node);
	}
	public override void _Process(double delta)
	{
		foreach (Node exception in GetCollisionExceptions())
		{
			RemoveCollisionExceptionWith(exception);
		}
		if (trailMoving)
		{
			trailCoordinate += directionalVelocity * (float)delta;
		}
		else if (Math.Abs((trailCoordinate - GlobalPosition).Length()) > projectiles[type].trailLength)
		{
			trailMoving = true;
		}
		distance = directionalVelocity * (float)delta;
		while (true)
		{
			KinematicCollision2D collision = MoveAndCollide(distance, true);
			if (collision != null && active == true)
			{
				distance = collision.GetRemainder();
				GlobalPosition += distance - collision.GetRemainder();
				GodotObject collided = collision.GetCollider();
				AddCollisionExceptionWith((Node)collided);
				projectiles[type].collision(this, collided);
			}	
			else { GlobalPosition += distance; break; }	
		}
	}
}*/