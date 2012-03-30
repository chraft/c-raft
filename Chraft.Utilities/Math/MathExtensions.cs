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

using System;

namespace Chraft.Utilities.Math
{
    public static class MathExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private const double _180OverPi = 180.0 / System.Math.PI;
        private const double _PiOver180 = System.Math.PI / 180.0;

        /// <summary>
        /// Converts a double from degrees to radians
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double ToRadians(this double val)
        {
            return val * _PiOver180;
        }

        /// <summary>
        /// Converts a double from radians to degrees
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double ToDegrees(this double val)
        {
            return val * _180OverPi;
        }

        private const double EqualityTolerence = Double.Epsilon;

        /// <summary>
        /// Compares two doubles for equality, returning true if the absolute difference is less than or equal to Double.Epsilon
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool DoubleIsEqual(this double val1, double val2)
        {
            return System.Math.Abs(val1 - val2) <= EqualityTolerence;
        }
    }
}
