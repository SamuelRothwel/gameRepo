using Godot;
using System.Collections.Generic;

public partial class StyleManagement : managerNode
{
	public event System.EventHandler styleChanged;
	public Dictionary<string, StyleBoxFlat> panelStyles;
	public Dictionary<string, StyleBoxFlat> windowStyles;
	public Dictionary<string, WindowPreset> windowPresets;
	public Dictionary<string, TextStyle> textStyles;
	public System.Collections.Generic.List<ColorScheme> colorSchemes;
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
		"styleTextAccent"
	};

	public override void setup()
	{
		panelStyles = new Dictionary<string, StyleBoxFlat>();
		windowStyles = new Dictionary<string, StyleBoxFlat>();
		windowPresets = new Dictionary<string, WindowPreset>();
		textStyles = new Dictionary<string, TextStyle>();

		setupColors();
		setupColorSchemes();
		setupPanelStyles();
		setupTextStyles();
		setupWindowStyles();
		mAccess.colorManager.colorChanged += onColorChanged;
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
		addColor("styleWindowShadow", new Color(0f, 1f, 0f, 0f));

		addColor("styleTextPrimary", new Color(0.93f, 0.94f, 0.95f, 1f));
		addColor("styleTextMuted", new Color(0.63f, 0.66f, 0.7f, 1f));
		addColor("styleTextAccent", new Color(0.96f, 0.72f, 0.2f, 1f));
		addColor("styleTextDanger", new Color(0.95f, 0.28f, 0.22f, 1f));
		addColor("styleTextDisabled", new Color(0.38f, 0.4f, 0.43f, 1f));
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
			color("styleTextAccent")
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
			new Color(0.32f, 0.72f, 0.95f, 1f)
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
			new Color(0.95f, 0.48f, 0.18f, 1f)
		}));
		activeColorSchemeIndex = 0;
	}

	void setupPanelStyles()
	{
		addPanelStyle("default", createBox("stylePanelBackground", "stylePanelBorder", 1, 6));
		addPanelStyle("subtle", createBox("stylePanelSubtleBackground", "stylePanelBorder", 1, 4));
		addPanelStyle("raised", createBox("stylePanelRaisedBackground", "stylePanelStrongBorder", 1, 8));
	}

	void setupTextStyles()
	{
		addTextStyle("default", new TextStyle("styleTextPrimary", "styleTextAccent", "styleTextMuted", "styleTextDisabled", 16));
		addTextStyle("muted", new TextStyle("styleTextMuted", "styleTextPrimary", "styleTextMuted", "styleTextDisabled", 14));
		addTextStyle("accent", new TextStyle("styleTextAccent", "styleTextPrimary", "styleTextMuted", "styleTextDisabled", 16));
		addTextStyle("danger", new TextStyle("styleTextDanger", "styleTextPrimary", "styleTextMuted", "styleTextDisabled", 16));
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
			shadowSize = 12
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
			shadowSize = 12
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
			shadowSize = 12
		});
	}

	void addColor(string name, Color value)
	{
		mAccess.colorManager.addColor(name, value);
	}

	void onColorChanged(object sender, ColorChangedEvent e)
	{
		if (System.Array.IndexOf(colorSchemeColorNames, e.name) == -1 && e.name != "styleWindowShadow")
		{
			return;
		}

		rebuildStyles();
		styleChanged?.Invoke(this, System.EventArgs.Empty);
	}

	void rebuildStyles()
	{
		panelStyles.Clear();
		windowStyles.Clear();
		windowPresets.Clear();
		setupPanelStyles();
		setupWindowStyles();
	}

	Color color(string name)
	{
		return mAccess.colorManager.getColor(name);
	}

	StyleBoxFlat createBox(string backgroundColorName, string borderColorName, int borderWidth, int cornerRadius)
	{
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = color(backgroundColorName);
		style.BorderColor = color(borderColorName);
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

	public StyleBoxFlat getPanelStyle(string name = "default")
	{
		return panelStyles.ContainsKey(name) ? panelStyles[name] : panelStyles["default"];
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

	public void applyPanelStyle(Control control, string styleName = "default")
	{
		control.AddThemeStyleboxOverride("panel", getPanelStyle(styleName));
	}

	public void applyWindowStyle(Control control, string styleName = "default")
	{
		control.AddThemeStyleboxOverride("panel", getWindowStyle(styleName));
	}

	public void applyTextStyle(Control control, string styleName = "default")
	{
		TextStyle style = getTextStyle(styleName);
		control.AddThemeColorOverride("font_color", color(style.colorName));
		control.AddThemeColorOverride("font_hover_color", color(style.hoverColorName));
		control.AddThemeColorOverride("font_pressed_color", color(style.pressedColorName));
		control.AddThemeColorOverride("font_disabled_color", color(style.disabledColorName));
		control.AddThemeFontSizeOverride("font_size", style.fontSize);
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

	public TextStyle(string colorNameArg, string hoverColorNameArg, string pressedColorNameArg, string disabledColorNameArg, int fontSizeArg)
	{
		colorName = colorNameArg;
		hoverColorName = hoverColorNameArg;
		pressedColorName = pressedColorNameArg;
		disabledColorName = disabledColorNameArg;
		fontSize = fontSizeArg;
	}
}
