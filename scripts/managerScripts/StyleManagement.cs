using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class StyleManagement : managerNode
{
	public event System.EventHandler styleChanged;
	public Dictionary<string, StyleBoxFlat> panelStyles;
	public Dictionary<string, ButtonVisualStyle> buttonStyles;
	public Dictionary<string, StyleBoxFlat> windowStyles;
	public Dictionary<string, WindowPreset> windowPresets;
	public Dictionary<string, TextStyle> textStyles;
	public Dictionary<string, Font> fonts;
	public List<string> fontNames;
	public System.Collections.Generic.List<ColorScheme> colorSchemes;
	const string DefaultFontName = "Xirod";
	const int BuiltInColorSchemeCount = 3;
	public bool isSetupComplete { get; private set; }
	public int activeColorSchemeIndex;
	public readonly string[] colorSchemeColorNames = new string[]
	{
		"stylePanelBackground",
		"stylePanelSubtleBackground",
		"stylePanelRaisedBackground",
		"stylePanelBorder",
		"styleWindowBackground",
		"styleWindowBorder",
		"styleTextPrimary",
		"styleTextMuted",
		"styleTextAccent",
		"stylePanelStrongBorder"
	};

	public override void setup()
	{
		isSetupComplete = false;
		panelStyles = new Dictionary<string, StyleBoxFlat>();
		buttonStyles = new Dictionary<string, ButtonVisualStyle>();
		windowStyles = new Dictionary<string, StyleBoxFlat>();
		windowPresets = new Dictionary<string, WindowPreset>();
		textStyles = new Dictionary<string, TextStyle>();
		fonts = new Dictionary<string, Font>();
		fontNames = new List<string>();

		setupColors();
		setupColorSchemes();
		setupFonts();
		setupTextStyles();
		applyStoredGameSettings(mAccess.entityFrameworkManager?.LoadGameSettings(), false);
		setupPanelStyles();
		setupButtonStyles();
		setupWindowStyles();
		mAccess.colorManager.colorChanged += onColorChanged;
		isSetupComplete = true;
	}

	void setupColors()
	{
		addColor("stylePanelBackground", new Color(0.13f, 0.14f, 0.16f, 1f));
		addColor("stylePanelSubtleBackground", new Color(0.09f, 0.1f, 0.11f, 0.94f));
		addColor("stylePanelRaisedBackground", new Color(0.18f, 0.19f, 0.21f, 1f));
		addColor("stylePanelBorder", new Color(0.72f, 0.75f, 0.8f, 0.25f));
		addColor("stylePanelStrongBorder", new Color(0.92f, 0.86f, 0.58f, 0.5f));

		addColor("styleWindowBackground", new Color(0.18f, 0.19f, 0.21f, 1f));
		addColor("styleWindowBorder", new Color(0.72f, 0.75f, 0.8f, 0.28f));
		addColor("styleWindowShadow", new Color(0f, 0f, 0f));

		addColor("styleTextPrimary", new Color(0.93f, 0.94f, 0.95f, 1f));
		addColor("styleTextMuted", new Color(0.63f, 0.66f, 0.7f, 1f));
		addColor("styleTextAccent", new Color(0.96f, 0.72f, 0.2f, 1f));
		addColor("styleTextDanger", new Color(0.95f, 0.28f, 0.22f, 1f));
		addColor("styleTextDisabled", new Color(0.38f, 0.4f, 0.43f, 1f));
		addColor("styleSwatchBorder", new Color(0.05f, 0.05f, 0.05f, 1f));
	}

	void setupColorSchemes()
	{
		colorSchemes = new System.Collections.Generic.List<ColorScheme>();
		colorSchemes.Add(new ColorScheme("Default", new Color[]
		{
			color("stylePanelBackground"),
			color("stylePanelSubtleBackground"),
			color("stylePanelRaisedBackground"),
			color("stylePanelBorder"),
			color("styleWindowBackground"),
			color("styleWindowBorder"),
			color("styleTextPrimary"),
			color("styleTextMuted"),
			color("styleTextAccent"),
			color("stylePanelStrongBorder")
		}));
		colorSchemes.Add(new ColorScheme("Deep Blue", new Color[]
		{
			new Color(0.08f, 0.11f, 0.15f, 1f),
			new Color(0.05f, 0.07f, 0.1f, 0.96f),
			new Color(0.12f, 0.17f, 0.24f, 1f),
			new Color(0.4f, 0.65f, 0.9f, 0.36f),
			new Color(0.1f, 0.14f, 0.2f, 1f),
			new Color(0.4f, 0.65f, 0.9f, 0.36f),
			new Color(0.9f, 0.95f, 1f, 1f),
			new Color(0.62f, 0.75f, 0.85f, 1f),
			new Color(0.32f, 0.72f, 0.95f, 1f),
			new Color(0.4f, 0.65f, 0.9f, 0.5f)
		}));
		colorSchemes.Add(new ColorScheme("Forge", new Color[]
		{
			new Color(0.16f, 0.13f, 0.11f, 1f),
			new Color(0.1f, 0.08f, 0.07f, 0.96f),
			new Color(0.23f, 0.18f, 0.14f, 1f),
			new Color(0.88f, 0.56f, 0.28f, 0.38f),
			new Color(0.22f, 0.17f, 0.13f, 1f),
			new Color(0.88f, 0.56f, 0.28f, 0.38f),
			new Color(0.96f, 0.92f, 0.84f, 1f),
			new Color(0.72f, 0.64f, 0.55f, 1f),
			new Color(0.95f, 0.48f, 0.18f, 1f),
			new Color(0.88f, 0.56f, 0.28f, 0.5f)
		}));
		activeColorSchemeIndex = 0;
	}

	void setupFonts()
	{
		fonts.Clear();
		fontNames.Clear();
		string[] files = DirAccess.GetFilesAt("res://visualAssets/Fonts");
		foreach (string file in files.OrderBy(fileName => fileName))
		{
			string extension = file.GetExtension().ToLower();
			if (extension != "otf" && extension != "ttf" && extension != "ttc" && extension != "woff" && extension != "woff2")
			{
				continue;
			}

			string fontName = file.GetBaseName();
			FontFile font = GD.Load<FontFile>("res://visualAssets/Fonts/" + file);
			if (font == null)
			{
				continue;
			}

			fonts[fontName] = font;
			fontNames.Add(fontName);
		}

		if (!fonts.ContainsKey(DefaultFontName) && fontNames.Count > 0)
		{
			fonts[DefaultFontName] = fonts[fontNames[0]];
			fontNames.Insert(0, DefaultFontName);
		}
	}

	void setupPanelStyles()
	{
		addPanelStyle("default", createBox("stylePanelBackground", "stylePanelBorder", 1, 6));
		addPanelStyle("subtle", createBox("stylePanelSubtleBackground", "stylePanelBorder", 1, 4));
		addPanelStyle("raised", createBox("stylePanelRaisedBackground", "stylePanelStrongBorder", 1, 8));
	}

	void setupButtonStyles()
	{
		Color background = color("stylePanelRaisedBackground");
		Color border = color("stylePanelBorder");
		Color strongBorder = color("stylePanelStrongBorder");
		addButtonStyle("menu", new ButtonVisualStyle
		(
			createBox(background, border, 1, 6),
			createBox(background.Lightened(0.12f), strongBorder, 1, 6),
			createBox(background.Darkened(0.12f), strongBorder, 1, 6),
			createBox(color("stylePanelSubtleBackground"), border.Darkened(0.25f), 1, 6)
		));

		addButtonStyle("secondary", new ButtonVisualStyle
		(
			createBox(color("stylePanelSubtleBackground"), border, 1, 4),
			createBox(background, border, 1, 4),
			createBox(background.Darkened(0.12f), strongBorder, 1, 4),
			createBox(color("stylePanelSubtleBackground"), border.Darkened(0.25f), 1, 4)
		));

		addButtonStyle("selected", new ButtonVisualStyle
		(
			createBox(background.Lightened(0.18f), strongBorder, 1, 4),
			createBox(background.Lightened(0.24f), strongBorder, 1, 4),
			createBox(background.Lightened(0.08f), strongBorder, 1, 4),
			createBox(color("stylePanelSubtleBackground"), border.Darkened(0.25f), 1, 4)
		));
	}

	void setupTextStyles()
	{
		addTextStyle("default", new TextStyle("styleTextPrimary", "styleTextAccent", "styleTextMuted", "styleTextDisabled", 16, DefaultFontName));
		addTextStyle("muted", new TextStyle("styleTextMuted", "styleTextPrimary", "styleTextMuted", "styleTextDisabled", 14, DefaultFontName));
		addTextStyle("accent", new TextStyle("styleTextAccent", "styleTextPrimary", "styleTextMuted", "styleTextDisabled", 16, DefaultFontName, true, false, false));
		addTextStyle("danger", new TextStyle("styleTextDanger", "styleTextPrimary", "styleTextMuted", "styleTextDisabled", 16, DefaultFontName));
		addTextStyle("title", new TextStyle("styleTextAccent", "styleTextPrimary", "styleTextMuted", "styleTextDisabled", 20, DefaultFontName, true, false, false));
	}

	void setupWindowStyles()
	{
		StyleBoxFlat defaultWindow = createBox("styleWindowBackground", "styleWindowBorder", 1, 10);
		defaultWindow.ShadowColor = color("styleWindowShadow");
		defaultWindow.ShadowSize = 0;
		addWindowStyle("default", defaultWindow);

		WindowPreset staticMenu = new WindowPreset
		{
			hasTopbar = true,
			hasCloseButton = true,
			resizeToContents = true,
			resizable = false,
			minimumSize = Vector2I.Zero,
			padding = new Vector2I(12, 12),
			topbarHeight = 34,
			cornerRadius = 10,
			backgroundColor = color("styleWindowBackground"),
			borderColor = color("styleWindowBorder"),
			shadowColor = color("styleWindowShadow"),
			shadowSize = 8
		};
		addWindowPreset("staticMenu", staticMenu);
		addWindowPreset("closeButtonTransparentTopbar", staticMenu);

		addWindowPreset("resizableMenu", new WindowPreset
		{
			hasTopbar = true,
			hasCloseButton = true,
			resizeToContents = false,
			resizable = true,
			minimumSize = new Vector2I(560, 430),
			initialSize = new Vector2I(640, 500),
			padding = new Vector2I(14, 14),
			topbarHeight = 34,
			cornerRadius = 10,
			backgroundColor = color("styleWindowBackground"),
			borderColor = color("styleWindowBorder"),
			shadowColor = color("styleWindowShadow"),
			shadowSize = 8
		});

		addWindowPreset("transparentTopbarNoClose", new WindowPreset
		{
			hasTopbar = true,
			hasCloseButton = false,
			resizeToContents = true,
			resizable = false,
			minimumSize = Vector2I.Zero,
			padding = new Vector2I(12, 12),
			topbarHeight = 34,
			cornerRadius = 10,
			backgroundColor = color("styleWindowBackground"),
			borderColor = color("styleWindowBorder"),
			shadowColor = color("styleWindowShadow"),
			shadowSize = 8
		});
	}

	void addColor(string name, Color value)
	{
		mAccess.colorManager.addColor(name, value);
	}

	void onColorChanged(object sender, ColorChangedEvent e)
	{
		if (!e.name.StartsWith("style"))
		{
			return;
		}

		rebuildStyles();
		styleChanged?.Invoke(this, System.EventArgs.Empty);
	}

	void rebuildStyles()
	{
		panelStyles.Clear();
		buttonStyles.Clear();
		windowStyles.Clear();
		windowPresets.Clear();
		setupPanelStyles();
		setupButtonStyles();
		setupWindowStyles();
		applyUniversalStyleTree(GetTree().Root, true);
	}

	Color color(string name)
	{
		return mAccess.colorManager.getColor(name);
	}

	StyleBoxFlat createBox(string backgroundColorName, string borderColorName, int borderWidth, int cornerRadius)
	{
		return createBox(color(backgroundColorName), color(borderColorName), borderWidth, cornerRadius);
	}

	StyleBoxFlat createBox(Color backgroundColor, Color borderColor, int borderWidth, int cornerRadius)
	{
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = backgroundColor;
		style.BorderColor = borderColor;
		style.BorderWidthBottom = borderWidth;
		style.BorderWidthLeft = borderWidth;
		style.BorderWidthRight = borderWidth;
		style.BorderWidthTop = borderWidth;
		style.CornerRadiusBottomLeft = cornerRadius;
		style.CornerRadiusBottomRight = cornerRadius;
		style.CornerRadiusTopLeft = cornerRadius;
		style.CornerRadiusTopRight = cornerRadius;
		style.AntiAliasing = false;
		return style;
	}

	public void addPanelStyle(string name, StyleBoxFlat style)
	{
		panelStyles[name] = style;
	}

	public void addButtonStyle(string name, ButtonVisualStyle style)
	{
		buttonStyles[name] = style;
	}

	public void addWindowStyle(string name, StyleBoxFlat style)
	{
		windowStyles[name] = style;
	}

	public void addWindowPreset(string name, WindowPreset preset)
	{
		windowPresets[name] = preset;
	}

	public void addTextStyle(string name, TextStyle style)
	{
		textStyles[name] = style;
	}

	public int addColorScheme(string name, Color[] colors)
	{
		string schemeName = string.IsNullOrWhiteSpace(name) ? "Custom Scheme" : name.Trim();
		colorSchemes.Add(new ColorScheme(schemeName, colors));
		return colorSchemes.Count - 1;
	}

	public StyleBoxFlat getPanelStyle(string name = "default")
	{
		return panelStyles.ContainsKey(name) ? panelStyles[name] : panelStyles["default"];
	}

	public ButtonVisualStyle getButtonStyle(string name = "menu")
	{
		return buttonStyles.ContainsKey(name) ? buttonStyles[name] : buttonStyles["menu"];
	}

	public StyleBoxFlat getWindowStyle(string name = "default")
	{
		return windowStyles.ContainsKey(name) ? windowStyles[name] : windowStyles["default"];
	}

	public WindowPreset getWindowPreset(string name = "closeButtonTransparentTopbar")
	{
		return windowPresets.ContainsKey(name) ? windowPresets[name] : windowPresets["closeButtonTransparentTopbar"];
	}

	public TextStyle getTextStyle(string name = "default")
	{
		return textStyles.ContainsKey(name) ? textStyles[name] : textStyles["default"];
	}

	public Font getFont(string fontName)
	{
		if (!string.IsNullOrEmpty(fontName) && fonts.ContainsKey(fontName))
		{
			return fonts[fontName];
		}
		if (fonts.ContainsKey(DefaultFontName))
		{
			return fonts[DefaultFontName];
		}
		return fonts.Count > 0 ? fonts.First().Value : null;
	}

	public Font getStyledFont(TextStyle style)
	{
		Font baseFont = getFont(style.fontName);
		if (baseFont == null || (!style.bold && !style.italic))
		{
			return baseFont;
		}

		FontVariation variation = new FontVariation();
		variation.BaseFont = baseFont;
		if (style.bold)
		{
			variation.VariationEmbolden = 0.8f;
		}
		if (style.italic)
		{
			variation.VariationTransform = new Transform2D(1f, 0f, -0.18f, 1f, 0f, 0f);
		}
		return variation;
	}

	public ColorScheme getActiveColorScheme()
	{
		return colorSchemes[activeColorSchemeIndex];
	}

	public void nextColorScheme()
	{
		activeColorSchemeIndex = (activeColorSchemeIndex + 1) % colorSchemes.Count;
		applyColorScheme(activeColorSchemeIndex);
	}

	public void applyColorScheme(int schemeIndex)
	{
		activeColorSchemeIndex = Mathf.Clamp(schemeIndex, 0, colorSchemes.Count - 1);
		ColorScheme scheme = colorSchemes[activeColorSchemeIndex];
		for (int i = 0; i < colorSchemeColorNames.Length && i < scheme.colors.Length; i++)
		{
			mAccess.colorManager.updateColor(colorSchemeColorNames[i], scheme.colors[i]);
		}
	}

	public void applyStoredGameSettings(StoredGameSettings settings, bool refresh = true)
	{
		if (settings == null)
		{
			return;
		}

		activeColorSchemeIndex = Mathf.Clamp(settings.ActiveColorSchemeIndex, 0, colorSchemes.Count - 1);
		foreach (StoredColorSchemeSettings savedScheme in settings.ColorSchemes)
		{
			Color[] colors = savedScheme.Colors
				.Select(color => color.ToColor())
				.ToArray();
			if (colors.Length == 0)
			{
				continue;
			}
			addColorScheme(savedScheme.Name, colors);
		}
		activeColorSchemeIndex = Mathf.Clamp(settings.ActiveColorSchemeIndex, 0, colorSchemes.Count - 1);
		foreach (KeyValuePair<string, StoredColorValue> savedColor in settings.Colors)
		{
			if (colorSchemeColorNames.Contains(savedColor.Key))
			{
				mAccess.colorManager.updateColor(savedColor.Key, savedColor.Value.ToColor());
			}
		}

		foreach (KeyValuePair<string, StoredTextStyleSettings> savedTextStyle in settings.TextStyles)
		{
			if (!textStyles.ContainsKey(savedTextStyle.Key))
			{
				continue;
			}

			textStyles[savedTextStyle.Key].CopyFrom(savedTextStyle.Value.ToTextStyle(textStyles[savedTextStyle.Key]));
		}

		if (refresh)
		{
			rebuildStyles();
			styleChanged?.Invoke(this, System.EventArgs.Empty);
		}
	}

	public StoredGameSettings createStoredGameSettings(float masterVolumePercent, Vector2I resolution)
	{
		StoredGameSettings settings = new StoredGameSettings
		{
			ActiveColorSchemeIndex = activeColorSchemeIndex,
			MasterVolumePercent = masterVolumePercent,
			ResolutionWidth = resolution.X,
			ResolutionHeight = resolution.Y
		};

		foreach (string colorName in colorSchemeColorNames)
		{
			settings.Colors[colorName] = new StoredColorValue(color(colorName));
		}
		foreach (KeyValuePair<string, TextStyle> textStyle in textStyles)
		{
			settings.TextStyles[textStyle.Key] = new StoredTextStyleSettings(textStyle.Value);
		}
		foreach (ColorScheme scheme in colorSchemes.Skip(BuiltInColorSchemeCount))
		{
			settings.ColorSchemes.Add(new StoredColorSchemeSettings(scheme));
		}

		return settings;
	}

	public void refreshTextStyles()
	{
		applyUniversalStyleTree(GetTree().Root, true);
		styleChanged?.Invoke(this, System.EventArgs.Empty);
	}

	public void applyPanelStyle(Control control, string styleName = "default")
	{
		control.SetMeta("styleManaged", true);
		control.SetMeta("styleType", "panel");
		control.SetMeta("styleName", styleName);
		control.AddThemeStyleboxOverride("panel", getPanelStyle(styleName));
	}

	public void applyButtonStyle(Button button, string styleName = "menu", string textStyleName = "default")
	{
		ButtonVisualStyle style = getButtonStyle(styleName);
		button.AddThemeStyleboxOverride("normal", style.normal);
		button.AddThemeStyleboxOverride("hover", style.hover);
		button.AddThemeStyleboxOverride("pressed", style.pressed);
		button.AddThemeStyleboxOverride("disabled", style.disabled);
		button.AddThemeStyleboxOverride("focus", style.hover);
		applyTextStyle(button, textStyleName);
		button.SetMeta("styleManaged", true);
		button.SetMeta("styleType", "button");
		button.SetMeta("styleName", styleName);
		button.SetMeta("buttonStyleName", styleName);
		button.SetMeta("buttonTextStyleName", textStyleName);
	}

	public void applyWindowStyle(Control control, string styleName = "default")
	{
		control.SetMeta("styleManaged", true);
		control.SetMeta("styleType", "window");
		control.SetMeta("styleName", styleName);
		control.AddThemeStyleboxOverride("panel", getWindowStyle(styleName));
	}

	public void applyTextStyle(Control control, string styleName = "default")
	{
		control.SetMeta("styleManaged", true);
		control.SetMeta("styleType", "text");
		control.SetMeta("styleName", styleName);
		TextStyle style = getTextStyle(styleName);
		Font font = getStyledFont(style);
		if (font != null)
		{
			control.AddThemeFontOverride("font", font);
		}
		control.AddThemeColorOverride("font_color", color(style.colorName));
		control.AddThemeColorOverride("font_hover_color", color(style.hoverColorName));
		control.AddThemeColorOverride("font_pressed_color", color(style.pressedColorName));
		control.AddThemeColorOverride("font_disabled_color", color(style.disabledColorName));
		control.AddThemeFontSizeOverride("font_size", style.fontSize);
		applyUnderlineStyle(control, style.underline, color(style.colorName));
	}

	public void applyUnderlineStyle(Control control, bool underline, Color underlineColor)
	{
		if (!control.HasMeta("styleUnderlineDrawConnected"))
		{
			control.Draw += () => drawUnderline(control);
			control.SetMeta("styleUnderlineDrawConnected", true);
		}

		control.SetMeta("styleUnderlineActive", underline);
		control.SetMeta("styleUnderlineColor", underlineColor);
		control.QueueRedraw();
	}

	void drawUnderline(Control control)
	{
		if (!control.GetMeta("styleUnderlineActive", false).AsBool())
		{
			return;
		}

		Color underlineColor = control.GetMeta("styleUnderlineColor", Colors.White).AsColor();
		float y = Mathf.Max(control.Size.Y - 3f, 0f);
		control.DrawLine(new Vector2(0f, y), new Vector2(control.Size.X, y), underlineColor, 2f);
	}

	public void applyUniversalStyleTree(Node root, bool forceRefresh = false)
	{
		if (!isSetupComplete)
		{
			return;
		}

		if (root is Control control)
		{
			applyUniversalStyle(control, forceRefresh);
		}

		foreach (Node child in root.GetChildren())
		{
			applyUniversalStyleTree(child, forceRefresh);
		}
	}

	public void applyUniversalStyle(Control control, bool forceRefresh = false)
	{
		if (control.HasMeta("styleManaged"))
		{
			if (forceRefresh)
			{
				refreshManagedStyle(control);
			}
			return;
		}

		if (control is Button button)
		{
			applyButtonStyle(button, "secondary");
			return;
		}

		if (control is Label label)
		{
			applyTextStyle(label, "default");
			return;
		}

		if (control is PanelContainer panelContainer)
		{
			applyPanelStyle(panelContainer, "default");
			return;
		}

		if (control is Panel panel)
		{
			applyPanelStyle(panel, "default");
			return;
		}

		Font font = getFont(DefaultFontName);
		if (font != null)
		{
			control.AddThemeFontOverride("font", font);
		}
	}

	public void applySwatchStyle(Button button, Color swatchColor, int cornerRadius = 4)
	{
		button.SetMeta("styleManaged", true);
		button.SetMeta("styleType", "swatch");
		button.SetMeta("swatchColor", swatchColor);
		button.SetMeta("swatchCornerRadius", cornerRadius);
		button.AddThemeStyleboxOverride("normal", createSwatchStyle(swatchColor, cornerRadius));
		button.AddThemeStyleboxOverride("hover", createSwatchStyle(swatchColor.Lightened(0.15f), cornerRadius));
		button.AddThemeStyleboxOverride("pressed", createSwatchStyle(swatchColor.Darkened(0.15f), cornerRadius));
		button.AddThemeStyleboxOverride("disabled", createSwatchStyle(swatchColor.Darkened(0.25f), cornerRadius));
		button.AddThemeStyleboxOverride("focus", createSwatchStyle(swatchColor.Lightened(0.15f), cornerRadius));
	}

	public StyleBoxFlat createSwatchStyle(Color swatchColor, int cornerRadius = 4)
	{
		return createBox(swatchColor, color("styleSwatchBorder"), 2, cornerRadius);
	}

	void refreshManagedStyle(Control control)
	{
		string styleType = control.GetMeta("styleType", "").AsString();
		string styleName = control.GetMeta("styleName", "default").AsString();
		if (styleType == "button" && control is Button button)
		{
			string buttonStyleName = control.GetMeta("buttonStyleName", styleName).AsString();
			string textStyleName = control.GetMeta("buttonTextStyleName", control.GetMeta("textStyleName", "default")).AsString();
			applyButtonStyle(button, buttonStyleName, textStyleName);
		}
		else if (styleType == "swatch" && control is Button swatchButton)
		{
			Color swatchColor = control.GetMeta("swatchColor", Colors.White).AsColor();
			int cornerRadius = control.GetMeta("swatchCornerRadius", 4).AsInt32();
			applySwatchStyle(swatchButton, swatchColor, cornerRadius);
		}
		else if (styleType == "text")
		{
			applyTextStyle(control, styleName);
		}
		else if (styleType == "panel")
		{
			applyPanelStyle(control, styleName);
		}
		else if (styleType == "window")
		{
			applyWindowStyle(control, styleName);
		}
	}
}

