using Godot;
using System;

public partial class pen : Node2D
{
	public static readonly Color SelectionColor = new Color("#00ff0066");
	Rect2 rectangle = new Rect2();
	public bool HasRectangle => rectangle.Size.LengthSquared() > 0.001f;
	public Color VisibleColor => HasRectangle ? SelectionColor : new Color(0, 0, 0, 0);
	public void drawRectangle(Rect2 rect)
	{
		rectangle = rect;
		QueueRedraw();
	}
	public void drawRectangle(Vector2 p1, Vector2 p2) {
		float minX = MathF.Min(p1.X, p2.X);
		float minY = MathF.Min(p1.Y, p2.Y);
		drawRectangle(new Rect2(minX, 
			minY,
			MathF.Max(p1.X, p2.X) - minX,
			MathF.Max(p1.Y, p2.Y) - minY
		));
	}
	public void erase()
	{
		rectangle = new Rect2();
		QueueRedraw();
	}
    public override void _Draw()
    {
		if (HasRectangle)
		{
			DrawRect(rectangle, SelectionColor);
		}
    }
}
