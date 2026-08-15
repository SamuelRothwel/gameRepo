using Godot;
using System.Collections.Generic;

public partial class SettingsWindowContent : VBoxContainer
{
	readonly Dictionary<string, string> draftColorNames = new Dictionary<string, string>();
	readonly Dictionary<string, TextStyle> draftTextStyles = new Dictionary<string, TextStyle>();
	readonly List<ColorRect> schemePreviewSwatches = new List<ColorRect>();
	readonly List<Control> fontPreviewControls = new List<Control>();
	Label pageTitle;
	Label schemeNameLabel;
	Label volumeValueLabel;
	GridContainer savedColorsGrid;
	Button backButton;
	Button schemePreviewButton;
	Control contentHost;
	SettingsPage currentPage = SettingsPage.Home;
	int selectedColorSchemeIndex;
	Vector2I selectedResolution;
	float draftMasterVolumePercent;
	bool applyingSettings;
	readonly Vector2I[] resolutionOptions = new Vector2I[]
	{
		new Vector2I(1280, 720),
		new Vector2I(1366, 768),
		new Vector2I(1600, 900),
		new Vector2I(1920, 1080),
		new Vector2I(2560, 1440),
		new Vector2I(3840, 2160)
	};

	public SettingsWindowContent()
	{
		CustomMinimumSize = new Vector2(640, 460);
		SizeFlagsHorizontal = SizeFlags.ExpandFill;
		SizeFlagsVertical = SizeFlags.ExpandFill;
	}

	public override void _Ready()
	{
		StoredGameSettings savedSettings = mAccess.entityFrameworkManager?.LoadGameSettings();
		selectedColorSchemeIndex = mAccess.styleManager.activeColorSchemeIndex;
		draftMasterVolumePercent = savedSettings == null ? getMasterVolumePercent() : savedSettings.MasterVolumePercent;
		selectedResolution = getInitialResolution(savedSettings);
		setupDraftColors();
		setupDraftTextStyles();
		buildFrame();
		showHomeMenu();

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

	void buildFrame()
	{
		HBoxContainer header = new HBoxContainer();
		header.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		AddChild(header);

		backButton = createActionButton("Back", "secondary");
		backButton.CustomMinimumSize = new Vector2(88, 32);
		backButton.Pressed += showHomeMenu;
		header.AddChild(backButton);

		pageTitle = new Label();
		pageTitle.HorizontalAlignment = HorizontalAlignment.Center;
		pageTitle.VerticalAlignment = VerticalAlignment.Center;
		pageTitle.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		mAccess.styleManager.applyTextStyle(pageTitle, "accent");
		header.AddChild(pageTitle);

		Button saveButton = createActionButton("Save", "secondary");
		saveButton.CustomMinimumSize = new Vector2(88, 32);
		saveButton.Pressed += saveSettings;
		header.AddChild(saveButton);

		contentHost = new Control();
		contentHost.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		contentHost.SizeFlagsVertical = SizeFlags.ExpandFill;
		AddChild(contentHost);
	}

	void showHomeMenu()
	{
		currentPage = SettingsPage.Home;
		pageTitle.Text = "Options";
		backButton.Visible = false;
		setContent(createHomeMenu());
	}

	void showSubMenu(SettingsPage page, string title, Control content)
	{
		currentPage = page;
		pageTitle.Text = title;
		backButton.Visible = true;
		setContent(content);
	}

	void setContent(Control content)
	{
		foreach (Node child in contentHost.GetChildren())
		{
			contentHost.RemoveChild(child);
			child.QueueFree();
		}

		content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		contentHost.AddChild(content);
	}

	Control createHomeMenu()
	{
		VBoxContainer menu = new VBoxContainer();
		menu.AddThemeConstantOverride("separation", 12);
		menu.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		menu.SizeFlagsVertical = SizeFlags.ExpandFill;

		menu.AddChild(createVolumePanel());

		GridContainer categoryGrid = new GridContainer();
		categoryGrid.Columns = 2;
		categoryGrid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		categoryGrid.SizeFlagsVertical = SizeFlags.ExpandFill;
		categoryGrid.AddThemeConstantOverride("h_separation", 10);
		categoryGrid.AddThemeConstantOverride("v_separation", 10);
		menu.AddChild(categoryGrid);

		categoryGrid.AddChild(createCategoryButton("Audio", () => showSubMenu(SettingsPage.Audio, "Audio", createAudioMenu())));
		categoryGrid.AddChild(createCategoryButton("Visual", () => showSubMenu(SettingsPage.Visual, "Visual", createVisualMenu())));
		categoryGrid.AddChild(createCategoryButton("Controls", () => showSubMenu(SettingsPage.Controls, "Controls", createControlsMenu())));
		categoryGrid.AddChild(createCategoryButton("Appearance", () => showSubMenu(SettingsPage.Appearance, "Appearance", createStylesMenu())));
		categoryGrid.AddChild(createCategoryButton("Colors", () => showSubMenu(SettingsPage.Colors, "Colors", createColorsMenu())));
		categoryGrid.AddChild(createCategoryButton("Data", () => showSubMenu(SettingsPage.Data, "Data", createDataMenu())));

		if (shouldShowReturnToMainMenu())
		{
			Button returnButton = createActionButton("Return To Main Menu", "secondary");
			returnButton.CustomMinimumSize = new Vector2(0, 38);
			returnButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			returnButton.Pressed += returnToMainMenu;
			menu.AddChild(returnButton);
		}

		return menu;
	}

	PanelContainer createVolumePanel()
	{
		PanelContainer panel = new PanelContainer();
		panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		mAccess.styleManager.applyPanelStyle(panel, "raised");

		MarginContainer margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 12);
		margin.AddThemeConstantOverride("margin_right", 12);
		margin.AddThemeConstantOverride("margin_top", 10);
		margin.AddThemeConstantOverride("margin_bottom", 10);
		panel.AddChild(margin);

		HBoxContainer row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 10);
		margin.AddChild(row);

