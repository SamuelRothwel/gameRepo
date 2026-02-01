using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.AttachedLogic.SubComponents;
using Godot;

namespace coolbeats.scripts.logicScripts.AttachedLogic.Components
{
    public partial class componentGun : Node2D, componentController
    {
        public unitControler controler { get; set; }
        public string state { get; set; }
        public TypeRegistry<subComponent> subComponents { get; set; }

        public string subType {get; set;}

        public Type type => typeof(componentGun);
        Guid? currentTarget = null;
        public float range = 100000;
        public bool scan()
        {
            GD.Print("peakaboo");
            List<Guid> targets = mAccess.teamManager.searchBVH(mAccess.teamManager.GetTeam(controler.ID).targetBVH, math.getMinMax(Position, range));
            
            GD.Print("peakaboo");
            if (targets.Any())
            {
                Guid? target = null;
                float smallest = range;
                int currentPriority = targetPriority(currentTarget);
                bool higherPriority = false;
                foreach (Guid potentialTarget in targets)
                {
                    int priority = targetPriority(potentialTarget);
                    if (higherPriority)
                    {
                        float distance = Position.DistanceTo(mAccess.unitManager.units[potentialTarget].Position);
                        if ((priority == currentPriority && 
                        distance < smallest 
                        || priority > currentPriority) && canTarget(potentialTarget))
                        {
                            target = currentTarget;
                            currentPriority = priority;
                            smallest = distance;
                        }
                    } 
                    else
                    {
                        GD.Print(potentialTarget, targetPriority(potentialTarget), currentPriority, canTarget(currentTarget));
                        GD.Print(potentialTarget == currentTarget && canTarget(currentTarget), targetPriority(potentialTarget) > currentPriority && canTarget(potentialTarget));
                        if (potentialTarget == currentTarget && canTarget(currentTarget))
                        {
                            target = currentTarget;
                        }
                        else if (targetPriority(potentialTarget) > currentPriority && canTarget(potentialTarget))
                        {
                            target = currentTarget;
                            smallest = Position.DistanceTo(mAccess.unitManager.units[potentialTarget].Position);
                            higherPriority = true;
                        }
                    }
                }
                currentTarget = target;
                if (target == null)
                {
                    subComponents.Get<Gun>().shooting = false;
                    return false;
                } else
                {
                    GD.Print("target locked");
                    subComponents.Get<Gun>().shooting = true;
                    return true;
                }
            }
            else
            {
                GD.Print("repbozo");
                subComponents.Get<Gun>().shooting = false;
                return false;
            }
        }
        public int targetPriority(Guid? targetID)
        {
            if (targetID == null)
            {
                return -1000;
            }
            else
            {
                return 0;
            }
        }
        public bool canTarget(Guid? targetID)
        {
            if (targetID == null)
            {
                return false;
            } 
            GD.Print(Position.DistanceTo(mAccess.unitManager.units[(Guid)targetID].Position));
            return Position.DistanceTo(mAccess.unitManager.units[(Guid)targetID].Position) < range;
        }
        public void target(Guid ID)
        {
            subComponents.Get<Gun>().shooting = true;
        }
    }
}