using Godot;
using System;
using System.Collections.Generic;

public partial class WindowManagement : managerNode
{
	public Dictionary<string, ManagedViewportWindow> windows;
	public Dictionary<string, WindowPreset> presets;
	public Dictionary<string, bool> windowCloseChecks;
	CanvasLayer windowLayer;

	public override void setup()
	{
		windows = new Dictionary<string, ManagedViewportWindow>();
		presets = new Dictionary<string, WindowPreset>();
		windowCloseChecks = new Dictionary<string, bool>();
		windowLayer = new CanvasLayer();
		windowLayer.Name = "ManagedViewportWindows";
		windowLayer.Layer = 100;
		AddChild(windowLayer);
		mAccess.styleManager.styleChanged += onStyleChanged;
		refreshStylePresets();
	}

	void onStyleChanged(object sender, EventArgs e)
	{
		refreshStylePresets();
		refreshOpenWindowStyles();
	}

	void refreshStylePresets()
	{
		presets.Clear();
		addPreset("staticMenu", mAccess.styleManager.getWindowPreset("staticMenu"));
		addPreset("resizableMenu", mAccess.styleManager.getWindowPreset("resizableMenu"));
		addPreset("closeButtonTransparentTopbar", mAccess.styleManager.getWindowPreset("closeButtonTransparentTopbar"));
		addPreset("transparentTopbarNoClose", mAccess.styleManager.getWindowPreset("transparentTopbarNoClose"));
	}

	public void addPreset(string name, WindowPreset preset)
	{
		presets[name] = preset;
	}

	public Control openWindow(string name, Control content, string presetName = "closeButtonTransparentTopbar", bool checkUnsavedOnClose = true)
	{
		return openWindow(name, content, presetName, checkUnsavedOnClose, false, Vector2.Zero);
	}

	public Control openWindowAt(string name, Control content, Vector2 globalPosition, string presetName = "closeButtonTransparentTopbar", bool checkUnsavedOnClose = true)
	{
		return openWindow(name, content, presetName, checkUnsavedOnClose, true, globalPosition);
	}

	Control openWindow(string name, Control content, string presetName, bool checkUnsavedOnClose, bool usePosition, Vector2 position)
	{
		if (windows.ContainsKey(name) && GodotObject.IsInstanceValid(windows[name]))
		{
			closeWindow(name, false);
		}

		WindowPreset preset = presets[presetName];
		ManagedViewportWindow window = createWindowRoot(name, content, presetName, preset);
		windowLayer.AddChild(window);
		windows[name] = window;
		windowCloseChecks[name] = checkUnsavedOnClose;

		window.minimumSize = getWindowMinimumSize(window, preset);
		window.CustomMinimumSize = window.minimumSize;
		window.Size = getInitialWindowSize(window, preset);

		if (preset.resizeToContents)
		{
			resizeWindowToContents(window);
		}

		if (usePosition)
		{
			window.Position = clampWindowPosition(position, window.Size);
		}
		else
		{
			window.Position = getCenteredWindowPosition(window.Size);
		}

		window.MoveToFront();
		return window;
	}