		Label label = new Label();
		label.Text = "Master Volume";
		label.CustomMinimumSize = new Vector2(130, 0);
		label.VerticalAlignment = VerticalAlignment.Center;
		mAccess.styleManager.applyTextStyle(label, "default");
		row.AddChild(label);

		HSlider slider = new HSlider();
		slider.MinValue = 0;
		slider.MaxValue = 100;
		slider.Step = 1;
		slider.Value = draftMasterVolumePercent;
		slider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		slider.ValueChanged += value => setDraftMasterVolume((float)value);
		row.AddChild(slider);

		volumeValueLabel = new Label();
		volumeValueLabel.Text = Mathf.RoundToInt(draftMasterVolumePercent) + "%";
		volumeValueLabel.CustomMinimumSize = new Vector2(48, 0);
		volumeValueLabel.HorizontalAlignment = HorizontalAlignment.Right;
		volumeValueLabel.VerticalAlignment = VerticalAlignment.Center;
		mAccess.styleManager.applyTextStyle(volumeValueLabel, "muted");
		row.AddChild(volumeValueLabel);

		return panel;
	}

	Button createCategoryButton(string text, System.Action pressed)
	{
		Button button = createActionButton(text, "menu");
		button.CustomMinimumSize = new Vector2(0, 70);
		button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		button.SizeFlagsVertical = SizeFlags.ExpandFill;
		button.Pressed += pressed;
		return button;
	}

	Button createActionButton(string text, string styleName)
	{
		Button button = new Button();
		button.Text = text;
		mAccess.styleManager.applyButtonStyle(button, styleName);
		return button;
	}

	bool shouldShowReturnToMainMenu()
	{
		return mAccess.sceneManager?.gameStates != null && mAccess.sceneManager.gameStates.GetState() != "menu";
	}

	void returnToMainMenu()
	{
		mAccess.windowManager.closeWindow("Settings", false);
		mAccess.sceneManager.startMenu();
	}

	Control createAudioMenu()
	{
		VBoxContainer menu = new VBoxContainer();
		menu.AddThemeConstantOverride("separation", 12);
		menu.AddChild(createVolumePanel());
		return menu;
	}

	Control createVisualMenu()
	{
		VBoxContainer menu = new VBoxContainer();
		menu.AddThemeConstantOverride("separation", 8);
		menu.AddChild(createReadOnlySettingRow("Window Mode", "Windowed"));
		menu.AddChild(createResolutionRow());
		return menu;
	}

	Control createResolutionRow()
	{
		PanelContainer panel = new PanelContainer();
		panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		mAccess.styleManager.applyPanelStyle(panel, "subtle");

		MarginContainer margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 10);
		margin.AddThemeConstantOverride("margin_right", 10);
		margin.AddThemeConstantOverride("margin_top", 6);
		margin.AddThemeConstantOverride("margin_bottom", 6);
		panel.AddChild(margin);

		HBoxContainer row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 10);
		margin.AddChild(row);

		Label name = new Label();
		name.Text = "Resolution";
		name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		name.VerticalAlignment = VerticalAlignment.Center;
		mAccess.styleManager.applyTextStyle(name, "default");
		row.AddChild(name);

		OptionButton resolutionDropdown = new OptionButton();
		resolutionDropdown.CustomMinimumSize = new Vector2(180, 32);
		resolutionDropdown.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
		List<Vector2I> options = getResolutionOptions();
		for (int i = 0; i < options.Count; i++)
		{
			resolutionDropdown.AddItem(formatResolution(options[i]), i);
			if (options[i] == selectedResolution)
			{
				resolutionDropdown.Selected = i;
			}
		}
		resolutionDropdown.ItemSelected += index =>
		{
			selectedResolution = options[(int)index];
		};
		mAccess.styleManager.applyTextStyle(resolutionDropdown, "default");
		row.AddChild(resolutionDropdown);

		return panel;
	}

	Control createControlsMenu()
	{
		ScrollContainer scroll = new ScrollContainer();
		VBoxContainer menu = new VBoxContainer();
		menu.AddThemeConstantOverride("separation", 8);
		scroll.AddChild(menu);

		foreach (StringName actionName in InputMap.GetActions())
		{
			if (actionName.ToString().StartsWith("ui_"))
			{
				continue;
			}
			menu.AddChild(createInputActionRow(actionName));
		}

		return scroll;
	}

	Control createDataMenu()
	{
		VBoxContainer menu = new VBoxContainer();
		menu.AddThemeConstantOverride("separation", 8);

		Label metadataStatus = new Label();
		metadataStatus.Text = getMetadataStatusText();
		mAccess.styleManager.applyTextStyle(metadataStatus, "muted");
		menu.AddChild(metadataStatus);

		Button refreshMetadataButton = createActionButton("Refresh Metadata", "secondary");
		refreshMetadataButton.CustomMinimumSize = new Vector2(0, 38);
		refreshMetadataButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		refreshMetadataButton.Pressed += () => refreshMetadata(metadataStatus);
		menu.AddChild(refreshMetadataButton);

		return menu;
	}

	Control createInputActionRow(StringName actionName)
	{
		return createReadOnlySettingRow(actionName.ToString(), getActionBindText(actionName));
	}

	string getMetadataStatusText()
	{
		int typeCount = mAccess.unitManager?.unitVariableMetadata?.Count ?? 0;
		return "Metadata Types: " + typeCount;
	}

	void refreshMetadata(Label metadataStatus)
	{
		mAccess.unitManager?.setupUnitVariableMetadata();
		if (metadataStatus != null && GodotObject.IsInstanceValid(metadataStatus))
		{
			metadataStatus.Text = getMetadataStatusText();
		}
	}

	Control createReadOnlySettingRow(string settingName, string settingValue)
	{
		PanelContainer panel = new PanelContainer();
		panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		mAccess.styleManager.applyPanelStyle(panel, "subtle");

		MarginContainer margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 10);
		margin.AddThemeConstantOverride("margin_right", 10);
		margin.AddThemeConstantOverride("margin_top", 6);
		margin.AddThemeConstantOverride("margin_bottom", 6);
		panel.AddChild(margin);

		HBoxContainer row = new HBoxContainer();
		margin.AddChild(row);

		Label name = new Label();
		name.Text = settingName;
		name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		mAccess.styleManager.applyTextStyle(name, "default");
		row.AddChild(name);

		Label bind = new Label();
		bind.Text = settingValue;
		bind.HorizontalAlignment = HorizontalAlignment.Right;
		mAccess.styleManager.applyTextStyle(bind, "muted");
		row.AddChild(bind);

		return panel;
	}

	Vector2I getInitialResolution(StoredGameSettings savedSettings)
	{
		if (savedSettings != null && savedSettings.ResolutionWidth > 0 && savedSettings.ResolutionHeight > 0)
		{
			return new Vector2I(savedSettings.ResolutionWidth, savedSettings.ResolutionHeight);
		}

		return DisplayServer.WindowGetSize();
	}

	List<Vector2I> getResolutionOptions()
	{
		List<Vector2I> options = new List<Vector2I>(resolutionOptions);
		if (!options.Contains(selectedResolution))
		{
			options.Insert(0, selectedResolution);
		}
		return options;
	}

	string formatResolution(Vector2I resolution)
	{
		return resolution.X + " x " + resolution.Y;
	}

	string getActionBindText(StringName actionName)
	{
		Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(actionName);
		if (events.Count == 0)
		{
			return "Unbound";
		}

		return events[0].AsText();
	}

	Label createSectionLabel(string text)
	{
		Label label = new Label();
		label.Text = text;
		label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		mAccess.styleManager.applyTextStyle(label, "muted");
		return label;
	}

	Control createStylesMenu()
	{
		ScrollContainer scroll = new ScrollContainer();
		scroll.CustomMinimumSize = new Vector2(560, 340);
		scroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		scroll.SizeFlagsVertical = SizeFlags.ExpandFill;

		VBoxContainer menu = new VBoxContainer();
		menu.AddThemeConstantOverride("separation", 12);
		menu.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		menu.SizeFlagsVertical = SizeFlags.ExpandFill;
		scroll.AddChild(menu);

		Label title = new Label();
		title.Text = "Color Scheme";
		mAccess.styleManager.applyTextStyle(title, "default");
		menu.AddChild(title);

		schemePreviewButton = createActionButton("", "menu");
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

		schemePreviewSwatches.Clear();
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

		menu.AddChild(createFontPresetSection());

		return scroll;
	}

	Control createFontPresetSection()
	{
		VBoxContainer section = new VBoxContainer();
		section.AddThemeConstantOverride("separation", 8);
		section.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		section.SizeFlagsVertical = SizeFlags.ExpandFill;

		Label title = new Label();
		title.Text = "Font Presets";
		mAccess.styleManager.applyTextStyle(title, "default");
		section.AddChild(title);

		fontPreviewControls.Clear();
		foreach (KeyValuePair<string, TextStyle> draftStyle in draftTextStyles)
		{
			section.AddChild(createFontPresetEditor(draftStyle.Key, draftStyle.Value));
		}

		return section;
	}

	Control createFontPresetEditor(string presetName, TextStyle draftStyle)
	{
		PanelContainer panel = new PanelContainer();
		panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		panel.SizeFlagsVertical = SizeFlags.ExpandFill;
		mAccess.styleManager.applyPanelStyle(panel, "subtle");

		MarginContainer margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 10);
		margin.AddThemeConstantOverride("margin_right", 10);
		margin.AddThemeConstantOverride("margin_top", 8);
		margin.AddThemeConstantOverride("margin_bottom", 8);
		panel.AddChild(margin);

		VBoxContainer layout = new VBoxContainer();
		layout.AddThemeConstantOverride("separation", 6);
		margin.AddChild(layout);

		HBoxContainer topRow = new HBoxContainer();
		topRow.AddThemeConstantOverride("separation", 8);
		layout.AddChild(topRow);

		Label nameLabel = new Label();
		nameLabel.Text = presetName;
		nameLabel.CustomMinimumSize = new Vector2(110, 0);
		nameLabel.VerticalAlignment = VerticalAlignment.Center;
		mAccess.styleManager.applyTextStyle(nameLabel, "default");
		topRow.AddChild(nameLabel);

		Button fontButton = createActionButton(draftStyle.fontName, "secondary");
		fontButton.CustomMinimumSize = new Vector2(250, 34);
		fontButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		applyFontPreview(fontButton, draftStyle.fontName);
		fontButton.Pressed += () => openFontDropdown(presetName, fontButton);
		topRow.AddChild(fontButton);

		SpinBox sizeInput = new SpinBox();
		sizeInput.MinValue = 8;
		sizeInput.MaxValue = 40;
		sizeInput.Step = 1;
		sizeInput.Value = draftStyle.fontSize;
		sizeInput.CustomMinimumSize = new Vector2(82, 0);
		sizeInput.ValueChanged += value =>
		{
			draftStyle.fontSize = Mathf.RoundToInt((float)value);
			refreshFontPreviews();
		};
		topRow.AddChild(sizeInput);

		HBoxContainer toggles = new HBoxContainer();
		toggles.AddThemeConstantOverride("separation", 8);
		topRow.AddChild(toggles);

		toggles.AddChild(createStyleToggle("B", "Bold", new TextStyle(draftStyle.colorName, draftStyle.hoverColorName, draftStyle.pressedColorName, draftStyle.disabledColorName, draftStyle.fontSize, draftStyle.fontName, true, false, false), draftStyle.bold, value =>
		{
			draftStyle.bold = value;
			refreshFontPreviews();
		}));
		toggles.AddChild(createStyleToggle("I", "Italic", new TextStyle(draftStyle.colorName, draftStyle.hoverColorName, draftStyle.pressedColorName, draftStyle.disabledColorName, draftStyle.fontSize, draftStyle.fontName, false, true, false), draftStyle.italic, value =>
		{
			draftStyle.italic = value;
			refreshFontPreviews();
		}));
		toggles.AddChild(createStyleToggle("U", "Underline", new TextStyle(draftStyle.colorName, draftStyle.hoverColorName, draftStyle.pressedColorName, draftStyle.disabledColorName, draftStyle.fontSize, draftStyle.fontName, false, false, true), draftStyle.underline, value =>
		{
			draftStyle.underline = value;
			refreshFontPreviews();
		}));

		Label preview = new Label();
		preview.Text = "Preview: The quick settings menu";
		preview.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		preview.SetMeta("settingsDraftTextStyle", presetName);
		fontPreviewControls.Add(preview);
		applyDraftTextStyle(preview, draftStyle);
		layout.AddChild(preview);

		return panel;
	}

	Button createStyleToggle(string label, string tooltip, TextStyle previewStyle, bool value, System.Action<bool> changed)
	{
		Button toggle = createActionButton(label, value ? "selected" : "secondary");
		toggle.ToggleMode = true;
		toggle.Text = label;
		toggle.TooltipText = tooltip;
		toggle.ButtonPressed = value;
		toggle.CustomMinimumSize = new Vector2(38, 34);
		applyDraftTextStyle(toggle, previewStyle);
		toggle.Toggled += toggledOn =>
		{
			mAccess.styleManager.applyButtonStyle(toggle, toggledOn ? "selected" : "secondary");
			applyDraftTextStyle(toggle, previewStyle);
			changed(toggledOn);
		};
		return toggle;
	}

	void openFontDropdown(string presetName, Button anchor)
	{
		VBoxContainer list = new VBoxContainer();
		list.CustomMinimumSize = new Vector2(260, 0);
		foreach (string fontName in mAccess.styleManager.fontNames)
		{
			Button option = createActionButton(fontName, "secondary");
			option.CustomMinimumSize = new Vector2(240, 34);
			option.Alignment = HorizontalAlignment.Left;
			applyFontPreview(option, fontName);
			option.Pressed += () =>
			{
				draftTextStyles[presetName].fontName = fontName;
				mAccess.windowManager.closeWindow("Font Dropdown", false);
				showSubMenu(SettingsPage.Appearance, "Appearance", createStylesMenu());
			};
			list.AddChild(option);
		}

		Vector2 position = new Vector2(anchor.GlobalPosition.X, anchor.GlobalPosition.Y + anchor.Size.Y + 6f);
		mAccess.windowManager.openWindowAt("Font Dropdown", list, position, "closeButtonTransparentTopbar", false);
	}

	void applyFontPreview(Control control, string fontName)
	{
		Font font = mAccess.styleManager.getFont(fontName);
		if (font != null)
		{
			control.AddThemeFontOverride("font", font);
		}
	}

	void applyDraftTextStyle(Control control, TextStyle style)
	{
		Font font = mAccess.styleManager.getStyledFont(style);
		if (font != null)
		{
			control.AddThemeFontOverride("font", font);
		}
		control.AddThemeColorOverride("font_color", mAccess.colorManager.getColor(style.colorName));
		control.AddThemeColorOverride("font_hover_color", mAccess.colorManager.getColor(style.hoverColorName));
		control.AddThemeColorOverride("font_pressed_color", mAccess.colorManager.getColor(style.pressedColorName));
		control.AddThemeColorOverride("font_disabled_color", mAccess.colorManager.getColor(style.disabledColorName));
		control.AddThemeFontSizeOverride("font_size", style.fontSize);
		mAccess.styleManager.applyUnderlineStyle(control, style.underline, mAccess.colorManager.getColor(style.colorName));
	}

	void refreshFontPreviews()
	{
		foreach (Control preview in fontPreviewControls)
		{
			if (!GodotObject.IsInstanceValid(preview) || !preview.HasMeta("settingsDraftTextStyle"))
			{
				continue;
			}

			string presetName = preview.GetMeta("settingsDraftTextStyle", "").AsString();
			if (draftTextStyles.ContainsKey(presetName))
			{
				applyDraftTextStyle(preview, draftTextStyles[presetName]);
			}
		}
	}

	Control createColorsMenu()
	{
		ScrollContainer scroll = new ScrollContainer();
		scroll.CustomMinimumSize = new Vector2(400, 280);

		savedColorsGrid = new GridContainer();
		savedColorsGrid.Columns = 4;
		savedColorsGrid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		savedColorsGrid.AddThemeConstantOverride("h_separation", 8);
		savedColorsGrid.AddThemeConstantOverride("v_separation", 8);
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
		mAccess.uiManager.openColorPicker(this, colorName, pickerPosition);
	}

	void openColorSchemeList()
	{
		mAccess.windowManager.openWindowAt("Color Schemes", createColorSchemeList(), new Vector2(schemePreviewButton.GlobalPosition.X + schemePreviewButton.Size.X + 12f, schemePreviewButton.GlobalPosition.Y), "closeButtonTransparentTopbar", false);
	}

	Control createColorSchemeList()
	{
		VBoxContainer list = new VBoxContainer();
		list.CustomMinimumSize = new Vector2(300, 220);

		Button createButton = createActionButton("Create New Scheme", "secondary");
		createButton.CustomMinimumSize = new Vector2(280, 38);
		createButton.Pressed += openNewColorSchemeWindow;
		list.AddChild(createButton);

		for (int i = 0; i < mAccess.styleManager.colorSchemes.Count; i++)
		{
			list.AddChild(createColorSchemeRow(i));
		}

		return list;
	}

	void openNewColorSchemeWindow()
	{
		VBoxContainer content = new VBoxContainer();
		content.CustomMinimumSize = new Vector2(280, 92);
		content.AddThemeConstantOverride("separation", 8);

		Label label = new Label();
		label.Text = "Scheme Name";
		mAccess.styleManager.applyTextStyle(label, "default");
		content.AddChild(label);

		LineEdit nameInput = new LineEdit();
		nameInput.Text = "Custom Scheme " + (mAccess.styleManager.colorSchemes.Count + 1);
		nameInput.SelectAll();
		mAccess.styleManager.applyTextStyle(nameInput, "default");
		content.AddChild(nameInput);

		HBoxContainer buttons = new HBoxContainer();
		buttons.AddThemeConstantOverride("separation", 8);
		content.AddChild(buttons);

		Button createButton = createActionButton("Create", "secondary");
		createButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		createButton.Pressed += () => createNewColorScheme(nameInput.Text);
		buttons.AddChild(createButton);

		Button cancelButton = createActionButton("Cancel", "secondary");
		cancelButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		cancelButton.Pressed += () => mAccess.windowManager.closeWindow("New Color Scheme", false);
		buttons.AddChild(cancelButton);

		mAccess.windowManager.openWindow("New Color Scheme", content, "closeButtonTransparentTopbar", false);
		nameInput.GrabFocus();
	}

	void createNewColorScheme(string schemeName)
	{
		Color[] colors = new Color[mAccess.styleManager.colorSchemeColorNames.Length];
		for (int i = 0; i < colors.Length; i++)
		{
			string colorName = mAccess.styleManager.colorSchemeColorNames[i];
			colors[i] = mAccess.colorManager.getColor(draftColorNames[colorName]);
		}

		selectedColorSchemeIndex = mAccess.styleManager.addColorScheme(schemeName, colors);
		refreshColorSchemeUi();
		mAccess.windowManager.closeWindow("New Color Scheme", false);
		mAccess.windowManager.closeWindow("Color Schemes", false);
	}

	Button createColorSchemeRow(int schemeIndex)
	{
		ColorScheme scheme = mAccess.styleManager.colorSchemes[schemeIndex];
		Button rowButton = createActionButton("", "menu");
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

	void setDraftMasterVolume(float value)
	{
		draftMasterVolumePercent = Mathf.Clamp(value, 0f, 100f);
		if (volumeValueLabel != null)
		{
			volumeValueLabel.Text = Mathf.RoundToInt(draftMasterVolumePercent) + "%";
		}
	}

	float getMasterVolumePercent()
	{
		int busIndex = AudioServer.GetBusIndex("Master");
		float volumeDb = AudioServer.GetBusVolumeDb(busIndex);
		return Mathf.Clamp(Mathf.DbToLinear(volumeDb) * 100f, 0f, 100f);
	}

	void applyMasterVolume()
	{
		int busIndex = AudioServer.GetBusIndex("Master");
		float linearVolume = Mathf.Max(draftMasterVolumePercent / 100f, 0.0001f);
		AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(linearVolume));
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
			if (draftColorNames.TryGetValue(colorName, out string draftColorName))
			{
				schemePreviewSwatches[i].Color = mAccess.colorManager.getColor(draftColorName);
			}
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
		if (applyingSettings)
		{
			return;
		}

		setupDraftColors();
		if (currentPage == SettingsPage.Home)
		{
			showHomeMenu();
		}
		else
		{
			refreshColorSchemeUi();
		}
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
			draftColorNames[colorName] = "settingsDraft_" + colorName;
		}

		foreach (string colorName in mAccess.styleManager.colorSchemeColorNames)
		{
			mAccess.colorManager.updateColor(draftColorNames[colorName], mAccess.colorManager.getColor(colorName));
		}
	}

	void setupDraftTextStyles()
	{
		draftTextStyles.Clear();
		foreach (KeyValuePair<string, TextStyle> textStyle in mAccess.styleManager.textStyles)
		{
			draftTextStyles[textStyle.Key] = textStyle.Value.Clone();
		}
	}

	void saveSettings()
	{
		mAccess.styleManager.activeColorSchemeIndex = selectedColorSchemeIndex;
		Dictionary<string, Color> colorsToSave = new Dictionary<string, Color>();
		foreach (string colorName in mAccess.styleManager.colorSchemeColorNames)
		{
			colorsToSave[colorName] = mAccess.colorManager.getColor(draftColorNames[colorName]);
		}

		applyingSettings = true;
		foreach (KeyValuePair<string, Color> colorToSave in colorsToSave)
		{
			mAccess.colorManager.updateColor(colorToSave.Key, colorToSave.Value);
		}
		foreach (KeyValuePair<string, TextStyle> draftTextStyle in draftTextStyles)
		{
			if (mAccess.styleManager.textStyles.ContainsKey(draftTextStyle.Key))
			{
				mAccess.styleManager.textStyles[draftTextStyle.Key].CopyFrom(draftTextStyle.Value);
			}
			else
			{
				mAccess.styleManager.textStyles[draftTextStyle.Key] = draftTextStyle.Value.Clone();
			}
		}
		mAccess.styleManager.refreshTextStyles();
		applyingSettings = false;
		applyResolution();
		mAccess.entityFrameworkManager?.SaveGameSettings(mAccess.styleManager.createStoredGameSettings(draftMasterVolumePercent, selectedResolution));
		setupDraftColors();
		refreshColorSchemeUi();
		refreshFontPreviews();
		applyMasterVolume();
	}

	void applyResolution()
	{
		if (selectedResolution.X > 0 && selectedResolution.Y > 0)
		{
			DisplayServer.WindowSetSize(selectedResolution);
		}
	}

	void setButtonColor(Button button, Color color)
	{
		mAccess.styleManager.applySwatchStyle(button, color);
	}
}

public enum SettingsPage
{
	Home,
	Audio,
	Visual,
	Controls,
	Appearance,
	Colors,
	Data
}
