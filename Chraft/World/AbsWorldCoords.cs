using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utils;

namespace Chraft.World
{
    public struct AbsWorldCoords
    {
        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        public AbsWorldCoords(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public AbsWorldCoords(UniversalCoords coords)
        {
            X = coords.WorldX;
            Y = coords.WorldY;
            Z = coords.WorldZ;
        }
        
        public AbsWorldCoords(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }
        
        public Vector3 ToVector()
        {
            return new Vector3(X, Y, Z);
        }
    }
}
