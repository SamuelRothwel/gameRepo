 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using coolbeats.scripts.staticScriptsAndDataStructures;
using Godot;

public class projectileEffectsTemplate
{
    public float speed = 10;
    public float width = 10;
    public class def : projectileEffectsTemplate
    {
        
    }
}

public class projectilePhysicsTemplate
{
    public virtual void target(projectile proj, Guid Target)
    {
        
    }
    public virtual void travel(projectile proj)
    {
        Vector2 velocity = math.createVector(proj.effects.speed, proj.Rotation);
        Vector2 finalPosition = proj.Position + velocity;
        (Vector2, CollisionObject2D?) result = proj.simulation.rayCast(proj.Position, finalPosition);
        if (result.Item2 == null)
        {
            proj.Position = result.Item1;
            proj.distance += velocity.Length();
        } else
        {
            GD.Print(result.Item1);
            proj.Position = finalPosition;
            impact(proj, result.Item2);
            proj.distance += velocity.Length();
            proj.QueueFree();
        }
    }
    public virtual void impact(projectile proj, CollisionObject2D objectHit)
    {
        GD.Print("ouuuch");
        proj.clean();
    }
    public virtual void destroy(projectile proj)
    {
        proj.clean();
    }
    
    public class def : projectilePhysicsTemplate
    {
        
    }

    public class hitScan : projectilePhysicsTemplate
    {
        public override void travel(projectile proj)
        {
            
        }
    }
    public class untargeted : projectilePhysicsTemplate
    {
        public override void travel(projectile proj)
        {
            
        }
    }
}