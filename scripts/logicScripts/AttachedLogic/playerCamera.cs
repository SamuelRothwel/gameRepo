using Godot;
using System;

public partial class playerCamera : Camera2D
{
	public override void _Ready()
	{
		mAccess.inputManager.setCamera(this);
	}
}
