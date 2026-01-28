using Godot;
using System;

public partial class GunBarrel : pixelAnimated
{
	public override void _Ready()
    {
        spriteSet = "GunBarrel";
        setup();
    }
}
