using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace coolbeats.scripts.staticScriptsAndDataStructures
{
    public static class LUT
    {
        internal static double[] _sin = new double[360];
        internal static double[] _cos = new double[360];
        static LUT()
        {
            for (int i = 0; i < 360; i ++)
            {
                _sin[i] = Math.Sin(Math.PI*i/180);
                _cos[i] = Math.Cos(Math.PI*i/180);
            }
        }
        public static double sin(int x)
        {
            try
            {
                
            return _sin[x % 360];
            } catch
            {
                GD.Print("error", x);
                return 1;
            }
        }
        public static double cos(int x)
        {
            try
            {
                return _cos[x % 360];
            } catch
            {
                GD.Print("wrong:", x);
                return 1;
            }
        }
    }
}