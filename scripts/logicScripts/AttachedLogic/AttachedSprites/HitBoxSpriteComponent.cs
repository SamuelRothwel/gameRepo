using System;
using coolbeats.scripts.logicScripts.Bases;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class HitBoxSpriteComponent: hitboxSprite, subComponent
{
	public virtual void setup()
    {
        base.setup();
        parent.ToString();
        parent.controler.ToString();
        parent.controler.hitBoxes.ToString();
        hitBoxes.ToString();
        parent.controler.hitBoxes.AddRange(hitBoxes);
    }
}