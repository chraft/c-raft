#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion

using Chraft.Utilities.Math;

namespace Chraft.Utilities.Coords
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

        public override string ToString()
        {
            return string.Format("X:{0}, Y:{1}, Z:{2}", X, Y, Z);
        }
    }
}
