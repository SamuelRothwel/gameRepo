using coolbeats.scripts.staticScriptsAndDataStructures;
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

public class gunTemplate
{
	public virtual void process(Gun gun)
	{
		if (gun.shooting)
		{
			fire(gun);
		}
		else if (hasHeat)
		{
			cool(gun);
		}
	}
	public virtual void fire(Gun gun)
	{
		if (canShoot(gun))
		{
			shoot(gun);
		}
	}
	public virtual void shoot(Gun gun)
	{
		gun.heat += Math.Min(gun.Delta, 10);
		gun.animator.Play("GunAnimations/shoot" + type);
	}
	public virtual void cool(Gun gun)
	{
		if (gun.heat > 1 && gun.animator.CurrentAnimation == "")
		{
			gun.animator.Play("GunAnimations/cool" + type);
		}
		gun.heat = Math.Max(gun.heat - gun.Delta * gun.heat / 10, 0);
	}
	public bool canShoot(Gun gun)
	{
		return true;
	}
	public bool canHeat(Gun gun)
	{
		return true;
	}
	public virtual void spawnBullet(Gun gun)
	{
		projectile bullet = mAccess.recycleManager.Get<projectile>();
		bullet.spawn(gun.GlobalPosition, gun.GlobalRotation, 1000, "default", 
		new Guid[] { gun.target });
		//projectile bullet = mAccess.projectileManager.fire(bulletType, gun.GlobalPosition, (gun.GetParent() as Node2D).GlobalRotation - 0.5f * math.PI + math.randomFloat(-0.0087, 0.0087) * spread);
		gun.AddChild(bullet);
	}
	public virtual bool hasHeat { get; set; } = true;
	public virtual string type { get; set; } = "Template";
	public virtual string bulletType { get; set; } = "regular";
	public virtual float spread { get; set; } = 0;
	public virtual float firerate { get; set; } = 1;
	public class Minigun : gunTemplate
	{
		public override string type { get; set; } = "Minigun";
		public override float spread { get; set; } = 20f;
		public override float firerate { get; set; } = 3;
		public override void shoot(Gun gun)
		{
			gun.heat = Math.Min(0.03+gun.heat, 4);
			gun.rotation += (gun.heat * firerate * (gun.Delta))*100;
			spin(gun);
			if (gun.rotation > 120)
            {
				spawnBullet(gun);
				gun.gunComponents.MoveNext();
                gun.rotation = gun.rotation % 120;
            }
		}
		public override void cool(Gun gun)
		{
			if (gun.heat > 0.2)
            {
				gun.heat -= gun.Delta * gun.heat / 2 + 0.01;
				gun.rotation += (gun.heat * firerate * (gun.Delta))*100;
				if (gun.rotation > 120)
				{
					gun.gunComponents.MoveNext();
					gun.rotation = gun.rotation % 120;
				}
            } else if (gun.heat > 0)
            { 
                gun.heat = 0;
                spinStop(gun);
            } else
            {
                spinStop(gun);
            }
			spin(gun);
		}
		public void spinStop(Gun gun)
        {
			if (gun.rotation != 0)
			{
				gun.rotation += 1.5;
				if (gun.rotation > 120)
				{
					gun.rotation = 0;
				}
			}
        }
		public void spin(Gun gun)
        {
			IEnumerable<Sprite2D> components = gun.gunComponents.loop();
			int i = 0;
			foreach (Sprite2D component in components)
            {
				float sinValue = (float)LUT.sin(i + (int)gun.rotation)+1;
				float cosValue = (float)LUT.cos(i + (int)gun.rotation)+1;
				component.ZIndex = (int)(cosValue*1000);
				component.Position = component.Position with { X = (float)LUT.sin(i + (int)gun.rotation)*5};
				component.Scale = component.Scale with { X = (float)(LUT.cos(i + (int)gun.rotation)*0.1+0.9), Y = (float)(LUT.cos(i + (int)gun.rotation)*0.1+0.9) };
                i += 120;
            }
        }
	} 
	
	public class Sniper : gunTemplate
	{
		public override bool hasHeat { get; set; } = false;
		public override string type { get; set; } = "Sniper";
	} 
}
