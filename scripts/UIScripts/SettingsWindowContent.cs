using Godot;
using System.Collections.Generic;

public partial class SettingsWindowContent : VBoxContainer
{
	readonly Dictionary<string, string> draftColorNames = new Dictionary<string, string>();
	readonly List<ColorRect> schemePreviewSwatches = new List<ColorRect>();
	Label schemeNameLabel;
	GridContainer savedColorsGrid;
	Button schemePreviewButton;
	int selectedColorSchemeIndex;

	public SettingsWindowContent()
	{
		CustomMinimumSize = new Vector2(420, 320);
		SizeFlagsHorizontal = SizeFlags.ExpandFill;
		SizeFlagsVertical = SizeFlags.ExpandFill;
	}

	public override void _Ready()
	{
		selectedColorSchemeIndex = mAccess.styleManager.activeColorSchemeIndex;
		setupDraftColors();

		TabContainer tabs = new TabContainer();
		tabs.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		tabs.SizeFlagsVertical = SizeFlags.ExpandFill;
		AddChild(tabs);

		tabs.AddChild(createStylesMenu());
		tabs.AddChild(createColorsMenu());

		HBoxContainer actions = new HBoxContainer();
		actions.Alignment = BoxContainer.AlignmentMode.End;
		AddChild(actions);

		Button saveButton = new Button();
		saveButton.Text = "Save";
		saveButton.CustomMinimumSize = new Vector2(86, 30);
		saveButton.Pressed += saveSettings;
		actions.AddChild(saveButton);

		mAccess.colorManager.colorChanged += onColorChanged;
		mAccess.colorManager.colorLibraryChanged += onColorLibraryChanged;
		mAccess.styleManager.styleChanged += onStyleChanged;
	}

	public override void _ExitTree()
	{
		if (mAccess.colorManager != null)
		{
			mAccess.colorManager.colorChanged -= onColorChanged;
			mAccess.colorManager.colorLibraryChanged -= onColorLibraryChanged;
		}
		if (mAccess.styleManager != null)
		{
			mAccess.styleManager.styleChanged -= onStyleChanged;
		}
	}

	Control createStylesMenu()
	{
		VBoxContainer menu = new VBoxContainer();
		menu.Name = "Styles";
		menu.CustomMinimumSize = new Vector2(400, 280);

		Label title = new Label();
		title.Text = "Color Scheme";
		mAccess.styleManager.applyTextStyle(title, "default");
		menu.AddChild(title);

		schemePreviewButton = new Button();
		schemePreviewButton.Text = "";
		schemePreviewButton.CustomMinimumSize = new Vector2(390, 78);
		schemePreviewButton.Pressed += openColorSchemeList;
		menu.AddChild(schemePreviewButton);

		HBoxContainer row = new HBoxContainer();
		row.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		row.OffsetLeft = 10f;
		row.OffsetRight = -10f;
		row.OffsetTop = 8f;
		row.OffsetBottom = -8f;
		row.MouseFilter = Control.MouseFilterEnum.Ignore;
		schemePreviewButton.AddChild(row);

		VBoxContainer labels = new VBoxContainer();
		labels.CustomMinimumSize = new Vector2(120, 0);
		row.AddChild(labels);

		schemeNameLabel = new Label();
		schemeNameLabel.Text = mAccess.styleManager.colorSchemes[selectedColorSchemeIndex].name;
		mAccess.styleManager.applyTextStyle(schemeNameLabel, "accent");
		labels.AddChild(schemeNameLabel);

		Label hint = new Label();
		hint.Text = "Edit colors";
		mAccess.styleManager.applyTextStyle(hint, "muted");
		labels.AddChild(hint);

		HBoxContainer previewColors = new HBoxContainer();
		previewColors.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		previewColors.MouseFilter = Control.MouseFilterEnum.Ignore;
		row.AddChild(previewColors);

		foreach (string colorName in mAccess.styleManager.colorSchemeColorNames)
		{
			string draftColorName = draftColorNames[colorName];
			ColorRect swatch = new ColorRect();
			swatch.Color = mAccess.colorManager.getColor(draftColorName);
			swatch.CustomMinimumSize = new Vector2(24, 48);
			swatch.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			swatch.MouseFilter = Control.MouseFilterEnum.Ignore;
			schemePreviewSwatches.Add(swatch);
			previewColors.AddChild(swatch);
		}

		return menu;
	}

	Control createColorsMenu()
	{
		ScrollContainer scroll = new ScrollContainer();
		scroll.Name = "Colors";
		scroll.CustomMinimumSize = new Vector2(400, 280);

		savedColorsGrid = new GridContainer();
		savedColorsGrid.Columns = 4;
		savedColorsGrid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		scroll.AddChild(savedColorsGrid);

		rebuildColorsMenu();

		return scroll;
	}

	Button createColorButton(string colorName)
	{
		Button button = new Button();
		button.Text = "";
		button.TooltipText = colorName;
		button.CustomMinimumSize = new Vector2(42, 42);
		setButtonColor(button, mAccess.colorManager.getColor(colorName));
		button.Pressed += () => openColorPicker(colorName, button);
		return button;
	}

	void openColorPicker(string colorName, Button button)
	{
		Vector2 pickerPosition = new Vector2(button.GlobalPosition.X + button.Size.X + 12f, button.GlobalPosition.Y);
		mAccess.colorManager.openColorPicker(this, colorName, pickerPosition);
	}

	void openColorSchemeList()
	{
		mAccess.windowManager.openWindowAt("Color Schemes", createColorSchemeList(), new Vector2(schemePreviewButton.GlobalPosition.X + schemePreviewButton.Size.X + 12f, schemePreviewButton.GlobalPosition.Y), "closeButtonTransparentTopbar", false);
	}

