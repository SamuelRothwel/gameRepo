/*using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Schema;
using coolbeats.scripts.staticScriptsAndDataStructures;
using Godot;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;


public partial class deprecatedDamageableSprite : pixelAnimated
{
    public (int, int)[,] healthMap;
    CharacterBody2D collider;
    CollisionShape2D shape;

    public override void _Ready()
    {
        base.setup("TestEnemy");
        healthMap = mAccess.damageManager.GetHealthMap("TestEnemy"); 
        collider = GetChild<CharacterBody2D>(0);
        shape = collider.GetChild<CollisionShape2D>(0);
        collider.CollisionLayer = 6;
        RectangleShape2D rectangle = new RectangleShape2D();
        rectangle.Size = new Vector2(healthMap.GetLength(0) * 5, healthMap.GetLength(1) * 5);
        shape.Shape = rectangle;
    }

    public double yTilt(double x, double y, int theta)
    {
        return y * LUT.cos((int)theta) + x * LUT.sin((int)theta);
    } 
    public double xTilt(double x, double y, int theta)
    {
        return - x * LUT.cos((int)theta) + y * LUT.sin((int)theta);
    }
    
    public void damage((int, int) x) => damage(x.Item1, x.Item2);
    public void damage(int x, int y)
    {
        healthMap[x, y].Item1 = 0;
        spriteUpdateList.Add(new int[][] {new int[] {x, y}});
        setTexture(animationName, spriteNumber);
    }

  
    public void impact(projectile Projectile)
    {
        double x = Projectile.Position.X - this.Position.X+2.5*healthMap.GetLength(0); 
        double y = Projectile.Position.Y - this.Position.Y+2.5*healthMap.GetLength(1); 
        int angle = Convert.ToInt32(this.RotationDegrees - Projectile.RotationDegrees);
        angle += 450;
        double size =  5*Projectile.collider.Scale.X;
        double length = size; 
        double xMin;
        double xMax;
        xMin = xTilt(x, y, angle) - length; 
        xMax = xTilt(x, y, angle) + length;
        double yMin = yTilt(x, y, angle);
        double yMax = yTilt(x+Projectile.distance.X, y+Projectile.distance.Y, angle);
        double currentY = 100000;
        (int, int) smallest = (-1, -1);
        for (int i = 0; i < healthMap.GetLength(0); i++)
        {
            for (int j = 0; j < healthMap.GetLength(1); j++)
            {
                double ytilt = yTilt(5*i, 5*j, angle); 
                double xtilt = xTilt(5*i, 5*j, angle); 
                if (ytilt < currentY && ytilt < yMax && xtilt > xMin && xtilt < xMax && ytilt > yMin && healthMap[i, j].Item1 != 0)
                {
                    currentY = ytilt;
                    smallest = (i, j);
                }   
            }
        }
        if (smallest.Item1 != -1)
        {
            damage(smallest);
            setTexture(animationName, spriteNumber);
            Projectile.remove();
        }

        double max = x + Projectile.collider.Scale.X / LUT.cos((int)angle);
    }
}*/