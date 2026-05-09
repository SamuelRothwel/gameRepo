using Godot;
using System;
using System.Collections.Generic;

public partial class SpriteCreatorSidebar : VBoxContainer
{
    readonly Dictionary<string, Button> colorButtons = new Dictionary<string, Button>();
    GridContainer colorGrid;
    Button savedSpritesButton;
    ScrollContainer savedSpritesScroll;
    VBoxContainer savedSpritesList;

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

        savedSpritesButton = createButton("Saved Sprites");
        savedSpritesButton.CustomMinimumSize = new Vector2(127, 31);
        savedSpritesButton.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        savedSpritesButton.Pressed += toggleSavedSpritesList;
        AddChild(savedSpritesButton);
        MoveChild(savedSpritesButton, 1);

        savedSpritesScroll = new ScrollContainer();
        savedSpritesScroll.CustomMinimumSize = new Vector2(140, 240);
        savedSpritesScroll.Visible = false;
        savedSpritesList = new VBoxContainer();
        savedSpritesScroll.AddChild(savedSpritesList);
        AddChild(savedSpritesScroll);
        MoveChild(savedSpritesScroll, 2);

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

    void toggleSavedSpritesList()
    {
        savedSpritesScroll.Visible = !savedSpritesScroll.Visible;
        if (savedSpritesScroll.Visible)
        {
            populateSavedSpritesList();
        }
    }

    void populateSavedSpritesList()
    {
        foreach (Node child in savedSpritesList.GetChildren())
        {
            child.QueueFree();
        }

        List<StoredSprite> sprites = mAccess.entityFrameworkManager.GetSprites();
        if (sprites.Count == 0)
        {
            Label emptyLabel = new Label();
            emptyLabel.Text = "No saved sprites";
            savedSpritesList.AddChild(emptyLabel);
            return;
        }

        foreach (StoredSprite storedSprite in sprites)
        {
            savedSpritesList.AddChild(createSavedSpriteRow(storedSprite));
        }
    }

    Control createSavedSpriteRow(StoredSprite storedSprite)
    {
        HBoxContainer row = new HBoxContainer();
        row.CustomMinimumSize = new Vector2(130, 58);

        TextureRect preview = new TextureRect();
        preview.CustomMinimumSize = new Vector2(54, 54);
        preview.Texture = mAccess.spriteCreatorManager.CreateStoredSpritePreview(storedSprite);
        preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        row.AddChild(preview);

        Label nameLabel = new Label();
        nameLabel.Text = storedSprite.Name;
        nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(nameLabel);

        return row;
    }
}
