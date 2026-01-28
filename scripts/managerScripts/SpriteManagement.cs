using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using Godot;
using System;
using Microsoft.EntityFrameworkCore.Storage;
using System.Globalization;
using System.Data;
using System.Runtime.CompilerServices;


public partial class SpriteManagement : managerNode
{
    [Export] public PackedScene packedSpriteMaps;
    public Dictionary<int, Color> colors;
    public Dictionary<string, Dictionary<string, int[][,]>> spriteMaps;
    public override void setup()
    {
        colors = new Dictionary<int, Color>();
        spriteMaps = new Dictionary<string, Dictionary<string, int[][,]>>();
        colors[0] = new Color(0, 0, 0, 0);
        colors[1] = new Color(0, 0, 0);
        colors[2] = new Color(0.5f, 0.5f, 0.5f);
        decodeSprites();
    }
    public Dictionary<string, Image[]>  getEntityImages(string entityName)
    {
        return spriteMaps[entityName].ToDictionary(x => x.Key, x => x.Value.Select(y => mapToImage(y)).ToArray());
    }
    public void decodeSprites()
    {
        Node animationScene = packedSpriteMaps.Instantiate();
		foreach (Node node in animationScene.GetChildren())
		{		
            foreach (PackedSprites sprites in node.GetChildren())
            {
                sprites.setup();
                spriteMaps[node.Name] =  new Dictionary<string, int[][,]>();
                decodeAnimationSprites(sprites.packedSprites, node.Name, sprites.Name);
            }
        }
    }
    public void decodeAnimationSprites(((int, int)[], int, int)[][] images, string objectName, string animationName)
    {
        spriteMaps[objectName][animationName] = images.Select(x => drawSpriteMap(x)).ToArray();
    }

    public List<(int, int)> getShapeCoordinates((int, int)[] coordinates)
    {
        List<(int, int)> outline = getShapeOutline(coordinates);
        Dictionary<int, List<int>> intercepts = new Dictionary<int, List<int>>();
        foreach ((int, int) coord in outline)
        {
            if (!intercepts.ContainsKey(coord.Item1))
            {
                intercepts[coord.Item1] = new List<int>();
            } 
            if (!intercepts[coord.Item1].Contains(coord.Item2))
            {
                intercepts[coord.Item1].Add(coord.Item2);
            }
        }
        foreach (int height in intercepts.Keys)
        {
            //GD.Print(height);
            string x = "";
            foreach (int i in intercepts[height])
            {
                x += i;
            }
            //GD.Print(x);
            
            bool onF = true;
            List<int> points = intercepts[height];
            points.Sort();
            for (int i = 0; i < points.Count() - 1; i++)
            {
                if (onF)
                {
                    for (int j = points[i] + 1; j < points[i + 1]; j++)
                    {
                        outline.Add((height, j));
                    }
                }
                onF = !onF;
            }
        }
        return outline;
    }

    public List<(int, int)> getShapeOutline((int, int)[] coordinates)
    {
        List<(int, int)> outCoords = new List<(int, int)>();
        for (int i = 0; i < coordinates.Count(); i++)
        {
            List<(int,int)> line = getLine(coordinates[i], coordinates[(i + 1) % coordinates.Count()]);
            foreach ((int, int) j in line)
            {
                outCoords.Add(j);
            }
        }
        return outCoords;
    }
    public List<(int, int)> getLine((int, int) p1, (int, int) p2) => getLine(p1.Item1, p1.Item2, p2.Item1, p2.Item2);
    public List<(int, int)> getLine(int x1, int y1, int x2, int y2)
    {
        List<(int, int)> lineOut = new List<(int, int)>();
        int x = Math.Abs(x1 - x2);
        int y = Math.Abs(y1 - y2);
        int xFlip = x1 > x2?  -1: 1;
        int yFlip = y1 > y2?  -1: 1;
        double slant = x == 0? int.MaxValue : (double)y / x;
        for (int i = 0; i < x + 1; i++)
        {
            lineOut.Add((x1 + i*xFlip, y1 + (int)Math.Round(i*yFlip * slant)));
        }
        return lineOut;
    }
    public Image mapToImage(int[,] map)
    {
        int height = map.GetLength(0);
        int width = map.GetLength(1);
        for (int i = 0; i < height; i++)
        {
            string e = "";
            for (int j = 0; j < width; j++)
            {
                e = e + map[i, j].ToString();
            }
        }
        Image image = Image.Create(width, height, false, Image.Format.Rgba8);
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (map[i, j] != 0)
                {
                    image.SetPixel(j, i, colors[map[i,j]]);
                }
            }
        }
        return image;
    }
    public int[,] drawSpriteMap(((int, int)[], int, int)[] shapes)
    {
        Dictionary<(int, int), (int, int)> colorMap = new Dictionary<(int, int), (int, int)>();

        for (int i = 0; i < shapes.Length; i++)
        {
            List<(int, int)> coords = getShapeCoordinates(shapes[i].Item1);
            foreach ((int, int) coord in coords)
            {
                (int, int) mapPoint = colorMap.TryGetValue((coord.Item1, coord.Item2), out mapPoint)? mapPoint : (-1, -1);
                if (mapPoint.Item1 < shapes[i].Item3)
                {
                    mapPoint.Item2 = shapes[i].Item2;
                    mapPoint.Item1 = shapes[i].Item3;
                    colorMap[(coord.Item1, coord.Item2)] = mapPoint;
                }
            }
        }
        int height = colorMap.Keys.Max(x => x.Item1);
        int width = colorMap.Keys.Max(x => x.Item2);
        int[,] spriteMap = new int[height + 1,width + 1];
        foreach ((int, int) k in colorMap.Keys)
        {
            spriteMap[k.Item1, k.Item2] = colorMap[k].Item2;
        }
        return spriteMap;
    }
    public void drawSquare(Color[][] image, int x, int y, int width, int height, Color color)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                image[x + i][y + j] = color;
            }
        }
    }
    
    public void drawTriangle(Color[][] image, int x, int y, int width, int height, bool right, Color color)
    {
        double rate = height / width;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                image[x + i][y + j] = color;
            }
        }
    }

    public class Entity
    {
        public Dictionary<string, Image> Images = new Dictionary<string, Image>();
    }
}