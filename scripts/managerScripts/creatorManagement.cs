using Godot;
using System;
using System.Collections.Generic;

public partial class creatorManagement : managerNode
{
	public Sprite2D activeSprite;
	public List<Sprite2D> spriteLayers;
	public override void setup()
	{
		spriteLayers = new List<Sprite2D>();
	}
	public void AddSpriteLayer()
	{
		activeSprite = new Sprite2D();
		spriteLayers.Add(activeSprite);
		AddChild(activeSprite);
		activeSprite.Texture = ImageTexture.CreateFromImage(mAccess.spriteManager.mapToImage(EnumerableExtensions.Repeat<int>(1, 100, 100)));
	}
}
