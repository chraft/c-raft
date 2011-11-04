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

/**
 * Random number generator based on the Xorshift generator by George Marsaglia.
 *
 * @author Benjamin Glatzel <benjamin.glatzel@me.com>
 */
public class FastRandom {

    private long _seed = DateTime.Now.Millisecond;

    /**
     * Initializes a new instance of the random number generator using
     * a specified seed.
     *
     * @param seed The seed to use
     */
    public FastRandom(long seed) {
        this._seed = seed;
    }

    /**
     * Initializes a new instance of the random number generator using
     * System.currentTimeMillis() as seed.
     */
    public FastRandom() {
    }

    /**
     * Returns a random value as long.
     *
     * @return Random value
     */
    long randomLong() {
        _seed ^= (_seed << 21);
        _seed ^= ((int)(((uint)_seed) >> 35));
        _seed ^= (_seed << 4);
        return _seed;
    }

    /**
     * Returns a random value as integer.
     *
     * @return Random value
     */
    public int randomInt() {
        return (int) randomLong();
    }

    /**
     * Returns a random value as double.
     *
     * @return Random value
     */
    public double randomDouble() {
        return randomLong() / ((double) long.MaxValue - 1d);
    }

    /**
     * Returns a random value as boolean.
     *
     * @return Random value
     */
    public bool randomBoolean() {
        return randomLong() > 0;
    }

    /**
     * Returns a random character string with a specified length.
     *
     * @param length The length of the generated string
     * @return Random character string
     */
    public String randomCharacterString(int length) {
        StringBuilder s = new StringBuilder();

        for (int i = 0; i < length / 2; i++) {
            s.Append((char)('a' + Math.Abs(randomDouble()) * 26d));
            s.Append((char)('A' + Math.Abs(randomDouble()) * 26d));
        }

        return s.ToString();
    }

    /**
     * Calculates a standardized normal distributed value (using the polar method).
     *
     * @return
     */
    public double standNormalDistrDouble() {

        double q = Double.MaxValue;
        double u1 = 0;
        double u2;

        while (q >= 1d || q == 0) {
            u1 = randomDouble();
            u2 = randomDouble();

            q = Math.Pow(u1, 2) + Math.Pow(u2, 2);
        }

        double p = Math.Sqrt((-2d * (Math.Log(q))) / q);
        return u1 * p; // or u2 * p
    }

    /**
     * Some random noise.
     *
     * @param x
     * @param y
     * @param z
     * @param seed
     * @return
     */
    public static double randomNoise(double x, double y, double z, int seed) {
        int u = (int) x * 702395077 + (int) y * 915488749 + (int) z * 1299721 + seed * 1402024253;
        u = (u << 13) ^ u;
        return (1.0 - ((u * (u * u * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0);
    }
}
}