	Control createColorSchemeList()
	{
		VBoxContainer list = new VBoxContainer();
		list.CustomMinimumSize = new Vector2(300, 220);

		for (int i = 0; i < mAccess.styleManager.colorSchemes.Count; i++)
		{
			list.AddChild(createColorSchemeRow(i));
		}

		return list;
	}

	Button createColorSchemeRow(int schemeIndex)
	{
		ColorScheme scheme = mAccess.styleManager.colorSchemes[schemeIndex];
		Button rowButton = new Button();
		rowButton.Text = "";
		rowButton.CustomMinimumSize = new Vector2(280, 54);
		rowButton.Pressed += () => selectColorScheme(schemeIndex);

		HBoxContainer row = new HBoxContainer();
		row.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		row.OffsetLeft = 8f;
		row.OffsetRight = -8f;
		row.OffsetTop = 6f;
		row.OffsetBottom = -6f;
		row.MouseFilter = Control.MouseFilterEnum.Ignore;
		rowButton.AddChild(row);

		Label nameLabel = new Label();
		nameLabel.Text = scheme.name;
		nameLabel.CustomMinimumSize = new Vector2(90, 0);
		nameLabel.VerticalAlignment = VerticalAlignment.Center;
		nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
		mAccess.styleManager.applyTextStyle(nameLabel, schemeIndex == selectedColorSchemeIndex ? "accent" : "default");
		row.AddChild(nameLabel);

		HBoxContainer colors = new HBoxContainer();
		colors.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		colors.MouseFilter = Control.MouseFilterEnum.Ignore;
		row.AddChild(colors);

		foreach (Color color in scheme.colors)
		{
			ColorRect swatch = new ColorRect();
			swatch.Color = color;
			swatch.CustomMinimumSize = new Vector2(20, 34);
			swatch.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			swatch.MouseFilter = Control.MouseFilterEnum.Ignore;
			colors.AddChild(swatch);
		}

		return rowButton;
	}

	void selectColorScheme(int schemeIndex)
	{
		selectedColorSchemeIndex = schemeIndex;
		ColorScheme scheme = mAccess.styleManager.colorSchemes[selectedColorSchemeIndex];
		for (int i = 0; i < mAccess.styleManager.colorSchemeColorNames.Length && i < scheme.colors.Length; i++)
		{
			string realColorName = mAccess.styleManager.colorSchemeColorNames[i];
			mAccess.colorManager.updateColor(draftColorNames[realColorName], scheme.colors[i]);
		}
		refreshColorSchemeUi();
		mAccess.windowManager.closeWindow("Color Schemes", false);
	}

	void refreshColorSchemeUi()
	{
		if (schemeNameLabel != null)
		{
			schemeNameLabel.Text = mAccess.styleManager.colorSchemes[selectedColorSchemeIndex].name;
		}

		for (int i = 0; i < schemePreviewSwatches.Count && i < mAccess.styleManager.colorSchemeColorNames.Length; i++)
		{
			string colorName = mAccess.styleManager.colorSchemeColorNames[i];
			schemePreviewSwatches[i].Color = mAccess.colorManager.getColor(draftColorNames[colorName]);
		}
	}

	void onColorChanged(object sender, ColorChangedEvent e)
	{
		if (e.name.StartsWith("settingsDraft_"))
		{
			refreshColorSchemeUi();
		}
	}

	void onStyleChanged(object sender, System.EventArgs e)
	{
		setupDraftColors();
		refreshColorSchemeUi();
	}

	void onColorLibraryChanged(object sender, System.EventArgs e)
	{
		rebuildColorsMenu();
	}

	void rebuildColorsMenu()
	{
		if (savedColorsGrid == null)
		{
			return;
		}

		foreach (Node child in savedColorsGrid.GetChildren())
		{
			savedColorsGrid.RemoveChild(child);
			child.QueueFree();
		}

		foreach (string colorName in mAccess.colorManager.savedColorNames)
		{
			savedColorsGrid.AddChild(createColorButton(colorName));
		}
	}

	void setupDraftColors()
	{
		draftColorNames.Clear();
		foreach (string colorName in mAccess.styleManager.colorSchemeColorNames)
		{
			string draftColorName = "settingsDraft_" + colorName;
			draftColorNames[colorName] = draftColorName;
			mAccess.colorManager.updateColor(draftColorName, mAccess.colorManager.getColor(colorName));
		}
	}

	void saveSettings()
	{
		mAccess.styleManager.activeColorSchemeIndex = selectedColorSchemeIndex;
		foreach (string colorName in mAccess.styleManager.colorSchemeColorNames)
		{
			mAccess.colorManager.updateColor(colorName, mAccess.colorManager.getColor(draftColorNames[colorName]));
		}
	}

	void setButtonColor(Button button, Color color)
	{
		button.AddThemeStyleboxOverride("normal", createSwatchStyle(color));
		button.AddThemeStyleboxOverride("hover", createSwatchStyle(color.Lightened(0.15f)));
		button.AddThemeStyleboxOverride("pressed", createSwatchStyle(color.Darkened(0.15f)));
	}

	StyleBoxFlat createSwatchStyle(Color color)
	{
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = color;
		style.BorderColor = new Color(0.05f, 0.05f, 0.05f, 1f);
		style.BorderWidthBottom = 2;
		style.BorderWidthLeft = 2;
		style.BorderWidthRight = 2;
		style.BorderWidthTop = 2;
		style.CornerRadiusBottomLeft = 4;
		style.CornerRadiusBottomRight = 4;
		style.CornerRadiusTopLeft = 4;
		style.CornerRadiusTopRight = 4;
		return style;
	}
}
