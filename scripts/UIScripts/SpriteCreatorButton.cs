using Godot;
using System;

public partial class SpriteCreatorButton : Button
{
    public override void _Pressed()
    {
        mAccess.sceneManager.spriteCreator();
    }
}
