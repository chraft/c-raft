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

namespace Chraft.Utilities.Math
{
    public class MathHelper
    {
        public MathHelper()
        {
        }

        public static float Sin(float f)
        {
            return SIN_TABLE[(int) (f*10430.38F) & 0xffff];
        }

        public static float cos(float f)
        {
            return SIN_TABLE[(int) (f*10430.38F + 16384F) & 0xffff];
        }

        public static float sqrt_float(float f)
        {
            return (float) System.Math.Sqrt(f);
        }

        public static float sqrt_double(double d)
        {
            
            return (float) System.Math.Sqrt(d);
        }

        [System.Obsolete]
        public static int floor_float(float f)
        {
            int i = (int) f;
            return f >= (float) i ? i : i - 1;
        }

        [System.Obsolete]
        public static int floor_double(double d)
        {
            int i = (int) d;
            return d >= (double) i ? i : i - 1;
        }
  
        [System.Obsolete]      
        public static float abs(float f)
        {
            return f < 0.0F ? -f : f;
        }

        public static double abs_max(double d, double d1)
        {
            if (d < 0.0D)
            {
                d = -d;
            }
            if (d1 < 0.0D)
            {
                d1 = -d1;
            }
            return d <= d1 ? d1 : d;
        }

        private static float[] SIN_TABLE;

        static MathHelper()
        {
            SIN_TABLE = new float[0x10000];
            for (int i = 0; i < 0x10000; i++)
            {
                SIN_TABLE[i] = (float) System.Math.Sin(((double) i*3.1415926535897931D*2D)/65536D);
            }
        }
    }
}