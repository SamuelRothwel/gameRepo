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
        public TypeRegistry subComponents { get; set; }
        public Type type => typeof(componentGun);
        Guid? currentTarget = null;
        public float range = 300;
        public Guid? scan()
        {
            List<Guid> targets = mAccess.teamManager.searchBVH(mAccess.teamManager.GetTeam(controler.ID).targetBVH, math.getMinMax(Position, range));
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
                            target = potentialTarget;
                            currentPriority = priority;
                            smallest = distance;
                        }
                    } 
                    else
                    {
                        if (potentialTarget == currentTarget && canTarget(currentTarget))
                        {
                            target = potentialTarget;
                        }
                        else if (targetPriority(potentialTarget) > currentPriority && canTarget(potentialTarget))
                        {
                            target = potentialTarget;
                            smallest = Position.DistanceTo(mAccess.unitManager.units[potentialTarget].Position);
                            higherPriority = true;
                        }
                    }
                }
                currentTarget = target;
                if (target == null)
                {
                    subComponents.Get<Gun>().shooting = false;
                    return currentTarget;
                } else
                {
                    subComponents.Get<Gun>().fire((Guid)target);
                    return currentTarget;
                }
            }
            else
            {
                currentTarget = null;
                subComponents.Get<Gun>().shooting = false;
                return currentTarget;
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
            return Position.DistanceTo(mAccess.unitManager.units[(Guid)targetID].Position) < range;
        }
        public void target(Guid ID)
        {
            subComponents.Get<Gun>().shooting = true;
        }
    }
}