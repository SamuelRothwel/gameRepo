using Godot;

// Keep RTS Run Tests unchanged; MOBA has an explicit suite launcher so its
// frame-based visual/collision regression test can be run from the menu.
public partial class RunMobaGameplayTestsButton : Button
{
	public override void _Pressed()
	{
		mAccess.testManager.RunSuite("moba-projectile-arena");
	}
}
