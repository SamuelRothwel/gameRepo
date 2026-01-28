using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coolbeats.scripts.staticScriptsAndDataStructures
{
    public static class MortenEncoding
    {
        public static uint encode(uint x, uint y)
        {
            uint result = 0;
            for (int i = 0; i < 32; i++)
            {
                if (((x >> i) & 1) == 1)
                {
                    result |= (uint)(1 << (2 * i));
                }
                if (((y >> i) & 1) == 1)
                {
                    result |= (uint)(1 << (2 * i + 1));
                }
            }
            return result;
        }
    }
}