using System.Linq;
using System.Collections.Generic;
using Godot;
using System;
using System.Data;
using System.Xml.Linq;
using coolbeats.scripts.logicScripts.Bases;


public partial class SpriteManagement : managerNode
{
    [Export] public PackedScene packedSpriteMaps;
    public Dictionary<int, Color> colors;
    public Dictionary<string, Dictionary<string, int[][,]>> spriteMaps;
    public Dictionary<string, PackedScene[]> spriteHitBoxes;
    public Dictionary<int, (int, int)> directionLookup;
    public Dictionary<int, int> directionOrder;
    public override void setup()
    {
        colors = new Dictionary<int, Color>();
        spriteMaps = new Dictionary<string, Dictionary<string, int[][,]>>();
        colors[0] = new Color(0, 0, 0, 0);
        colors[1] = new Color(0, 0, 0);
        colors[2] = new Color(0.5f, 0.5f, 0.5f);

        directionLookup = new Dictionary<int, (int, int)>
        {
            { -1, (0, 0) },
            { 0, (1, 0) },
            { 1, (1, 1) },
            { 2, (0, 1) },
            { 3, (-1, 1) },
            { 4, (-1, 0) },
            { 5, (-1, -1) },
            { 6, (0, -1) },
            { 7, (1, -1) }
        };;
        directionOrder = new Dictionary<int, int>
        {
            { -1, -1 },
            { 0, -1 },
            { 1, 0 },
            { 4, 1 },
            { 3, 2 },
            { 5, 3 },
            { 2, 4 },
            { 8, 5 },
            { 6, 6 },
            { 7, 7 }
        };
        
        decodeSprites();
        spriteHitBoxes = new Dictionary<string, PackedScene[]>();
        foreach (string map in spriteMaps.Keys)
        {
            createHitBoxes(map);
        }
    }
    public Area2D[] getHitBoxes(string name)
    {
        return spriteHitBoxes[name].Select(x => (Area2D)x.Instantiate()).ToArray();
    }
    public void createHitBoxes(string name)
    {
            GD.Print("sprite: " + name);
        int[,] spriteEdges = groupPattenAlgo(spriteMaps[name].First().Value[0].Select(x => x != 0));
        List<List<(int, int)>> coordinates = new List<List<(int, int)>>();
        for (int i = 0; i < spriteEdges.GetLength(0); i++)
        {
            for (int j = 0; j < spriteEdges.GetLength(1); j++)
            {
                if (spriteEdges[i,j] != 0)
                {
                    List<(int, int)> currentShape = new List<(int, int)>();
                    int x = i;
                    int y = j;
                    int direction;
                    while (true)
                    {
                        direction = directionOrder[spriteEdges[x,y]];
                        spriteEdges[x, y] = 0;
                        currentShape.Add((x, y));
                        int xt = x;
                        int yt = y;
                        for (int k = 1; k < 5; k++)
                        {
                            (int, int) xy = directionLookup[(direction + k) % 8];
                            if (spriteEdges[x + xy.Item1, y + xy.Item2] != 0)
                            {
                                x += xy.Item1;
                                y += xy.Item2;
                                break;
                            }
                        }
                        if (xt == x && yt == y)
                        {
                            break;
                        }
                    }
                    coordinates.Add(currentShape);
                }
            }
        }
        int setNumber = coordinates.Count();
        Area2D hitBox = new Area2D();
        hitBox.Position = new Vector2(-0.5f* spriteEdges.GetLength(0)+1, -0.5f*spriteEdges.GetLength(1)+1);
        for (int i = 0; i < setNumber; i++)
        {
            List<(int, int)> set = coordinates[i];
            int setSize = set.Count();
            List<int> solidified = new List<int>();
            solidified.Add(0);
            DVH(ref set, 0, setSize/2, 1, ref solidified);
            solidified.Add(setSize/2);
            DVH(ref set, setSize/2, setSize - 1, 1, ref solidified);
            CollisionShape2D collisionShape = new CollisionShape2D();
            GD.Print("e");
            GD.Print(string.Join(" ", solidified));
            GD.Print(string.Join(" ", solidified.Select(x => set[x])));
            ConcavePolygonShape2D polygon = new ConcavePolygonShape2D();
            List<Vector2> coords = new List<Vector2>();
            Vector2 start = math.createVector(set[solidified[0]]);
            coords.Add(start);
            for (int j = 1; j < solidified.Count(); j++)
            {
                Vector2 cur = math.createVector(set[solidified[j]]);
                coords.Add(cur);
                coords.Add(cur);
            }
            coords.Add(start);
            polygon.Segments = coords.ToArray();
            collisionShape.Shape = polygon;
            hitBox.AddChild(collisionShape);
            collisionShape.Owner = hitBox;
        }
        spriteHitBoxes[name] = new PackedScene[] { new PackedScene() };
        spriteHitBoxes[name][0].Pack(hitBox);
    }
    public void DVH(ref List<(int, int)> points, int start, int end, float distance, ref List<int> solidified)
    {
        (int, int) p1 = points[start];
        (int, int) p2 = points[end];
        GD.Print(p1, p2, start, ",", end,",",distance);
        int point = -1;
        float maxDistance = distance;
        for (int i = start; i < end; i++)
        {
            (int, int) p3 = points[i];
            float curDist = (float) (Math.Abs(
            (p2.Item1 - p1.Item1) * (p1.Item2 - p3.Item2) - (p2.Item2 - p1.Item2) * (p1.Item1 - p3.Item1))
            / Math.Sqrt(Math.Pow(p2.Item1 - p1.Item1, 2) + Math.Pow(p2.Item2 -p1.Item2, 2)));
            if (curDist > maxDistance)
            {
                maxDistance = curDist;
                point = i;
            } 
        }
        if (point != -1)
        {
            DVH(ref points, start, point, distance, ref solidified);
            solidified.Add(point);
            DVH(ref points, point, end, distance, ref solidified);
        }
    }
    public int[,] groupPattenAlgo(bool[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        bool[,] mapW = new bool[width, height - 1];
        bool[,] mapH = new bool[height, width - 1];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height - 1; j++)
            {
                mapW[i, j] = map[i, j] && map[i, j + 1];
            }
        }
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width - 1; j++)
            {
                mapH[i, j] = map[j, i] && map[j + 1, i];
            }
        }
        int[,] output = new int[width + 3, height + 3];
        for (int i = 0; i < mapW.GetLength(1); i++)
        {
            bool preF = false;
            for (int j = 0; j < mapW.GetLength(0); j++)
            {
                bool cur = mapW[j, i];
                output[j + 1, i + 2] = (Convert.ToInt16(cur) * 1 + Convert.ToInt16(preF) * 2) % 3;
                preF = cur;
            }
            output[width + 1, i + 2] = preF? 2 : 0;
        }
        for (int i = 0; i < mapH.GetLength(1); i++)
        {
            bool preF = false;
            for (int j = 0; j < mapH.GetLength(0); j++)
            {
                bool cur = mapH[j, i];
                output[i + 2, j + 1] += (Convert.ToInt16(cur) * 3 + Convert.ToInt16(preF) * 6) % 9;
                preF = cur;
            }
            output[i + 2, height + 1] = preF? 6 : 0;
        }
        for (int i = 0; i < output.GetLength(0); i++)
        {
            string outs = "";
            for (int j = 0; j < output.GetLength(1); j++)
            {
                outs += output[i, j];    
            }
            GD.Print(outs);
        }
        return output;
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
            string x = "";
            foreach (int i in intercepts[height])
            {
                x += i;
            }
            
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