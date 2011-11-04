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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft
{
    /*
 * Copyright 2011 Benjamin Glatzel <benjamin.glatzel@me.com>.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/**
 * @author Benjamin Glatzel <benjamin.glatzel@me.com>
 */
public class MathHelperNew {

    public static float biLerp(float x, float y, float q11, float q12, float q21, float q22, float x1, float x2, float y1, float y2) {
        float r1 = lerp(x, x1, x2, q11, q21);
        float r2 = lerp(x, x1, x2, q12, q22);
        return lerp(y, y1, y2, r1, r2);
    }

    private static float lerp(float x, float x1, float x2, float q00, float q01) {
        return ((x2 - x) / (x2 - x1)) * q00 + ((x - x1) / (x2 - x1)) * q01;
    }

    public static float triLerp(float x, float y, float z, float q000, float q001, float q010, float q011, float q100, float q101, float q110, float q111, float x1, float x2, float y1, float y2, float z1, float z2) {
        float x00 = lerp(x, x1, x2, q000, q100);
        float x10 = lerp(x, x1, x2, q010, q110);
        float x01 = lerp(x, x1, x2, q001, q101);
        float x11 = lerp(x, x1, x2, q011, q111);
        float r0 = lerp(y, y1, y2, x00, x01);
        float r1 = lerp(y, y1, y2, x10, x11);
        return lerp(z, z1, z2, r0, r1);
    }

    /**
     * Applies Cantor's pairing function to 2D coordinates.
     *
     * @param k1 X-coordinate
     * @param k2 Y-coordinate
     * @return Unique 1D value
     */
    public static int cantorize(int k1, int k2) {
        return ((k1 + k2) * (k1 + k2 + 1) / 2) + k2;
    }

    /**
     * Inverse function of Cantor's pairing function.
     *
     * @param c Cantor value
     * @return Value along the x-axis
     */
    public static int cantorX(int c) {
        int j = (int) (Math.Sqrt(0.25 + 2 * c) - 0.5);
        return j - cantorY(c);
    }

    /**
     * Inverse function of Cantor's pairing function.
     *
     * @param c Cantor value
     * @return Value along the y-axis
     */
    private static int cantorY(int c) {
        int j = (int) (Math.Sqrt(0.25 + 2 * c) - 0.5);
        return c - j * (j + 1) / 2;
    }
}

}
