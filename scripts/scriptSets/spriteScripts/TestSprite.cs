using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class TestSprite : PackedSprites
{
    public override void setup()
    {
        packedSprites = 
        new ( (int, int)[], string, int )[][] 
        {
            new ( (int, int)[], string, int )[]
            {
                ( 
                    new[] { (0, 0), (0, 30), (30, 30), (30, 0) }, 
                    "black", 
                    1 
                )
            },
        };
    }
}
