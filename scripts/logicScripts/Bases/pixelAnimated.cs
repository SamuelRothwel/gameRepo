using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Resolvers;
using Godot;

public partial class pixelAnimated : Sprite2D
{
    public string spriteSet;
    Dictionary<string, Image[]> animationImages;
    Dictionary<string, ImageTexture[]> animationTextures;
    Dictionary<string, int> spriteUpdateNumber;
    public List<int[][]> spriteUpdateList = new List<int[][]>();
    public string animationName;
    public int spriteNumber;
    public void setup(string name = null)
    {
        animationImages = mAccess.spriteManager.getEntityImages(name?? spriteSet);
        animationTextures = animationImages.ToDictionary(x => x.Key, x => x.Value.Select(y => ImageTexture.CreateFromImage(y)).ToArray());
        spriteUpdateNumber = animationImages.Select(x => x.Value.Select((y, i) => x.Key + (i))).SelectMany(x => x).ToDictionary(x => x, x => 0);
        setTexture("idle", 0);
    }
    public void pixelUpdates(ImageTexture texture, Image image, int[][] updates)
    {
        for (int i = 0; i < updates.Length; i++)
        {
            Color c = new Color(255, 255, 255, 0);
            image.SetPixel(updates[i][0], updates[i][1], c);
        }
        texture.Update(image);
    }
    public void setTexture(string name, int number)
    {
        spriteNumber = number;
        animationName = name;
        if (spriteUpdateNumber[name + number] < spriteUpdateList.Count())
        {
            for (int i = spriteUpdateNumber[name + number]; i < spriteUpdateList.Count(); i++)
            {
                pixelUpdates(animationTextures[name][number], animationImages[name][number], spriteUpdateList[i]);
            }
        }
        Texture = animationTextures[name][number];
    }
}