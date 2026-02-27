/*using Godot;
using System;

public class deprecatedProjectile
{
	public virtual float velocity { get; set; } = 1500;
	public virtual float lifetime { get; set; } = 1f;
	public virtual Color[] trailColour { get; set; } = { new Color(0, 1, 1, 1), new Color(1, 0, 1, 1) };
	public virtual double trailLength { get; set; } = 40;
	public virtual Shape2D shape { get; set; } = new CircleShape2D();
	public virtual void collision(projectile Projectile, GodotObject collider = null)
	{
		GD.Print("ouch");
		damageableSprite p = (damageableSprite)((CharacterBody2D)collider).GetParent();
		p.impact(Projectile);
	}
	public class regular : deprecatedProjectile
	{
		public override Color[] trailColour { get; set; } = { new Color(0, 0, 0, 0), new Color(0, 0, 0, 0) };
	}
}
*/