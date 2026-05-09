using Godot;
using System;
using System.Collections.Generic;

public partial class WindowManagement : managerNode
{
	public Dictionary<string, Window> windows;
	public Dictionary<string, WindowPreset> presets;

	public override void setup()
	{
		windows = new Dictionary<string, Window>();
		presets = new Dictionary<string, WindowPreset>();
		addPreset("closeButtonTransparentTopbar", new WindowPreset
		{
			hasTopbar = true,
			hasCloseButton = true,
			resizeToContents = true,
			padding = new Vector2I(12, 12),
			topbarHeight = 34,
			cornerRadius = 10,
			backgroundColor = new Color(0.18f, 0.19f, 0.21f, 1f),
			borderColor = new Color(0.72f, 0.75f, 0.8f, 0.28f),
			shadowColor = new Color(0f, 0f, 0f, 0f),
			shadowSize = 0
		});
	}

	public void addPreset(string name, WindowPreset preset)
	{
		presets[name] = preset;
	}

	public Window openWindow(string name, Control content, string presetName = "closeButtonTransparentTopbar")
	{
		if (windows.ContainsKey(name) && GodotObject.IsInstanceValid(windows[name]))
		{
			windows[name].QueueFree();
		}

		WindowPreset preset = presets[presetName];
		Window window = new Window();
		window.Name = name;
		window.Title = name;
		window.Borderless = true;
		window.TransparentBg = true;
		window.CloseRequested += () => closeWindow(name);

		Control windowRoot = createWindowRoot(window, content, preset);
		window.AddChild(windowRoot);
		AddChild(window);
		windows[name] = window;

		if (preset.resizeToContents)
		{
			resizeWindowToContents(window, windowRoot, preset.padding);
		}

		window.PopupCentered();
		return window;
	}

	Control createWindowRoot(Window window, Control content, WindowPreset preset)
	{
		Control root = new Control();
		root.CustomMinimumSize = getRootMinimumSize(content, preset);
		root.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		Panel panel = new Panel();
		panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		panel.AddThemeStyleboxOverride("panel", createWindowStyle(preset));
		root.AddChild(panel);

		MarginContainer contentMargin = new MarginContainer();
		contentMargin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		contentMargin.AddThemeConstantOverride("margin_left", preset.padding.X);
		contentMargin.AddThemeConstantOverride("margin_right", preset.padding.X);
		contentMargin.AddThemeConstantOverride("margin_top", preset.padding.Y + (preset.hasTopbar ? preset.topbarHeight : 0));
		contentMargin.AddThemeConstantOverride("margin_bottom", preset.padding.Y);
		contentMargin.AddChild(content);
		root.AddChild(contentMargin);

		if (preset.hasTopbar)
		{
			Control topbar = createTopbar(window, preset);
			topbar.AnchorRight = 1f;
			topbar.OffsetBottom = preset.topbarHeight;
			topbar.ZIndex = 1;
			root.AddChild(topbar);
		}

		if (preset.hasCloseButton)
		{
			Button closeButton = new Button();
			closeButton.Text = "X";
			closeButton.CustomMinimumSize = new Vector2(28, 28);
			closeButton.AnchorLeft = 1f;
			closeButton.AnchorRight = 1f;
			closeButton.OffsetLeft = -34f;
			closeButton.OffsetRight = -6f;
			closeButton.OffsetTop = 6f;
			closeButton.OffsetBottom = 34f;
			closeButton.ZIndex = 2;
			closeButton.Pressed += () => closeWindow(window.Name.ToString());
			root.AddChild(closeButton);
		}

		return root;
	}

	StyleBoxFlat createWindowStyle(WindowPreset preset)
	{
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = preset.backgroundColor;
		style.BorderColor = preset.borderColor;
		style.BorderWidthBottom = 1;
		style.BorderWidthLeft = 1;
		style.BorderWidthRight = 1;
		style.BorderWidthTop = 1;
		style.CornerRadiusBottomLeft = preset.cornerRadius;
		style.CornerRadiusBottomRight = preset.cornerRadius;
		style.CornerRadiusTopLeft = preset.cornerRadius;
		style.CornerRadiusTopRight = preset.cornerRadius;
		style.AntiAliasing = false;
		style.ShadowColor = preset.shadowColor;
		style.ShadowSize = preset.shadowSize;
		return style;
	}

	Control createTopbar(Window window, WindowPreset preset)
	{
		Control topbar = new Control();
		topbar.CustomMinimumSize = new Vector2(0, preset.topbarHeight);
		topbar.MouseFilter = Control.MouseFilterEnum.Stop;

		bool dragging = false;
		Vector2I dragStartMouse = Vector2I.Zero;
		Vector2I dragStartWindow = Vector2I.Zero;
		topbar.GuiInput += inputEvent =>
		{
			if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
			{
				dragging = mouseButton.Pressed;
				dragStartMouse = DisplayServer.MouseGetPosition();
				dragStartWindow = window.Position;
			}
			else if (inputEvent is InputEventMouseMotion && dragging)
			{
				window.Position = dragStartWindow + DisplayServer.MouseGetPosition() - dragStartMouse;
			}
		};

		return topbar;
	}

	Vector2 getRootMinimumSize(Control content, WindowPreset preset)
	{
		Vector2 contentSize = content.CustomMinimumSize;
		if (contentSize == Vector2.Zero)
		{
			contentSize = content.GetCombinedMinimumSize();
		}

		return new Vector2
		(
			contentSize.X + preset.padding.X * 2,
			contentSize.Y + preset.padding.Y * 2 + (preset.hasTopbar ? preset.topbarHeight : 0)
		);
	}

	void resizeWindowToContents(Window window, Control content, Vector2I padding)
	{
		Vector2 minimumSize = content.GetCombinedMinimumSize();
		window.Size = new Vector2I
		(
			Mathf.CeilToInt(minimumSize.X),
			Mathf.CeilToInt(minimumSize.Y)
		);
	}

	public void closeWindow(string name)
	{
		if (!windows.ContainsKey(name))
		{
			return;
		}

		if (GodotObject.IsInstanceValid(windows[name]))
		{
			windows[name].QueueFree();
		}

		windows.Remove(name);
	}
}

public class WindowPreset
{
	public bool hasTopbar = true;
	public bool hasCloseButton = false;
	public bool resizeToContents = false;
	public Vector2I padding = Vector2I.Zero;
	public int topbarHeight = 0;
	public int cornerRadius = 0;
	public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
	public Color borderColor = new Color(1f, 1f, 1f, 1f);
	public Color shadowColor = new Color(0f, 0f, 0f, 0f);
	public int shadowSize = 0;
}