public class ButtonVisualStyle
{
	public StyleBoxFlat normal;
	public StyleBoxFlat hover;
	public StyleBoxFlat pressed;
	public StyleBoxFlat disabled;

	public ButtonVisualStyle(StyleBoxFlat normalArg, StyleBoxFlat hoverArg, StyleBoxFlat pressedArg, StyleBoxFlat disabledArg)
	{
		normal = normalArg;
		hover = hoverArg;
		pressed = pressedArg;
		disabled = disabledArg;
	}
}

public class ColorScheme
{
	public string name;
	public Color[] colors;

	public ColorScheme(string nameArg, Color[] colorsArg)
	{
		name = nameArg;
		colors = colorsArg;
	}
}

public class TextStyle
{
	public string colorName;
	public string hoverColorName;
	public string pressedColorName;
	public string disabledColorName;
	public int fontSize;
	public string fontName;
	public bool bold;
	public bool italic;
	public bool underline;

	public TextStyle(string colorNameArg, string hoverColorNameArg, string pressedColorNameArg, string disabledColorNameArg, int fontSizeArg, string fontNameArg, bool boldArg = false, bool italicArg = false, bool underlineArg = false)
	{
		colorName = colorNameArg;
		hoverColorName = hoverColorNameArg;
		pressedColorName = pressedColorNameArg;
		disabledColorName = disabledColorNameArg;
		fontSize = fontSizeArg;
		fontName = fontNameArg;
		bold = boldArg;
		italic = italicArg;
		underline = underlineArg;
	}

	public TextStyle Clone()
	{
		return new TextStyle(colorName, hoverColorName, pressedColorName, disabledColorName, fontSize, fontName, bold, italic, underline);
	}

	public void CopyFrom(TextStyle other)
	{
		colorName = other.colorName;
		hoverColorName = other.hoverColorName;
		pressedColorName = other.pressedColorName;
		disabledColorName = other.disabledColorName;
		fontSize = other.fontSize;
		fontName = other.fontName;
		bold = other.bold;
		italic = other.italic;
		underline = other.underline;
	}
}
