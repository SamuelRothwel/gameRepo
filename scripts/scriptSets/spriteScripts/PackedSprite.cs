using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;


public partial class PackedSprites : Node
{
    //coordinate, shape, image
    public ((int, int)[], int, int)[][] packedSprites;
    public virtual void setup() {  }
}
