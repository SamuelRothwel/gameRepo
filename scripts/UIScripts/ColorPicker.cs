using Godot;
using System;

public partial class ColorPicker : Control
{
    TextureRect wheel;
    ShaderMaterial wheelMaterial;
    ColorRect preview;
    HSlider brightnessBar;
    HSlider translucenceBar;
    GridContainer recentColorMenu;
    GridContainer savedColorMenu;
    PanelContainer savedColorsPanel;
    Button expandButton;
    bool pickingWheel;
    bool syncingControls;
    bool ignoreOutsideClick;
    readonly Vector2 pickerSize = new Vector2(160, 160);

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;

        VBoxContainer layout = new VBoxContainer();
        layout.CustomMinimumSize = new Vector2(180, 360);
        AddChild(layout);

        recentColorMenu = new GridContainer();
        recentColorMenu.Columns = 4;
        recentColorMenu.CustomMinimumSize = new Vector2(180, 72);
        layout.AddChild(recentColorMenu);

        wheel = new TextureRect();
        wheel.CustomMinimumSize = pickerSize;
        wheel.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        wheel.StretchMode = TextureRect.StretchModeEnum.Scale;
        wheel.MouseFilter = MouseFilterEnum.Stop;
        wheel.Texture = ImageTexture.CreateFromImage(Image.Create(1, 1, false, Image.Format.Rgba8));
        wheelMaterial = new ShaderMaterial();
        wheelMaterial.Shader = GD.Load<Shader>("res://scripts/scriptSets/ShaderScripts/HSVShader.gdshader");
        wheel.Material = wheelMaterial;
        layout.AddChild(wheel);

        brightnessBar = createBar(1f);
        layout.AddChild(brightnessBar);

        translucenceBar = createBar(1f);
        layout.AddChild(translucenceBar);

        preview = new ColorRect();
        preview.CustomMinimumSize = new Vector2(180, 24);
        layout.AddChild(preview);

        HBoxContainer actionBar = new HBoxContainer();
        layout.AddChild(actionBar);

        Button saveButton = new Button();
        saveButton.Text = "Save";
        saveButton.CustomMinimumSize = new Vector2(86, 30);
        saveButton.Pressed += saveColor;
        actionBar.AddChild(saveButton);

        expandButton = new Button();
        expandButton.Text = "All";
        expandButton.CustomMinimumSize = new Vector2(86, 30);
        expandButton.Pressed += toggleSavedColors;
        actionBar.AddChild(expandButton);

