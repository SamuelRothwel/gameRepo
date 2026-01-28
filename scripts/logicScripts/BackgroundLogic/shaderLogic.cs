using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;
using Godot;

namespace coolbeats.scripts.logicScripts.BackgroundLogic
{
    public partial class shaderLogic : BackgroundLogicNode
    {
        public override void setup()
        {
            creationFlags = new string[] { "pixelHitbox" };
        }
        public override void preProcess(Node node)
        {
            /*if (node is damageableSprite sprite)
            {
                int height = sprite.Texture.GetHeight();
                int width = sprite.Texture.GetWidth();
                sprite.healthMap = new double[width, height, 2];
                for (int i = 0; i < width; i++)
                {
                    string output = "";
                    for (int j = 0; j < height; j++)
                    {
                        sprite.healthMap[i, j, 1] = 1;
                        if (sprite.Texture._IsPixelOpaque(i, j))
                        {
                            sprite.healthMap[i, j, 0] = 0;
                            output += "0";
                        }
                        else
                        {
                            sprite.healthMap[i, j, 0] = 1;
                            output += "1";
                        }
                    }
                }
            }
            else
            {
                GD.Print(node.Name + " is not a damageable sprite");
            }*/
        }
    }
}