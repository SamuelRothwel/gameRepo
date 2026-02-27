using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace coolbeats.scripts.logicScripts.Bases
{
    public partial class hitboxSprite : pixelAnimated
    {
        public Area2D[] hitBoxes;
        Area2D hitBox { get { return hitBoxes[currentHitbox]; } }
        int currentHitbox = 0;
        public Type type => typeof(hitboxSprite);
        public override void setup(string name = null)
        {
            hitBoxes = mAccess.spriteManager.getHitBoxes(name?? spriteSet);
            foreach (Area2D box in hitBoxes)
            {
                box.Visible = true;
                AddChild(box);
                GD.Print(box.Name);
            }
            base.setup(name);
        }
    }
}