using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public static class math
{
	public static float PI = 3.141592653589793f;
	public static Random rand = new Random();
	public static (float, float, float, float) getMinMax(Vector2 p1, Vector2 p2)
	{
		return (MathF.Max(p1.X, p2.X), MathF.Min(p1.X, p2.X), MathF.Max(p1.Y, p2.Y), MathF.Min(p1.Y, p2.Y));
	}
	public static (float, float, float, float) getMinMax(Vector2 p, float radius)
	{
		return (p.X + radius, p.X - radius, p.Y + radius, p.Y - radius);
	}
	public static Vector2 createVector((int, int) point)
	{
		return new Vector2(point.Item1, point.Item2);
	}
	public static Vector2 createVector(float velocity, float angle, bool degrees = false)
	{
		if (degrees)
		{
			angle = angle / 180 * PI;
		}
		return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * velocity;
	}
	public static float randomFloat()
	{
		return (float)rand.NextDouble();
	}
	public static float randomFloat(double max)
	{
		return (float)(rand.NextDouble() * max);
	}
	public static float randomFloat(double min, double max)
	{
		return (float)(rand.NextDouble() * (max-min) + min);
	}
}
