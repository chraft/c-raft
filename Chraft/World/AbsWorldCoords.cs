using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utils;

namespace Chraft.World
{
    public struct AbsWorldCoords
    {
        /// <summary>
        /// The absolute world X coordinate (+ south / - north).
        /// </summary>
        public readonly double X;
        /// <summary>
        /// The absolute world Y coordinate (+ up).
        /// </summary>
        public readonly double Y;
        /// <summary>
        /// The absolute world Z coordinate (+ east / - west).
        /// </summary>
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

        public static bool operator ==(AbsWorldCoords coords1, AbsWorldCoords coords2)
        {
            return coords1.X == coords2.X &&
                    coords1.Y == coords2.Y &&
                    coords1.Z == coords2.Z;
        }

        public static bool operator !=(AbsWorldCoords coords1, AbsWorldCoords coords2)
        {
            return coords1.X != coords2.X ||
                    coords1.Y != coords2.Y ||
                    coords1.Z != coords2.Z;
        }
    }
}
