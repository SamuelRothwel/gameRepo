using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coolbeats.scripts.logicScripts.AttachedLogic.AttachedSprites
{
    public partial class Barracks : pixelAnimated       
    {
        public override void _Ready()
        {
            spriteSet = "Barracks";
            setup();
        }
    }
}