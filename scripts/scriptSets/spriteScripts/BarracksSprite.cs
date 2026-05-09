
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class BarracksSprite : PackedSprites
{
    public override void setup()
    {
        packedSprites = 
        new ( (int, int)[], string, int )[][] 
        {
            new ( (int, int)[], string, int )[]
            {
                ( 
                    new[] { (0, 100), (100, 100), (100, 0), (0, 0) }, 
                    "black", 
                    0 
                ),
            },
        };
    }
}