        brightnessBar.ValueChanged += onBrightnessChanged;
        translucenceBar.ValueChanged += onTranslucenceChanged;
        wheel.GuiInput += onWheelInput;
        mAccess.colorManager.colorChanged += onColorChanged;
        mAccess.colorManager.colorLibraryChanged += onColorLibraryChanged;
        syncControls();
        rebuildRecentColorMenu();
    }

    public override void _ExitTree()
    {
        if (mAccess.colorManager != null)
        {
            mAccess.colorManager.colorChanged -= onColorChanged;
            mAccess.colorManager.colorLibraryChanged -= onColorLibraryChanged;
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (!Visible)
        {
            return;
        }
        if (inputEvent is InputEventMouseButton button && button.Pressed)
        {
            if (ignoreOutsideClick)
            {
                ignoreOutsideClick = false;
                return;
            }

            Vector2 mousePosition = GetGlobalMousePosition();
            bool insidePicker = new Rect2(GlobalPosition, Size).HasPoint(mousePosition);
            bool insideSavedColors = savedColorsPanel != null && savedColorsPanel.Visible &&
                new Rect2(savedColorsPanel.GlobalPosition, savedColorsPanel.Size).HasPoint(mousePosition);

            if (!insidePicker && !insideSavedColors)
            {
                closePicker();
            }
        }
    }

    void onWheelInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton button && button.ButtonIndex == MouseButton.Left)
        {
            pickingWheel = button.Pressed;
            if (pickingWheel)
            {
                updateWheelSelection(button.Position);
                AcceptEvent();
            }
        }
        else if (inputEvent is InputEventMouseMotion motion && pickingWheel)
        {
            updateWheelSelection(motion.Position);
            AcceptEvent();
        }
    }

    void updateWheelSelection(Vector2 position)
    {
        Vector2 center = wheel.Size * 0.5f;
        Vector2 offset = position - center;
        float radius = Mathf.Min(offset.Length() / (wheel.Size.X * 0.5f), 1f);
        float hue = (Mathf.Atan2(offset.Y, offset.X) + Mathf.Pi) / Mathf.Tau;

        mAccess.colorManager.setPicker(
            hue,
            radius,
            (float)brightnessBar.Value,
            (float)translucenceBar.Value);
    }

    HSlider createBar(float value)
    {
        HSlider bar = new HSlider();
        bar.MinValue = 0;
        bar.MaxValue = 1;
        bar.Step = 0.01;
        bar.Value = value;
        bar.CustomMinimumSize = new Vector2(180, 24);
        return bar;
    }

    void onBrightnessChanged(double value)
    {
        if (syncingControls)
        {
            return;
        }
        mAccess.colorManager.setPickerBrightness((float)value);
        updateMaterial();
    }

    void onTranslucenceChanged(double value)
    {
        if (syncingControls)
        {
            return;
        }
        mAccess.colorManager.setPickerTransluscence((float)value);
    }

    void onColorChanged(object sender, ColorChangedEvent e)
    {
        if (e.name == mAccess.colorManager.pickerColorName || e.name == mAccess.colorManager.activeColorName)
        {
            updatePreview(e.color);
        }
        if (mAccess.colorManager.savedColorNames.Contains(e.name))
        {
            rebuildRecentColorMenu();
            rebuildSavedColorMenu();
        }
    }

    void onColorLibraryChanged(object sender, EventArgs e)
    {
        rebuildRecentColorMenu();
        rebuildSavedColorMenu();
    }

    void updateMaterial()
    {
        wheelMaterial.SetShaderParameter("value", (float)brightnessBar.Value);
    }

    void updatePreview(Color color)
    {
        preview.Color = color;
    }

    void saveColor()
    {
        mAccess.colorManager.savePickerColor();
        rebuildRecentColorMenu();
        rebuildSavedColorMenu();
    }

    void toggleSavedColors()
    {
        ensureSavedColorsPanel();
        savedColorsPanel.Visible = !savedColorsPanel.Visible;
        updateSavedColorsPanelPosition();
        keepSavedColorsPanelOnScreen();
        rebuildSavedColorMenu();
    }

    void rebuildRecentColorMenu()
    {
        if (recentColorMenu == null)
        {
            return;
        }
        rebuildColorGrid(recentColorMenu, mAccess.colorManager.recentColorNames);
    }

    void rebuildSavedColorMenu()
    {
        if (savedColorMenu == null)
        {
            return;
        }
        rebuildColorGrid(savedColorMenu, mAccess.colorManager.savedColorNames);
    }

    void rebuildColorGrid(GridContainer menu, System.Collections.Generic.IEnumerable<string> colorNames)
    {
        foreach (Node child in menu.GetChildren())
        {
            menu.RemoveChild(child);
            child.QueueFree();
        }

        foreach (string colorName in colorNames)
        {
            menu.AddChild(createColorButton(colorName, mAccess.colorManager.getColor(colorName)));
        }
    }

    void ensureSavedColorsPanel()
    {
        if (savedColorsPanel != null && GodotObject.IsInstanceValid(savedColorsPanel))
        {
            return;
        }

        savedColorsPanel = new PanelContainer();
        savedColorsPanel.Visible = false;
        savedColorsPanel.CustomMinimumSize = new Vector2(180, 180);
        (GetParent() ?? this).AddChild(savedColorsPanel);

        savedColorMenu = new GridContainer();
        savedColorMenu.Columns = 4;
        savedColorMenu.CustomMinimumSize = new Vector2(170, 170);
        savedColorsPanel.AddChild(savedColorMenu);
    }

    void updateSavedColorsPanelPosition()
    {
        if (savedColorsPanel != null)
        {
            savedColorsPanel.GlobalPosition = new Vector2(GlobalPosition.X + Size.X + 12f, GlobalPosition.Y);
        }
    }

    void keepSavedColorsPanelOnScreen()
    {
        if (savedColorsPanel == null)
        {
            return;
        }
        savedColorsPanel.GlobalPosition = clampToViewport(savedColorsPanel.GlobalPosition, savedColorsPanel.Size);
    }

    Button createColorButton(string colorName, Color color)
    {
        Button button = new Button();
        button.CustomMinimumSize = new Vector2(38, 28);
        button.TooltipText = colorName;
        setButtonColor(button, color);
        button.Pressed += () => {
            mAccess.colorManager.applySavedColor(colorName);
            syncControls();
        };
        return button;
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
        return style;
    }

    public void openForColor(string colorName)
    {
        mAccess.colorManager.setPickerColorName(colorName);
        syncControls();
        rebuildRecentColorMenu();
        rebuildSavedColorMenu();
        updateSavedColorsPanelPosition();
        keepSavedColorsPanelOnScreen();
        Visible = true;
        ignoreOutsideClick = true;
        KeepOnScreen();
    }

    void syncControls()
    {
        syncingControls = true;
        brightnessBar.Value = mAccess.colorManager.picker.brightness;
        translucenceBar.Value = mAccess.colorManager.picker.transluscence;
        syncingControls = false;
        updateMaterial();
        updatePreview(mAccess.colorManager.getColor(mAccess.colorManager.pickerColorName));
    }

    public void KeepOnScreen()
    {
        GlobalPosition = clampToViewport(GlobalPosition, Size);
        updateSavedColorsPanelPosition();
        keepSavedColorsPanelOnScreen();
    }

    Vector2 clampToViewport(Vector2 position, Vector2 size)
    {
        Rect2 viewportRect = GetViewportRect();
        Vector2 margin = new Vector2(8, 8);
        Vector2 effectiveSize = new Vector2(Mathf.Max(size.X, 1f), Mathf.Max(size.Y, 1f));
        float maxX = Mathf.Max(margin.X, viewportRect.Size.X - effectiveSize.X - margin.X);
        float maxY = Mathf.Max(margin.Y, viewportRect.Size.Y - effectiveSize.Y - margin.Y);
        return new Vector2(
            Mathf.Clamp(position.X, margin.X, maxX),
            Mathf.Clamp(position.Y, margin.Y, maxY));
    }

    void closePicker()
    {
        mAccess.colorManager.commitPickerColorToRecent();
        rebuildRecentColorMenu();
        if (savedColorsPanel != null)
        {
            savedColorsPanel.Visible = false;
        }
        Visible = false;
    }
}
