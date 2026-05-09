using Godot;
using System;
using System.Collections.Generic;

public partial class SpriteScrollContainer : VBoxContainer
{
	const float DragThreshold = 6f;
	public Vector2 spriteSize;
	public Dictionary<string, Sprite2D> sprites;
	public Dictionary<string, Control> spriteSlots;
	public Vector2 marginLength;
	public int borderLength;
	string draggedSpriteName;
	Control draggedSlot;
	Vector2 dragStartPosition;
	bool dragMoved;
	public override void _Ready()
	{
		((ScrollContainer)GetParent()).MouseForcePassScrollEvents = false;
		sprites = new Dictionary<string, Sprite2D>();
		spriteSlots = new Dictionary<string, Control>();
		spriteSize = math.createVector(180);
		marginLength = math.createVector(10);
		borderLength = 5;
		mAccess.creatorManager.spriteChangeEvent += spriteChange;
	}
	void spriteChange(object sender, SpriteEvent e)
	{
		if (sprites.ContainsKey(e.name))
		{
			sprites[e.name].Texture = e.sprite;
			setSprite(sprites[e.name]);
			setSlotOrder(e.name, e.order);
		} else
		{
			addSprite(e.name, e.sprite, e.order);
		}
	}
	void setSprite(Sprite2D sprite)
	{
		Vector2 textureSize = sprite.Texture.GetSize();
		float textureScale = Math.Min(spriteSize.X/textureSize.X,spriteSize.Y/textureSize.Y);
		sprite.Scale = math.createVector(textureScale);
	}
	void addSprite(string Name, Texture2D texture, int order)
	{
		Button slot = new Button();
		slot.CustomMinimumSize = spriteSize + marginLength * 2;
		slot.Text = "";
		slot.Flat = true;
		slot.GuiInput += inputEvent => OnSlotGuiInput(Name, slot, inputEvent);
		AddChild(slot);

		Sprite2D sprite = new Sprite2D();
		sprite.Texture = texture;
		sprite.Centered = true;
		sprite.Position = slot.CustomMinimumSize / 2;
		setSprite(sprite);
		slot.AddChild(sprite);
		mAccess.layerManager.addLayer(sprite, "UI2");
		sprites[Name] = sprite;
		spriteSlots[Name] = slot;
		setSlotOrder(Name, order);
	}
	void setSlotOrder(string Name, int order)
	{
		if (!spriteSlots.ContainsKey(Name))
		{
			return;
		}

		if (order < 0)
		{
			order = mAccess.creatorManager.GetSpriteLayerOrder(Name);
		}

		if (order >= 0)
		{
			MoveChild(spriteSlots[Name], order);
		}
	}
	void OnSlotGuiInput(string Name, Control slot, InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
		{
			draggedSpriteName = Name;
			draggedSlot = slot;
			dragStartPosition = mouseButton.GlobalPosition;
			dragMoved = false;
			mAccess.creatorManager.SetActiveSpriteLayer(Name);
		}
	}
	public override void _Input(InputEvent inputEvent)
	{
		if (draggedSlot == null)
		{
			return;
		}

		if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed)
		{
			FinishDrag();
		}
		else if (inputEvent is InputEventMouseMotion mouseMotion)
		{
			UpdateDrag(mouseMotion.GlobalPosition);
		}
	}
	void UpdateDrag(Vector2 globalPosition)
	{
		if (!dragMoved && dragStartPosition.DistanceTo(globalPosition) < DragThreshold)
		{
			return;
		}

		dragMoved = true;
		int order = GetSlotOrderFromGlobalY(globalPosition.Y);
		if (order >= 0 && order != draggedSlot.GetIndex())
		{
			MoveChild(draggedSlot, order);
		}
	}
	void FinishDrag()
	{
		if (dragMoved)
		{
			mAccess.creatorManager.MoveSpriteLayer(draggedSpriteName, draggedSlot.GetIndex());
		}
		else
		{
			mAccess.creatorManager.SetActiveSpriteLayer(draggedSpriteName);
		}

		draggedSpriteName = null;
		draggedSlot = null;
		dragMoved = false;
	}
	int GetSlotOrderFromGlobalY(float globalY)
	{
		for (int i = 0; i < GetChildCount(); i++)
		{
			Control slot = GetChild<Control>(i);
			if (globalY < slot.GlobalPosition.Y + slot.Size.Y / 2f)
			{
				return i;
			}
		}

		return GetChildCount() - 1;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
