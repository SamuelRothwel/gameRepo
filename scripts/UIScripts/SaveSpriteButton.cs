using Godot;
using System;

public partial class SaveSpriteButton : Button
{
	public override void _Pressed()
	{
		Guid spriteId = mAccess.creatorManager.SaveSprite();
		GD.Print("Saved sprite: " + spriteId);
	}
}
