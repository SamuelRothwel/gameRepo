using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class GunBarrelSprite : PackedSprites
{
    public override void setup()
    {
        packedSprites = 
        new ( (int, int)[], int, int )[][] 
        {
            new ( (int, int)[], int, int )[]
            {
                ( 
                    new[] { (0, 0), (0, 5), (30, 5), (30, 0) }, 
                    1, 
                    0 
                ),
                ( 
                    new[] { (1, 1), (1, 4), (29, 4), (29, 1) }, 
                    2, 
                    1 
                ), 
            },
        };
    }
}
