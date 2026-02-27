using coolbeats.scripts.staticScriptsAndDataStructures;
using Godot;
using System;

public partial class AddSpriteLayerButton : Button
{
    public override void _Pressed()
    {
        mAccess.creatorManager.AddSpriteLayer();
    }
}
