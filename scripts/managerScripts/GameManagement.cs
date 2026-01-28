using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;


public partial class GameManagement : managerNode
{
    public override void setup()
    {
        Engine.MaxFps = 100;
    }
}