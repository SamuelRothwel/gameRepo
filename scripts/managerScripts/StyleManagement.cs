using Godot;
using System.Collections.Generic;

public partial class StyleManagement : managerNode
{
	public Dictionary<string, StyleBoxFlat> panelStyles;
	public Dictionary<string, StyleBoxFlat> windowStyles;
	public Dictionary<string, WindowPreset> windowPresets;
	public Dictionary<string, TextStyle> textStyles;

	public override void setup()
	{
		panelStyles = new Dictionary<string, StyleBoxFlat>();
		windowStyles = new Dictionary<string, StyleBoxFlat>();
		windowPresets = new Dictionary<string, WindowPreset>();
		textStyles = new Dictionary<string, TextStyle>();

		setupColors();
		setupPanelStyles();
		setupTextStyles();
		setupWindowStyles();
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
		addColor("styleWindowShadow", new Color(0f, 0f, 0f, 0f));

		addColor("styleTextPrimary", new Color(0.93f, 0.94f, 0.95f, 1f));
		addColor("styleTextMuted", new Color(0.63f, 0.66f, 0.7f, 1f));
		addColor("styleTextAccent", new Color(0.96f, 0.72f, 0.2f, 1f));
		addColor("styleTextDanger", new Color(0.95f, 0.28f, 0.22f, 1f));
		addColor("styleTextDisabled", new Color(0.38f, 0.4f, 0.43f, 1f));
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

		addWindowPreset("closeButtonTransparentTopbar", new WindowPreset
		{
			hasTopbar = true,
			hasCloseButton = true,
			resizeToContents = true,
			padding = new Vector2I(12, 12),
			topbarHeight = 34,
			cornerRadius = 10,
			backgroundColor = color("styleWindowBackground"),
			borderColor = color("styleWindowBorder"),
			shadowColor = color("styleWindowShadow"),
			shadowSize = 0
		});
		addWindowPreset("transparentTopbarNoClose", new WindowPreset
		{
			hasTopbar = true,
			hasCloseButton = false,
			resizeToContents = true,
			padding = new Vector2I(12, 12),
			topbarHeight = 34,
			cornerRadius = 10,
			backgroundColor = color("styleWindowBackground"),
			borderColor = color("styleWindowBorder"),
			shadowColor = color("styleWindowShadow"),
			shadowSize = 0
		});
	}

	void addColor(string name, Color value)
	{
		mAccess.colorManager.addColor(name, value);
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
