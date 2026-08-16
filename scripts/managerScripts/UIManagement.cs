using Godot;
using System;
using System.Collections.Generic;

public partial class UIManagement : managerNode
{
	const int TopbarHeight = 44;
	Node CurrentUI;
	public Node CurrentUIRoot => CurrentUI;
	CanvasLayer topbarLayer;
	PanelContainer topbar;
	Button defaultSettingsButton;
	bool topbarVisible;
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
		if (mAccess.styleManager != null)
		{
			mAccess.styleManager.styleChanged += onStyleChanged;
		}
		changeUI("main");
	}

	public override void _ExitTree()
	{
		if (mAccess.styleManager != null)
		{
			mAccess.styleManager.styleChanged -= onStyleChanged;
		}
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
		updateTopbar(name);
		updateCurrentUiSafeArea();
	}

	void updateTopbar(string uiName)
	{
		topbarVisible = true;
		ensureTopbar();

		if (topbar != null && GodotObject.IsInstanceValid(topbar))
		{
			topbar.Visible = topbarVisible;
		}

		updateViewportSafeArea();
	}

	void ensureTopbar()
	{
		if (topbar != null && GodotObject.IsInstanceValid(topbar))
		{
			return;
		}

		topbarLayer = new CanvasLayer();
		topbarLayer.Name = "ApplicationTopbarLayer";
		topbarLayer.Layer = 120;
		AddChild(topbarLayer);

		topbar = new PanelContainer();
		topbar.Name = "ApplicationTopbar";
		topbar.SetAnchorsPreset(Control.LayoutPreset.TopWide);
		topbar.OffsetBottom = TopbarHeight;
		topbar.MouseFilter = Control.MouseFilterEnum.Stop;
		topbarLayer.AddChild(topbar);

		MarginContainer margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 12);
		margin.AddThemeConstantOverride("margin_right", 12);
		margin.AddThemeConstantOverride("margin_top", 5);
		margin.AddThemeConstantOverride("margin_bottom", 5);
		topbar.AddChild(margin);

		HBoxContainer layout = new HBoxContainer();
		layout.AddThemeConstantOverride("separation", 10);
		margin.AddChild(layout);

		Label title = new Label();
		title.Text = "Cool Beats";
		title.VerticalAlignment = VerticalAlignment.Center;
		title.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		title.MouseFilter = Control.MouseFilterEnum.Ignore;
		layout.AddChild(title);

		defaultSettingsButton = new Button();
		defaultSettingsButton.Name = "DefaultSettingsButton";
		defaultSettingsButton.Text = "Options";
		defaultSettingsButton.CustomMinimumSize = new Vector2(118, 32);
		defaultSettingsButton.Pressed += openDefaultSettings;
		layout.AddChild(defaultSettingsButton);

		setupTopbarDragging(topbar);
		applyTopbarStyle();
	}

	void setupTopbarDragging(Control dragArea)
	{
		bool dragging = false;
		Vector2I dragStartMouse = Vector2I.Zero;
		Vector2I dragStartWindow = Vector2I.Zero;
		dragArea.GuiInput += inputEvent =>
		{
			if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
			{
				dragging = mouseButton.Pressed;
				dragStartMouse = DisplayServer.MouseGetPosition();
				dragStartWindow = DisplayServer.WindowGetPosition();
				dragArea.AcceptEvent();
			}
			else if (inputEvent is InputEventMouseMotion && dragging)
			{
				DisplayServer.WindowSetPosition(dragStartWindow + DisplayServer.MouseGetPosition() - dragStartMouse);
				dragArea.AcceptEvent();
			}
		};
	}

	void applyTopbarStyle()
	{
		if (topbar == null || !GodotObject.IsInstanceValid(topbar) || mAccess.styleManager == null || !mAccess.styleManager.isSetupComplete)
		{
			return;
		}

		mAccess.styleManager.applyPanelStyle(topbar, "raised");
		if (defaultSettingsButton != null && GodotObject.IsInstanceValid(defaultSettingsButton))
		{
			mAccess.styleManager.applyButtonStyle(defaultSettingsButton, "secondary");
		}
		foreach (Node child in topbar.GetChildren())
		{
			mAccess.styleManager.applyUniversalStyleTree(child, true);
		}
	}

	void onStyleChanged(object sender, EventArgs e)
	{
		applyTopbarStyle();
	}

	void updateViewportSafeArea()
	{
		float reservedHeight = getReservedTopbarHeight();
		GetViewport().CanvasTransform = new Transform2D(0f, new Vector2(0f, reservedHeight));
	}

	void updateCurrentUiSafeArea()
	{
		if (CurrentUI == null || !GodotObject.IsInstanceValid(CurrentUI))
		{
			return;
		}

		float reservedHeight = getReservedTopbarHeight();
		if (CurrentUI is Control currentControl)
		{
			applyControlSafeArea(currentControl, reservedHeight);
			return;
		}

		foreach (Node child in CurrentUI.GetChildren())
		{
			if (child is Control control)
			{
				applyControlSafeArea(control, reservedHeight);
			}
		}
	}

	void applyControlSafeArea(Control control, float reservedHeight)
	{
		if (!control.HasMeta("baseOffsetTop"))
		{
			control.SetMeta("baseOffsetTop", control.OffsetTop);
			control.SetMeta("baseOffsetBottom", control.OffsetBottom);
		}

		control.OffsetTop = (float)control.GetMeta("baseOffsetTop").AsDouble() + reservedHeight;
		control.OffsetBottom = (float)control.GetMeta("baseOffsetBottom").AsDouble();
	}

	public float getReservedTopbarHeight()
	{
		return topbarVisible ? TopbarHeight : 0f;
	}

	public Vector2 toGameWindowPosition(Vector2 windowPosition)
	{
		return windowPosition - new Vector2(0f, getReservedTopbarHeight());
	}

	public void openColorPicker(Control owner, string colorName, Vector2 globalPosition)
	{
		ColorPicker activePicker = scenes["colorPicker"].Instantiate<ColorPicker>();
		mAccess.windowManager.openWindowAt("Color Picker", activePicker, globalPosition, "staticMenu", false);
		activePicker.openForColor(colorName);
	}

	void openDefaultSettings()
	{
		mAccess.windowManager.openWindow("Settings", new SettingsWindowContent(), "resizableMenu");
	}
}
