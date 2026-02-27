using Godot;
using System;

public partial class UnitCreatorButton : Button
{
    public override void _Pressed()
    {
        mAccess.sceneManager.unitCreator();
    }
}
