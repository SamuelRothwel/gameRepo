using Godot;
using System;
using System.Collections.Generic;

public partial class SpriteCreatorSidebar : VBoxContainer
{
    readonly Dictionary<string, Button> colorButtons = new Dictionary<string, Button>();
    GridContainer colorGrid;

    readonly string[] colorSlots = new string[]
    {
        "colorSlot0",
        "colorSlot1",
        "colorSlot2",
        "colorSlot3",
    };

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(150, 0);

        colorGrid = new GridContainer();
        colorGrid.Columns = 2;
        colorGrid.CustomMinimumSize = new Vector2(120, 120);
        AddChild(colorGrid);
        MoveChild(colorGrid, 0);

        foreach (string colorName in colorSlots)
        {
            Button button = createColorButton(colorName, mAccess.colorManager.getColor(colorName));
            button.GuiInput += (InputEvent inputEvent) => onColorButtonInput(inputEvent, colorName, button);
            colorButtons[colorName] = button;
            colorGrid.AddChild(button);
        }

        mAccess.colorManager.colorChanged += onColorChanged;
    }

    public override void _ExitTree()
    {
        if (mAccess.colorManager != null)
        {
            mAccess.colorManager.colorChanged -= onColorChanged;
        }
    }

    Button createButton(string label)
    {
        Button button = new Button();
        button.Text = label;
        button.CustomMinimumSize = new Vector2(54, 54);
        return button;
    }

    Button createColorButton(string label, Color color)
    {
        Button button = createButton("");
        button.TooltipText = label;
        setButtonColor(button, color);
        return button;
    }

    void setButtonColor(Button button, Color color)
    {
        StyleBoxFlat normal = createSwatchStyle(color);
        StyleBoxFlat hover = createSwatchStyle(color.Lightened(0.15f));
        StyleBoxFlat pressed = createSwatchStyle(color.Darkened(0.15f));
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
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

    void onColorButtonInput(InputEvent inputEvent, string colorName, Button colorButton)
    {
        if (inputEvent is InputEventMouseButton button && button.Pressed)
        {
            if (button.ButtonIndex == MouseButton.Left)
            {
                mAccess.colorManager.setActiveColorName(colorName);
                AcceptEvent();
            }
            else if (button.ButtonIndex == MouseButton.Right)
            {
                Vector2 pickerPosition = new Vector2(GlobalPosition.X + Size.X + 16f, colorButton.GlobalPosition.Y);
                mAccess.colorManager.openColorPicker(this, colorName, pickerPosition);
                AcceptEvent();
            }
        }
    }

    void onColorChanged(object sender, ColorChangedEvent e)
    {
        if (colorButtons.ContainsKey(e.name))
        {
            setButtonColor(colorButtons[e.name], e.color);
        }
    }
}
