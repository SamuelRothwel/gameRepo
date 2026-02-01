using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using Godot;

namespace coolbeats.scripts.logicScripts.AttachedLogic.SubComponents
{
    public partial class vision : Node, subComponent
    {
        public componentController parent { get; set; }
        Type subComponent.type => typeof(vision);
        public bool inRange(Guid ID, float range)
        {
            unitControler unit = mAccess.unitManager.units[ID];
            return parent.self.Position.DistanceTo(unit.Position) < range;
        }
        public Guid? scan(float range)
        {
            List<Guid> possibleTargets = new List<Guid>();
            mAccess.teamManager.searchBVH(mAccess.teamManager.GetTeam(parent.controler.ID).targetBVH, ref possibleTargets, math.getMinMax(parent.self.Position, range));
            Guid? output = null;
            foreach (Guid target in possibleTargets)
            {
                float distance = parent.self.Position.DistanceTo(mAccess.unitManager.units[target].Position);
                if (range > distance) {
                    range = distance;
                    output = target;
                }
            }
            return output;
        }
    }
}