/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;
using Godot;

namespace coolbeats.scripts.logicScripts.BackgroundLogic
{
    public partial class fetchSprite : BackgroundLogicNode
    {
        public override void setup()
        {
            creationFlags = new string[] { "Sprite2D" };
        }
        public override void creationFlag(Node node)
        {
            if (node is Sprite2D sprite)
            {
                //mAccess.animationManagement.setup(node.Name);
            } 
			else
            {
                GD.Print(node.Name + " is not a Sprite2D");
            }
        }

    }
}*/