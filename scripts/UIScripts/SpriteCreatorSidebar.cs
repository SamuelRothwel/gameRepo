using Godot;
using System;
using System.Collections.Generic;

public partial class SpriteCreatorSidebar : VBoxContainer
{
    readonly Dictionary<string, Button> colorButtons = new Dictionary<string, Button>();
    GridContainer colorGrid;
    Button savedSpritesButton;

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
        MouseFilter = MouseFilterEnum.Ignore;

        colorGrid = new GridContainer();
        colorGrid.Columns = 2;
        colorGrid.CustomMinimumSize = new Vector2(120, 120);
        colorGrid.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(colorGrid);
        MoveChild(colorGrid, 0);

        savedSpritesButton = createButton("Saved Sprites");
        savedSpritesButton.CustomMinimumSize = new Vector2(127, 31);
        savedSpritesButton.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        savedSpritesButton.Pressed += toggleSavedSpritesList;
        AddChild(savedSpritesButton);
        MoveChild(savedSpritesButton, 1);

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
        mAccess.windowManager.openWindow("Saved Sprites", createSavedSpritesWindowContent());
    }

    Control createSavedSpritesWindowContent()
    {
        ScrollContainer scroll = new ScrollContainer();
        scroll.CustomMinimumSize = new Vector2(320, 420);

        VBoxContainer savedSpritesList = new VBoxContainer();
        savedSpritesList.CustomMinimumSize = new Vector2(300, 0);
        scroll.AddChild(savedSpritesList);

        List<StoredSprite> sprites = mAccess.entityFrameworkManager.GetSprites();
        if (sprites.Count == 0)
        {
            Label emptyLabel = new Label();
            emptyLabel.Text = "No saved sprites";
            savedSpritesList.AddChild(emptyLabel);
            return scroll;
        }

        foreach (StoredSprite storedSprite in sprites)
        {
            savedSpritesList.AddChild(createSavedSpriteRow(storedSprite));
        }

        return scroll;
    }

    Control createSavedSpriteRow(StoredSprite storedSprite)
    {
        Button rowButton = new Button();
        rowButton.Text = "";
        rowButton.Flat = true;
        rowButton.CustomMinimumSize = new Vector2(130, 58);
        rowButton.Pressed += () =>
        {
            mAccess.spriteCreatorManager.LoadStoredSprite(storedSprite, loaded =>
            {
                if (loaded)
                {
                    mAccess.windowManager.closeWindow("Saved Sprites", false);
                }
            });
        };

        HBoxContainer row = new HBoxContainer();
        row.CustomMinimumSize = new Vector2(130, 58);
        row.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        row.OffsetLeft = 6f;
        row.OffsetRight = -6f;
        row.MouseFilter = Control.MouseFilterEnum.Ignore;
        rowButton.AddChild(row);

        TextureRect preview = new TextureRect();
        preview.CustomMinimumSize = new Vector2(54, 54);
        preview.Texture = mAccess.spriteCreatorManager.CreateStoredSpritePreview(storedSprite);
        preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        preview.MouseFilter = Control.MouseFilterEnum.Ignore;
        row.AddChild(preview);

        Label nameLabel = new Label();
        nameLabel.Text = storedSprite.Name;
        nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
        row.AddChild(nameLabel);

        return rowButton;
    }
}
