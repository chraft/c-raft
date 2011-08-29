using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Utils
{
    public static class MathExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private const double _180OverPi = 180.0 / Math.PI;
        private const double _PiOver180 = Math.PI / 180.0;

        public static double ToRadians(this double val)
        {
            return val * _PiOver180;
        }

        public static double ToDegrees(this double val)
        {
            return val * _180OverPi;
        }
    }
}
