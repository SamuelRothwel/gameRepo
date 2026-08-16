using Godot;

public partial class RunGameplayTestsButton : Button
{
	public override void _Pressed()
	{
		mAccess.testManager.RunSuite("cool-beats-tactical");
	}
}
