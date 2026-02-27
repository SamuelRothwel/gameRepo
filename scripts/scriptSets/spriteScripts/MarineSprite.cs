using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class MarineSprite : PackedSprites
{
    public override void setup()
    {
        packedSprites = 
        new ( (int, int)[], int, int )[][] 
        {
            new ( (int, int)[], int, int )[]
            {
                ( 
                    new[] { (0, 20), (20, 0), (40, 20), (20, 40) }, 
                    1, 
                    0 
                ),
            },
        };
    }
}
