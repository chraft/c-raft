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

/* Ported and modified by Stefano Bonicatti <smjert@gmail.com> */

using System;

namespace CustomGenerator
{

/**
 * Improved Perlin noise based on the reference implementation by Ken Perlin.
 *
 * @author Benjamin Glatzel <benjamin.glatzel@me.com>
 */
public class PerlinNoise {

    private int[] _noisePermutations, _noiseTable;

    /**
     * @param seed
     */
    public PerlinNoise(long seed) {
        FastRandom rand = new FastRandom(seed);
        _noisePermutations = new int[512];
        _noiseTable = new int[256];

        for (int i = 0; i < 256; i++)
            _noiseTable[i] = i;

        for (int i = 0; i < 256; i++) {
            int j = rand.randomInt() % 256;

            j = (j < 0) ? -j : j;

            int swap = _noiseTable[i];
            _noiseTable[i] = _noiseTable[j];
            _noiseTable[j] = swap;
        }

        for (int i = 0; i < 256; i++)
            _noisePermutations[i] = _noisePermutations[i + 256] = _noiseTable[i];
    }

    /**
     * @param x
     * @param y
     * @param z
     * @return
     */
    public double noise(double x, double y, double z) {
        int X = (int)Math.Floor(x) & 255, Y = (int)Math.Floor(y) & 255, Z = (int)Math.Floor(z) & 255;

        x -= Math.Floor(x);
        y -= Math.Floor(y);
        z -= Math.Floor(z);

        double u = fade(x), v = fade(y), w = fade(z);
        int A = _noisePermutations[X] + Y, AA = _noisePermutations[A] + Z, AB = _noisePermutations[(A + 1)] + Z,
                B = _noisePermutations[(X + 1)] + Y, BA = _noisePermutations[B] + Z, BB = _noisePermutations[(B + 1)] + Z;

        return lerp(w, lerp(v, lerp(u, grad(_noisePermutations[AA], x, y, z),
                grad(_noisePermutations[BA], x - 1, y, z)),
                lerp(u, grad(_noisePermutations[AB], x, y - 1, z),
                        grad(_noisePermutations[BB], x - 1, y - 1, z))),
                lerp(v, lerp(u, grad(_noisePermutations[(AA + 1)], x, y, z - 1),
                        grad(_noisePermutations[(BA + 1)], x - 1, y, z - 1)),
                        lerp(u, grad(_noisePermutations[(AB + 1)], x, y - 1, z - 1),
                                grad(_noisePermutations[(BB + 1)], x - 1, y - 1, z - 1))));
    }

    /**
     * @param t
     * @return
     */
    private static double fade(double t) {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    /**
     * @param t
     * @param a
     * @param b
     * @return
     */
    private static double lerp(double t, double a, double b) {
        return a + t * (b - a);
    }

    /**
     * @param hash
     * @param x
     * @param y
     * @param z
     * @return
     */
    private static double grad(int hash, double x, double y, double z) {
        int h = hash & 15;
        double u = h < 8 ? x : y, v = h < 4 ? y : h == 12 || h == 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    /**
     * @param x
     * @param y
     * @param z
     * @param octaves
     * @param lacunarity
     * @param gain
     * @param offset
     * @return
     */
    public double ridgedMultiFractalNoise(double x, double y, double z, int octaves, double lacunarity, double gain, double offset) {
        double frequency = 1f;
        double signal;

        /*
         * Fetch the first noise octave.
         */
        signal = ridge(noise(x, y, z), offset);
        double result = signal;
        double weight;

        for (int i = 1; i <= octaves; i++) {
            x *= lacunarity;
            y *= lacunarity;
            z *= lacunarity;

            weight = gain * signal;

            if (weight > 1.0f) {
                weight = 1.0f;
            } else if (weight < 0.0f) {
                weight = 0.0f;
            }

            signal = ridge(noise(x, y, z), offset);

            signal *= weight;
            result += signal * Math.Pow(frequency, -0.96461f);
            frequency *= lacunarity;
        }


        return result;
    }

    private double ridge(double n, double offset) {
        n = Math.Abs(n);
        n = offset - n;
        n = n * n;
        return n;
    }

    public double fBm(double x, double y, double z, int octaves, double lacunarity, double h)
    {
        double result = 0.0;

        for (int i = 0; i < octaves; i++)
        {
            result += noise(x, y, z) * Math.Pow(lacunarity, -h * i);

            x *= lacunarity;
            y *= lacunarity;
            z *= lacunarity;
        }

        return result;
    }
}
}
