using Godot;
using System;
using System.Collections.Generic;

public partial class SpriteScrollContainer : VBoxContainer
{
	public Vector2 spriteSize;
	public Dictionary<string, Sprite2D> sprites;
	public Vector2 marginLength;
	public int borderLength;
	public override void _Ready()
	{
		((ScrollContainer)GetParent()).MouseForcePassScrollEvents = false;
		sprites = new Dictionary<string, Sprite2D>();
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
		} else
		{
			addSprite(e.name, e.sprite);
		}
	}
	void setSprite(Sprite2D sprite)
	{
		Vector2 textureSize = sprite.Texture.GetSize();
		float textureScale = Math.Min(spriteSize.X/textureSize.X,spriteSize.Y/textureSize.Y);
		sprite.Scale = math.createVector(textureScale);
	}
	void addSprite(string Name, Texture2D texture)
	{
		Control slot = new Control();
		slot.CustomMinimumSize = spriteSize;
		AddChild(slot);

		Sprite2D sprite = new Sprite2D();
		sprite.Texture = texture;
		sprite.Centered = true;
		sprite.Position = (spriteSize+marginLength*2)/2;
		setSprite(sprite);
		slot.AddChild(sprite);
		mAccess.layerManager.addLayer(sprite, "UI2");
		sprites[Name] = sprite;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
