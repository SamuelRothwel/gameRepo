using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class privatePhysicsArea
{
    
    Rid physicsAreaID; 
    Rid physicsSpaceID;
    PhysicsDirectSpaceState2D physicsSpace { get { return PhysicsServer2D.SpaceGetDirectState(physicsSpaceID);}}
    List<Area2D> areas;
    public privatePhysicsArea() 
    {
        physicsSpaceID = PhysicsServer2D.SpaceCreate();
        physicsAreaID = PhysicsServer2D.AreaCreate();

        PhysicsServer2D.AreaSetSpace(physicsAreaID, physicsSpaceID);
        areas = new List<Area2D>();
    }
    public void addArea(Area2D area)
    {
        GD.Print(area.Name);
        GD.Print(area.GetRid());
        Rid shape = area.GetChild<CollisionShape2D>(0).Shape.GetRid();
        PhysicsServer2D.AreaAddShape(physicsAreaID, shape, area.GlobalTransform);
        areas.Add(area);
    }
    public (Vector2, CollisionObject2D?) rayCast(Vector2 from, Vector2 to)
    {
        PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(from, to);
        query.CollideWithAreas = true;
        int i = 0;
        foreach (Area2D area in areas)
        {
            PhysicsServer2D.AreaSetShapeTransform(physicsAreaID, i, areas[i].GlobalTransform);
            i++;
        }
        Godot.Collections.Dictionary result = physicsSpace.IntersectRay(query);
        return result.Count() == 0? (to, null) : ((Vector2)result["position"], areas[(int)result["shape"]]);
    }
}