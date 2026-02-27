using coolbeats.scripts.logicScripts.Bases;
using Godot;
using System;

public partial class MarineBody : HitBoxSpriteComponent
{
	public override void setup()
    {
        GD.Print("eeee");
        spriteSet = "Marine";
        base.setup();
    }
}
