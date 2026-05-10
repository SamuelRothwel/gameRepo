using Godot;

public partial class SettingsButton : Button
{
	public override void _Pressed()
	{
		mAccess.windowManager.openWindow("Settings", new SettingsWindowContent(), "resizableMenu");
	}
}
