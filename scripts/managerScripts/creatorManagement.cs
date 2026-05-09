using Godot;
using System;
using System.Collections.Generic;

public partial class CreatorManagement : managerNode
{
	const float InactiveLayerAlphaMultiplier = 0.5f;
	public Sprite2D tempSprite;
	public Sprite2D activeSprite;
	public Vector2 activeSpriteCoords;
	public float spriteScale;
	public event EventHandler<SpriteEvent> spriteChangeEvent;
	public Dictionary<string, (Sprite2D, List<Vector2>)> activeSpriteLayers;
	public Dictionary<string, Vector2> activeSpriteLayerCoords;
	public Dictionary<string, Color> activeSpriteLayerColors;
	public List<string> activeSpriteLayerOrder;
	string activeLayer;
	public Dictionary<Godot.Key, string> keyBinds;
	public string activeFunction;
	public override void setup()
	{
		activeSpriteLayers = new Dictionary<string, (Sprite2D, List<Vector2>)>();
		activeSpriteLayerCoords = new Dictionary<string, Vector2>();
		activeSpriteLayerColors = new Dictionary<string, Color>();
		activeSpriteLayerOrder = new List<string>();
		keyBinds = new Dictionary<Key, string>();
		activeFunction = "draw";
		mAccess.colorManager.colorChanged += OnColorChanged;
	}
	public void AddSpriteLayer(string name = "", int scale = 10)
	{
		if (name == "")
		{
			name = "Sprite layer " + activeSpriteLayers.Count;
		}
		
		activeSprite = new Sprite2D();
		activeSprite.Scale = new Vector2(scale, scale);
		AddChild(activeSprite);
		activeSprite.Texture = ImageTexture.CreateFromImage(Image.Create(1, 1, false, Image.Format.Rgba8));
		activeSpriteLayers[name] = (activeSprite, new List<Vector2>());
		activeSpriteLayerCoords[name] = math.createVector(0);
		activeSpriteLayerColors[name] = mAccess.colorManager.getActiveColor();
		activeSpriteLayerOrder.Add(name);
		SetActiveSpriteLayer(name);
		spriteScale = scale;
		activeSprite.Centered = false;
	}
	public void SetActiveSpriteLayer(string name)
	{
		if (!activeSpriteLayers.ContainsKey(name))
		{
			return;
		}

		string oldActiveLayer = activeLayer;
		activeLayer = name;
		activeSprite = activeSpriteLayers[name].Item1;
		activeSpriteCoords = activeSpriteLayerCoords[name];
		spriteScale = activeSprite.Scale.X;
		UpdateSpriteLayerZIndexes();

		if (oldActiveLayer != null && oldActiveLayer != activeLayer)
		{
			RedrawSpriteLayer(oldActiveLayer, false);
		}

		RedrawSpriteLayer(activeLayer, true);
	}
	public int GetSpriteLayerOrder(string name)
	{
		return activeSpriteLayerOrder.IndexOf(name);
	}
	public void MoveSpriteLayer(string name, int order)
	{
		int oldOrder = GetSpriteLayerOrder(name);
		if (oldOrder < 0)
		{
			return;
		}

		order = Math.Clamp(order, 0, activeSpriteLayerOrder.Count - 1);
		if (oldOrder == order)
		{
			return;
		}

		activeSpriteLayerOrder.RemoveAt(oldOrder);
		activeSpriteLayerOrder.Insert(order, name);
		UpdateSpriteLayerZIndexes();
	}
	void UpdateSpriteLayerZIndexes()
	{
		int activeOrder = GetSpriteLayerOrder(activeLayer);
		for (int i = 0; i < activeSpriteLayerOrder.Count; i++)
		{
			string name = activeSpriteLayerOrder[i];
			activeSpriteLayers[name].Item1.ZIndex = i - activeOrder;
		}
	}
	void RedrawSpriteLayer(string name, bool selected)
	{
		if (!activeSpriteLayers.ContainsKey(name))
		{
			return;
		}

		Image image;
		List<Vector2> coords = activeSpriteLayers[name].Item2;
		if (coords.Count == 0)
		{
			image = Image.Create(1, 1, false, Image.Format.Rgba8);
		}
		else
		{
			image = mAccess.spriteManager.vectorsToImage(coords, "black");
			ApplySavedColor(image, activeSpriteLayerColors[name]);
		}

		if (!selected)
		{
			HalveImageAlpha(image);
		}

		activeSpriteLayers[name].Item1.Texture = ImageTexture.CreateFromImage(image);
		spriteChangeEvent?.Invoke(this, new SpriteEvent(activeSpriteLayers[name].Item1.Texture, name, GetSpriteLayerOrder(name)));
	}
	void HalveImageAlpha(Image image)
	{
		for (int x = 0; x < image.GetWidth(); x++)
		{
			for (int y = 0; y < image.GetHeight(); y++)
			{
				Color color = image.GetPixel(x, y);
				color.A *= InactiveLayerAlphaMultiplier;
				image.SetPixel(x, y, color);
			}
		}
	}
	void ApplySavedColor(Image image, Color savedColor)
	{
		for (int x = 0; x < image.GetWidth(); x++)
		{
			for (int y = 0; y < image.GetHeight(); y++)
			{
				Color color = image.GetPixel(x, y);
				if (color.A > 0)
				{
					Color recolored = savedColor;
					recolored.A *= color.A;
					image.SetPixel(x, y, recolored);
				}
			}
		}
	}
	void OnColorChanged(object sender, ColorChangedEvent e)
	{
		if (e.name == mAccess.colorManager.activeColorName && activeLayer != null)
		{
			activeSpriteLayerColors[activeLayer] = e.color;
			RedrawSpriteLayer(activeLayer, true);
		}
	}
	public void hover(Vector2 coord)
	{
		if (activeSprite == null || activeLayer == null)
		{
			return;
		}

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
					activeSpriteLayerCoords[activeLayer] = activeSpriteCoords;
				}
				break;
		}
	}
	public void click(Vector2 coord)
	{
		if (activeSprite == null || activeLayer == null)
		{
			return;
		}

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
					activeSpriteLayerCoords[activeLayer] = activeSpriteCoords;
				}
				activeSpriteLayers[activeLayer].Item2.Add(dif);
				RedrawSpriteLayer(activeLayer, true);
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
	public int order {get;}
	public SpriteEvent(Texture2D spriteArg, string nameArg, int orderArg = -1)
	{
		sprite = spriteArg;
		name = nameArg;
		order = orderArg;
	}
}
