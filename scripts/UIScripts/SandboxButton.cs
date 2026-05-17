using Godot;

public partial class SandboxButton : Button
{
	public override void _Pressed()
	{
		mAccess.sceneManager.sandbox();
	}
}
