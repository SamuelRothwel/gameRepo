using Godot;
using System;
using System.Collections.Generic;

public partial class CreatorManagement : managerNode
{
	public Sprite2D tempSprite;
	public Sprite2D activeSprite;
	public Vector2 activeSpriteCoords;
	public float spriteScale;
	public event EventHandler<SpriteEvent> spriteChangeEvent;
	public Dictionary<string, (Sprite2D, List<Vector2>)> activeSpriteLayers;
	string activeLayer;
	public Dictionary<Godot.Key, string> keyBinds;
	public string activeFunction;
	public override void setup()
	{
		activeSpriteLayers = new Dictionary<string, (Sprite2D, List<Vector2>)>();
		keyBinds = new Dictionary<Key, string>();
		activeFunction = "draw";
	}
	public void AddSpriteLayer(string name = "", int scale = 10)
	{
		if (name == "")
		{
			name = "Sprite layer " + activeSpriteLayers.Count;
		}
		
		activeSprite.TryQueueFree();
		activeSprite = new Sprite2D();
		activeSprite.Scale = new Vector2(scale, scale);
		AddChild(activeSprite);
		activeSprite.Texture = ImageTexture.CreateFromImage(Image.Create(1, 1, false, Image.Format.Rgba8));
		activeSpriteLayers[name] = (activeSprite, new List<Vector2>());
		activeLayer = name;
		spriteScale = scale;
		activeSprite.Centered = false;
		activeSpriteCoords = math.createVector(0);
	}
	public void hover(Vector2 coord)
	{
		switch (activeFunction)
		{
			case "draw":
				Vector2 dif = (coord/spriteScale - activeSpriteCoords).Ceil();
				if (dif.X < 0 || dif.Y < 0)
				{
					Vector2 negDif = new Vector2(MathF.Min(dif.X, 0), MathF.Min(dif.Y, 0));
					dif -= negDif;
					activeSpriteCoords += negDif;
					for (int i = 0; i < activeSpriteLayers[activeLayer].Item2.Count; i++)
					{
						activeSpriteLayers[activeLayer].Item2[i] -= negDif;
					}
					activeSprite.Position = activeSpriteCoords * spriteScale;
				}
				break;
		}
	}
	public void click(Vector2 coord)
	{
		switch (activeFunction)
		{
			case "draw":
				Vector2 dif = (coord/spriteScale - activeSpriteCoords).Ceil();
				if (dif.X < 0 || dif.Y < 0)
				{
					Vector2 negDif = new Vector2(MathF.Min(dif.X, 0), MathF.Min(dif.Y, 0));
					dif -= negDif;
					activeSpriteCoords += negDif;
					for (int i = 0; i < activeSpriteLayers[activeLayer].Item2.Count; i++)
					{
						activeSpriteLayers[activeLayer].Item2[i] -= negDif;
					}
					activeSprite.Position = activeSpriteCoords * spriteScale;
				}
				activeSpriteLayers[activeLayer].Item2.Add(dif);
				activeSprite.Texture = ImageTexture.CreateFromImage(mAccess.spriteManager.vectorsToImage(activeSpriteLayers[activeLayer].Item2, mAccess.colorManager.activeColorName));
				spriteChangeEvent?.Invoke(this, new SpriteEvent(activeSprite.Texture, activeLayer));
				break;
		}
	}
	public void keyPress(Godot.Key key)
	{
		if (keyBinds.ContainsKey(key))
		{
			activeFunction = keyBinds[key];
		}
	}
}

public class SpriteEvent : EventArgs
{
	public Texture2D sprite {get;}
	public string name{get;}
	public SpriteEvent(Texture2D spriteArg, string nameArg)
	{
		sprite = spriteArg;
		name = nameArg;
	}
}
