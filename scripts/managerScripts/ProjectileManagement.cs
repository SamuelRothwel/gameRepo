/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Godot;

public partial class ProjectileManagement : managerNode
{
    Stack<projectile> idleProjectiles = new Stack<projectile>();
    List<projectile> activeProjectiles = new List<projectile>();
    public override void setup()
    {
        idleProjectiles = new Stack<projectile>(Enumerable.Range(0, 1025)
        .Select(x => { projectile proj = (projectile)mAccess.entityManager.getEntity("projectile"); proj.setup(); return proj; }));
    }
    public projectile fire(string type, Vector2 coordinate, float rotation)
    {
        projectile activated;
        bool hasProjectile = idleProjectiles.TryPop(out activated);
        if (!hasProjectile)
        {
            activated = (projectile)mAccess.entityManager.getEntity("projectile");
            activated.setup();
        }
		activated.GlobalPosition = coordinate;
		activated.Rotation = rotation;
        activated.active = true;
        activated.fire(type);
        activeProjectiles.Add(activated);
        return activated;
    }
    public override void _Process(double delta)
    {
        for (int i = activeProjectiles.Count() - 1; i > 0; i--)
        {
            projectile currentProjectile = activeProjectiles[i];
            if (currentProjectile.active == true)
            {
                currentProjectile.trail.Points = new Godot.Vector2[] { currentProjectile.trailCoordinate, currentProjectile.GlobalPosition };
            }
            else
            {
                currentProjectile.GetParent().RemoveChild(currentProjectile);
                idleProjectiles.Push(currentProjectile);
                activeProjectiles.RemoveAt(i);
            }
        }
    }
}*/