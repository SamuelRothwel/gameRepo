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
        public string type { get => "weapon"; }
        public unitControler controler { get; set; }
        public string state { get; set; }
        public TypeRegistry<subComponent> subComponents { get; set; }

        public float range = 100;
        public void target(Guid ID)
        {
            if (subComponents.Get<vision>().inRange(ID, range))
            {
                subComponents.Get<Gun>().shooting = true;
            }
        }
    }
}