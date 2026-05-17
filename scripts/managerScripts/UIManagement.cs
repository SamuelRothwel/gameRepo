using Godot;
using System;
using System.Collections.Generic;

public partial class UIManagement : managerNode
{
	Node CurrentUI;
	Control defaultMenuControls;
	Button defaultSettingsButton;
	public Dictionary<string, PackedScene> scenes = new Dictionary<string, PackedScene>();
	[Export] public PackedScene UIScene;
	
	public override void setup()
	{
		Godot.Collections.Array<Node> entities = UIScene.Instantiate().GetChildren();
		foreach (Node entity in entities)
		{
			PackedScene packedEntity = new PackedScene();
			packedEntity.Pack(entity);
			scenes.Add(entity.Name, packedEntity);
		}
		changeUI("main");
	}
	public void changeUI(string name)
	{
		if (CurrentUI != null)
		{
			CurrentUI.QueueFree();
		}
		GD.Print();
		CurrentUI = scenes[name].Instantiate();
		AddChild(CurrentUI);
		updateDefaultMenuControls(name);
	}

	void updateDefaultMenuControls(string uiName)
	{
		bool showDefaultSettings = uiName != "main" && isNotMenuState();
		if (!showDefaultSettings)
		{
			if (defaultMenuControls != null && GodotObject.IsInstanceValid(defaultMenuControls))
			{
				defaultMenuControls.QueueFree();
				defaultMenuControls = null;
				defaultSettingsButton = null;
			}
			return;
		}

		if (defaultMenuControls == null || !GodotObject.IsInstanceValid(defaultMenuControls))
		{
			createDefaultMenuControls();
		}
	}

	bool isNotMenuState()
	{
		return mAccess.sceneManager?.gameStates == null || mAccess.sceneManager.gameStates.GetState() != "menu";
	}

	void createDefaultMenuControls()
	{
		defaultMenuControls = new Control();
		defaultMenuControls.Name = "DefaultMenuControls";
		defaultMenuControls.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		defaultMenuControls.MouseFilter = Control.MouseFilterEnum.Ignore;
		AddChild(defaultMenuControls);

		defaultSettingsButton = new Button();
		defaultSettingsButton.Name = "DefaultSettingsButton";
		defaultSettingsButton.Text = "Options";
		defaultSettingsButton.CustomMinimumSize = new Vector2(118, 34);
		defaultSettingsButton.AnchorLeft = 1f;
		defaultSettingsButton.AnchorRight = 1f;
		defaultSettingsButton.OffsetLeft = -134f;
		defaultSettingsButton.OffsetRight = -16f;
		defaultSettingsButton.OffsetTop = 16f;
		defaultSettingsButton.OffsetBottom = 50f;
		defaultSettingsButton.Pressed += openDefaultSettings;
		mAccess.styleManager.applyButtonStyle(defaultSettingsButton, "secondary");
		defaultMenuControls.AddChild(defaultSettingsButton);
	}

	void openDefaultSettings()
	{
		mAccess.windowManager.openWindow("Settings", new SettingsWindowContent(), "resizableMenu");
	}
}