	ManagedViewportWindow createWindowRoot(string name, Control content, string presetName, WindowPreset preset)
	{
		ManagedViewportWindow root = new ManagedViewportWindow();
		root.Name = name;
		root.presetName = presetName;
		root.preset = preset;
		root.CustomMinimumSize = getRootMinimumSize(content, preset);
		root.MouseFilter = Control.MouseFilterEnum.Stop;
		root.ClipContents = false;
		root.ZIndex = windows.Count;

		if (preset.shadowSize > 0 && preset.shadowColor.A > 0f)
		{
			root.AddChild(createWindowGlow(preset));
		}

		Panel panel = new Panel();
		panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		insetControl(panel, preset.shadowSize);
		panel.SetMeta("styleManaged", true);
		panel.SetMeta("styleType", "windowInternal");
		panel.SetMeta("windowInternalType", "panel");
		panel.AddThemeStyleboxOverride("panel", createWindowStyle(preset));
		root.AddChild(panel);

		MarginContainer contentMargin = new MarginContainer();
		contentMargin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		contentMargin.AddThemeConstantOverride("margin_left", preset.padding.X + preset.shadowSize);
		contentMargin.AddThemeConstantOverride("margin_right", preset.padding.X + preset.shadowSize);
		contentMargin.AddThemeConstantOverride("margin_top", preset.padding.Y + preset.shadowSize + (preset.hasTopbar ? preset.topbarHeight : 0));
		contentMargin.AddThemeConstantOverride("margin_bottom", preset.padding.Y + preset.shadowSize);
		contentMargin.AddChild(content);
		root.AddChild(contentMargin);

		if (preset.hasTopbar)
		{
			Control topbar = createTopbar(root, preset);
			topbar.AnchorLeft = 0f;
			topbar.AnchorRight = 1f;
			topbar.OffsetLeft = preset.shadowSize;
			topbar.OffsetTop = preset.shadowSize;
			topbar.OffsetRight = -preset.shadowSize;
			topbar.OffsetBottom = preset.shadowSize + preset.topbarHeight;
			topbar.ZIndex = 1;
			root.AddChild(topbar);
		}

		if (preset.hasCloseButton)
		{
			Button closeButton = new Button();
			closeButton.Text = "X";
			closeButton.CustomMinimumSize = new Vector2(28, 28);
			mAccess.styleManager.applyButtonStyle(closeButton, "secondary");
			closeButton.AnchorLeft = 1f;
			closeButton.AnchorRight = 1f;
			closeButton.OffsetLeft = -preset.shadowSize - 34f;
			closeButton.OffsetRight = -preset.shadowSize - 6f;
			closeButton.OffsetTop = preset.shadowSize + 6f;
			closeButton.OffsetBottom = preset.shadowSize + 34f;
			closeButton.ZIndex = 2;
			closeButton.Pressed += () => closeWindow(root.Name.ToString());
			root.AddChild(closeButton);
		}

		if (preset.resizable)
		{
			root.AddChild(createResizeEdges(root));
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
		style.ShadowColor = new Color(0f, 0f, 0f, 0f);
		style.ShadowSize = 0;
		return style;
	}

	Control createWindowGlow(WindowPreset preset)
	{
		Control glowRoot = new Control();
		glowRoot.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		glowRoot.MouseFilter = Control.MouseFilterEnum.Ignore;

		int layerCount = Mathf.Max(1, preset.shadowSize / 3);
		for (int layer = 0; layer < layerCount; layer++)
		{
			float spread = layer * 3f;
			Panel glowLayer = new Panel();
			glowLayer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			insetControl(glowLayer, preset.shadowSize - spread);
			glowLayer.MouseFilter = Control.MouseFilterEnum.Ignore;
			glowLayer.SetMeta("styleManaged", true);
			glowLayer.SetMeta("styleType", "windowInternal");
			glowLayer.SetMeta("windowInternalType", "glow");
			glowLayer.SetMeta("windowGlowLayer", layer);
			glowLayer.SetMeta("windowGlowLayerCount", layerCount);
			glowLayer.AddThemeStyleboxOverride("panel", createWindowGlowStyle(preset, layer, layerCount));
			glowRoot.AddChild(glowLayer);
		}

		return glowRoot;
	}

	void refreshOpenWindowStyles()
	{
		foreach (ManagedViewportWindow window in windows.Values)
		{
			if (!GodotObject.IsInstanceValid(window))
			{
				continue;
			}
			if (!string.IsNullOrEmpty(window.presetName) && presets.ContainsKey(window.presetName))
			{
				window.preset = presets[window.presetName];
			}
			refreshWindowInternalStyles(window, window.preset);
		}
	}

	void refreshWindowInternalStyles(Node node, WindowPreset preset)
	{
		if (node is Panel panel && panel.GetMeta("styleType", "").AsString() == "windowInternal")
		{
			string internalType = panel.GetMeta("windowInternalType", "panel").AsString();
			if (internalType == "glow")
			{
				int layer = panel.GetMeta("windowGlowLayer", 0).AsInt32();
				int layerCount = panel.GetMeta("windowGlowLayerCount", 1).AsInt32();
				panel.AddThemeStyleboxOverride("panel", createWindowGlowStyle(preset, layer, layerCount));
			}
			else
			{
				panel.AddThemeStyleboxOverride("panel", createWindowStyle(preset));
			}
		}

		foreach (Node child in node.GetChildren())
		{
			refreshWindowInternalStyles(child, preset);
		}
	}

	StyleBoxFlat createWindowGlowStyle(WindowPreset preset, int layer, int layerCount)
	{
		float strength = 1f - ((float)layer / layerCount);
		Color layerColor = preset.shadowColor;
		layerColor.A *= strength * 0.45f;

		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = new Color(0f, 0f, 0f, 0f);
		style.BorderColor = layerColor;
		style.BorderWidthBottom = 2;
		style.BorderWidthLeft = 2;
		style.BorderWidthRight = 2;
		style.BorderWidthTop = 2;
		style.CornerRadiusBottomLeft = preset.cornerRadius + layer * 2;
		style.CornerRadiusBottomRight = preset.cornerRadius + layer * 2;
		style.CornerRadiusTopLeft = preset.cornerRadius + layer * 2;
		style.CornerRadiusTopRight = preset.cornerRadius + layer * 2;
		style.AntiAliasing = false;
		return style;
	}

	void insetControl(Control control, float inset)
	{
		control.OffsetLeft += inset;
		control.OffsetTop += inset;
		control.OffsetRight -= inset;
		control.OffsetBottom -= inset;
	}

	Control createTopbar(ManagedViewportWindow window, WindowPreset preset)
	{
		Control topbar = new Control();
		topbar.CustomMinimumSize = new Vector2(0, preset.topbarHeight);
		topbar.MouseFilter = Control.MouseFilterEnum.Stop;

		bool dragging = false;
		Vector2 dragStartMouse = Vector2.Zero;
		Vector2 dragStartWindow = Vector2.Zero;
		topbar.GuiInput += inputEvent =>
		{
			if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
			{
				dragging = mouseButton.Pressed;
				dragStartMouse = GetViewport().GetMousePosition();
				dragStartWindow = window.Position;
				window.MoveToFront();
				topbar.AcceptEvent();
			}
			else if (inputEvent is InputEventMouseMotion && dragging)
			{
				window.Position = clampWindowPosition(dragStartWindow + GetViewport().GetMousePosition() - dragStartMouse, window.Size);
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

		Vector2 contentMinimum = new Vector2
		(
			contentSize.X + preset.padding.X * 2 + preset.shadowSize * 2,
			contentSize.Y + preset.padding.Y * 2 + preset.shadowSize * 2 + (preset.hasTopbar ? preset.topbarHeight : 0)
		);
		return new Vector2
		(
			Mathf.Max(contentMinimum.X, preset.minimumSize.X),
			Mathf.Max(contentMinimum.Y, preset.minimumSize.Y)
		);
	}

	void resizeWindowToContents(ManagedViewportWindow window)
	{
		Vector2 minimumSize = window.GetCombinedMinimumSize();
		window.Size = new Vector2
		(
			Mathf.Max(window.minimumSize.X, Mathf.CeilToInt(minimumSize.X)),
			Mathf.Max(window.minimumSize.Y, Mathf.CeilToInt(minimumSize.Y))
		);
	}

	Vector2I getWindowMinimumSize(Control windowRoot, WindowPreset preset)
	{
		Vector2 minimum = windowRoot.CustomMinimumSize;
		if (minimum == Vector2.Zero)
		{
			minimum = windowRoot.GetCombinedMinimumSize();
		}

		return new Vector2I
		(
			Mathf.Max(preset.minimumSize.X, Mathf.CeilToInt(minimum.X)),
			Mathf.Max(preset.minimumSize.Y, Mathf.CeilToInt(minimum.Y))
		);
	}

	Vector2 getInitialWindowSize(Control windowRoot, WindowPreset preset)
	{
		Vector2I minimumSize = getWindowMinimumSize(windowRoot, preset);
		if (preset.initialSize != Vector2I.Zero)
		{
			return new Vector2
			(
				Mathf.Max(minimumSize.X, preset.initialSize.X),
				Mathf.Max(minimumSize.Y, preset.initialSize.Y)
			);
		}

		return minimumSize;
	}

	Control createResizeEdges(ManagedViewportWindow window)
	{
		Control edges = new Control();
		edges.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		edges.MouseFilter = Control.MouseFilterEnum.Ignore;
		edges.ZIndex = 3;

		edges.AddChild(createResizeEdge(window, ResizeEdge.Left));
		edges.AddChild(createResizeEdge(window, ResizeEdge.Right));
		edges.AddChild(createResizeEdge(window, ResizeEdge.Top));
		edges.AddChild(createResizeEdge(window, ResizeEdge.Bottom));

		return edges;
	}

	Control createResizeEdge(ManagedViewportWindow window, ResizeEdge edge)
	{
		Control handle = new Control();
		float thickness = 8f;
		handle.MouseFilter = Control.MouseFilterEnum.Stop;
		handle.TooltipText = "Resize";

		if (edge == ResizeEdge.Left)
		{
			handle.AnchorTop = 0f;
			handle.AnchorBottom = 1f;
			handle.OffsetLeft = 0f;
			handle.OffsetRight = thickness;
			handle.MouseDefaultCursorShape = Control.CursorShape.Hsize;
		}
		else if (edge == ResizeEdge.Right)
		{
			handle.AnchorLeft = 1f;
			handle.AnchorRight = 1f;
			handle.AnchorTop = 0f;
			handle.AnchorBottom = 1f;
			handle.OffsetLeft = -thickness;
			handle.OffsetRight = 0f;
			handle.MouseDefaultCursorShape = Control.CursorShape.Hsize;
		}
		else if (edge == ResizeEdge.Top)
		{
			handle.AnchorRight = 1f;
			handle.OffsetTop = 0f;
			handle.OffsetBottom = thickness;
			handle.MouseDefaultCursorShape = Control.CursorShape.Vsize;
		}
		else
		{
			handle.AnchorLeft = 0f;
			handle.AnchorTop = 1f;
			handle.AnchorRight = 1f;
			handle.AnchorBottom = 1f;
			handle.OffsetTop = -thickness;
			handle.OffsetBottom = 0f;
			handle.MouseDefaultCursorShape = Control.CursorShape.Vsize;
		}

		bool resizing = false;
		Vector2 resizeStartMouse = Vector2.Zero;
		Vector2 resizeStartSize = Vector2.Zero;
		Vector2 resizeStartPosition = Vector2.Zero;
		handle.GuiInput += inputEvent =>
		{
			if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
			{
				resizing = mouseButton.Pressed;
				resizeStartMouse = GetViewport().GetMousePosition();
				resizeStartSize = window.Size;
				resizeStartPosition = window.Position;
				window.MoveToFront();
				handle.AcceptEvent();
			}
			else if (inputEvent is InputEventMouseMotion && resizing)
			{
				Vector2 delta = GetViewport().GetMousePosition() - resizeStartMouse;
				resizeWindowFromEdge(window, edge, resizeStartPosition, resizeStartSize, delta);
				handle.AcceptEvent();
			}
		};

		return handle;
	}

	void resizeWindowFromEdge(ManagedViewportWindow window, ResizeEdge edge, Vector2 startPosition, Vector2 startSize, Vector2 delta)
	{
		if (edge == ResizeEdge.Right)
		{
			window.Size = new Vector2(Mathf.Max(window.minimumSize.X, startSize.X + delta.X), startSize.Y);
		}
		else if (edge == ResizeEdge.Bottom)
		{
			window.Size = new Vector2(startSize.X, Mathf.Max(window.minimumSize.Y, startSize.Y + delta.Y));
		}
		else if (edge == ResizeEdge.Left)
		{
			float newWidth = Mathf.Max(window.minimumSize.X, startSize.X - delta.X);
			float widthDelta = startSize.X - newWidth;
			window.Position = new Vector2(startPosition.X + widthDelta, startPosition.Y);
			window.Size = new Vector2(newWidth, startSize.Y);
		}
		else if (edge == ResizeEdge.Top)
		{
			float newHeight = Mathf.Max(window.minimumSize.Y, startSize.Y - delta.Y);
			float heightDelta = startSize.Y - newHeight;
			window.Position = new Vector2(startPosition.X, startPosition.Y + heightDelta);
			window.Size = new Vector2(startSize.X, newHeight);
		}

		window.Position = clampWindowPosition(window.Position, window.Size);
	}

	Vector2 getCenteredWindowPosition(Vector2 size)
	{
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float reservedTop = getReservedTopbarHeight();
		float availableHeight = Mathf.Max(0f, viewportSize.Y - reservedTop);
		return new Vector2
		(
			Mathf.Max(8f, (viewportSize.X - size.X) * 0.5f),
			Mathf.Max(reservedTop + 8f, reservedTop + (availableHeight - size.Y) * 0.5f)
		);
	}

	Vector2 clampWindowPosition(Vector2 position, Vector2 size)
	{
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float reservedTop = getReservedTopbarHeight();
		Vector2 margin = new Vector2(8, reservedTop + 8f);
		float maxX = Mathf.Max(margin.X, viewportSize.X - size.X - margin.X);
		float maxY = Mathf.Max(margin.Y, viewportSize.Y - size.Y - margin.Y);

		return new Vector2
		(
			Mathf.Clamp(position.X, margin.X, maxX),
			Mathf.Clamp(position.Y, margin.Y, maxY)
		);
	}

	float getReservedTopbarHeight()
	{
		return mAccess.uiManager == null ? 0f : mAccess.uiManager.getReservedTopbarHeight();
	}

	public void closeWindow(string name, bool checkUnsaved = true)
	{
		if (!windows.ContainsKey(name))
		{
			return;
		}

		bool windowChecksUnsaved = true;
		if (windowCloseChecks.ContainsKey(name))
		{
			windowChecksUnsaved = windowCloseChecks[name];
		}

		if (checkUnsaved && windowChecksUnsaved && mAccess.entityFrameworkManager != null)
		{
			mAccess.entityFrameworkManager.CheckUnsavedObjects(canClose =>
			{
				if (canClose)
				{
					closeWindowImmediately(name);
				}
			});
			return;
		}

		closeWindowImmediately(name);
	}

	void closeWindowImmediately(string name)
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
		windowCloseChecks.Remove(name);
	}
}

public partial class ManagedViewportWindow : Control
{
	public Vector2I minimumSize = Vector2I.Zero;
	public string presetName = "";
	public WindowPreset preset;
}

public class WindowPreset
{
	public bool hasTopbar = true;
	public bool hasCloseButton = false;
	public bool resizeToContents = false;
	public bool resizable = false;
	public Vector2I minimumSize = Vector2I.Zero;
	public Vector2I initialSize = Vector2I.Zero;
	public Vector2I padding = Vector2I.Zero;
	public int topbarHeight = 0;
	public int cornerRadius = 0;
	public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
	public Color borderColor = new Color(1f, 1f, 1f, 1f);
	public Color shadowColor = new Color(0f, 0f, 0f, 0f);
	public int shadowSize = 0;
}

public enum ResizeEdge
{
	Left,
	Right,
	Top,
	Bottom
}
