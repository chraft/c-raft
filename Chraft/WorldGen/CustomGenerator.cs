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
using Chraft.World;
using System;
using System.Diagnostics;
using Chraft.World.Blocks;

namespace Chraft.WorldGen
{
    public class CustomChunkGenerator : IChunkGenerator
    {
        private bool GenInit = false;

        private PerlinNoise _Gen1;
        private PerlinNoise _Gen2;
        private PerlinNoise _Gen3;
        private PerlinNoise _Gen4;
        private PerlinNoise _Gen5;
        private PerlinNoise _Gen6;
        private FastRandom _FastRandom;

        private long _Seed;
        private WorldManager _World;

        public CustomChunkGenerator(WorldManager world, long seed)
        {
            _Seed = seed;
            _World = world;
        }

        public enum BIOME_TYPE
        {
            MOUNTAINS, SNOW, DESERT, PLAINS
        }

        private void InitGen()
        {
            if (GenInit)
                return;

            GenInit = true;

            _Gen1 = new PerlinNoise(_Seed);
            _Gen2 = new PerlinNoise(_Seed + 1);
            _Gen3 = new PerlinNoise(_Seed + 2);
            _Gen4 = new PerlinNoise(_Seed + 3);
            _Gen5 = new PerlinNoise(_Seed + 4);
            _Gen6 = new PerlinNoise(_Seed + 5);
            _FastRandom = new FastRandom(_Seed);
        }

        public void ProvideChunk(int x, int z, Chunk chunk, bool recalculate)
        {
            
            InitGen();

            byte[] data = new byte[32768];
            Stopwatch watch = new Stopwatch();
            watch.Start();
            GenerateTerrain(chunk, data, x, z);
            GenerateFlora(chunk, data, x, z);
            chunk.SetAllBlocks(data);
            watch.Stop();
            if(recalculate)
                chunk.Recalculate();
           

            Console.WriteLine("Chunk {0} {1}, {2}", x, z, watch.ElapsedMilliseconds);

            
            //chunk.Save();
            _World.AddChunk(chunk);
            chunk.MarkToSave();
        }

        private void GenerateTerrain(Chunk c, byte[] data, int x, int z)
        {
            double[, ,] density = new double[17, 129, 17];

            // Build the density map with lower resolution, 4*4*16 instead of 16*16*128
            for (int bx = 0; bx <= 16; bx += 4)
            {
                int worldX = bx + (x * 16);
                for (int bz = 0; bz <= 16; bz += 4)
                {
                    BIOME_TYPE type = CalcBiomeType(x, z);
                    int worldZ = bz + (z * 16);
                    for (int by = 0; by <= 128; by += 8)
                    {
                        density[bx, by, bz] = CalcDensity(worldX, by, worldZ, type);
                    }
                }
            }

            triLerpDensityMap(density);

            for (int bx = 0; bx < 16; bx++)
            {
                int worldX = bx + (x * 16);
                for (int bz = 0; bz < 16; bz++)
                {
                    int worldZ = bz + (z * 16);
                    int firstBlockHeight = -1;

                    BIOME_TYPE type = CalcBiomeType(worldX, worldZ);
                    for (int by = 127; by >= 0; --by)
                    {
                        int index = bx << 11 | bz << 7 | by;
                        if (by == 0) // First bedrock Layer
                        {
                            data[index] = (byte)BlockData.Blocks.Bedrock;
                            continue;
                        }
                        else if (by > 0 && by < 5 && _FastRandom.randomDouble() > 0.3) // Randomly put blocks of the remaining 4 layers of bedrock
                        {
                            data[index] = (byte)BlockData.Blocks.Bedrock;
                            continue;
                        }
                        else if (by <= 55)
                            data[index] = (byte)BlockData.Blocks.Stone;
                        else
                        {
                            if (by > 55 && by < 64)
                            {
                                data[index] = (byte)BlockData.Blocks.Still_Water;
                                if (by == 63 && type == BIOME_TYPE.SNOW)
                                {
                                    data[index] = (byte)BlockData.Blocks.Ice;
                                }
                            }

                            double dens = density[bx, by, bz];

                            if (dens >= 0.009 && dens <= 0.02)
                            {
                                // Some block was set...
                                if (firstBlockHeight == -1)
                                    firstBlockHeight = by;

                                GenerateOuterLayer(bx, by, bz, firstBlockHeight, type, c, data);
                            }
                            else if (dens > 0.02)
                            {
                                // Some block was set...
                                if (firstBlockHeight == -1)
                                    firstBlockHeight = by;

                                if (CalcCaveDensity(worldX, by, worldZ) > -0.6)
                                    GenerateInnerLayer(bx, by, bz, type, data);
                            }
                            else
                                firstBlockHeight = -1;
                        }

                        if (data[index] == (byte)BlockData.Blocks.Stone)
                            GenerateResource(bx, by, bz, data);
                    }
                }
            }
        }

        private void GenerateResource(int x, int y, int z, byte[] data)
        {
            // TODO: Find formula similar to original one
        }

        private void GenerateFlora(Chunk c, byte[] data, int x, int z)
        {
            BIOME_TYPE biome = CalcBiomeType(x, z); 
            for (int bx = 0; bx < 16; ++bx)
            {
                int worldX = bx + x * 16;
                for (int bz = 0; bz < 16; ++bz)
                {
                    int worldZ = bz + z * 16;
                    for (int by = 64; by < 128; ++by)
                    {
                        int worldY = by;
                        int index = bx << 11 | bz << 7 | by + 1;

                        if (data[bx << 11 | bz << 7 | by] == (byte)BlockData.Blocks.Grass && data[index] == (byte)BlockData.Blocks.Air)
                        {
                            double grassDens = CalcGrassDensity(worldX, worldZ);
                            if (grassDens > 0.0)
                            {
                                // Generate high grass.
                                double rand = _FastRandom.standNormalDistrDouble();
                                if (rand > -0.2 && rand < 0.2)
                                {
                                    data[index] = (byte)BlockData.Blocks.TallGrass;
                                    c.Data.setNibble(bx, by + 1, bz, 1);
                                }
                                
                        
                                //Generate flowers.
                                if (_FastRandom.standNormalDistrDouble() < -2)
                                {
                                    if (_FastRandom.randomBoolean())
                                        data[index] = (byte)BlockData.Blocks.Rose;
                                    else
                                        data[index] = (byte)BlockData.Blocks.Yellow_Flower;
                                }
                            }

                            if(by < 110 && bx % 4 == 0 && bz % 4 == 0)
                            {
                                double forestDens = CalcForestDensity(worldX, worldZ);

                                if (forestDens > 0.005)
                                {
                                    int randX = bx + _FastRandom.randomInt() % 12 + 4;
                                    int randZ = bz + _FastRandom.randomInt() % 12 + 4;

                                    if (randX < 3)
                                       randX = 3;
                                    else if (randX > 12)
                                       randX = 12;

                                    if (randZ < 3)
                                        randZ = 3;
                                    else if (randZ > 15)
                                        randZ = 12;

                                    if (data[randX << 11 | randZ << 7 | by] == (byte)BlockData.Blocks.Grass)
                                    {
                                        GenerateTree(c, randX, by, randZ, data);
                                    }
                                    else if (biome == BIOME_TYPE.DESERT && data[randX << 11 | randZ << 7 | by] == (byte)BlockData.Blocks.Sand)
                                    {
                                        GenerateCactus(randX, by, randZ, data);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GenerateCactus(int x, int y, int z, byte[] data)
        {
            int height = (_FastRandom.randomInt() + 1) % 3;

            if (!CanSeeTheSky(x, y + 1, z, data))
                return;

            for (int by = height; by < y + height; ++y)
                data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Cactus;
        }

        private void GenerateTree(Chunk c, int x, int y, int z, byte[] data)
        {
            // Trees should only be placed in direct sunlight
            if (!CanSeeTheSky(x, y + 1, z, data))
                return;

            double r2 = _FastRandom.standNormalDistrDouble();
            /*if (r2 > -2 && r2 < -1)
            {*/
                // Standard tree

            for (int by = y + 4; by < y + 6; by++)
                for (int bx = x - 2; bx <= x + 2; bx++)
                    for (int bz = z - 2; bz <= z + 2; bz++)
                    {
                        data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Leaves;
                        c.Data.setNibble(bx, by, bz, 0);
                    }

            for (int bx = x - 1; bx <= x + 1; bx++)
                for (int bz = z - 1; bz <= z + 1; bz++)
                {
                    data[bx << 11 | bz << 7 | y + 6] = (byte)BlockData.Blocks.Leaves;
                    c.Data.setNibble(bx, y + 6, bz, 0);
                }

            for (int by = y + 1; by < y + 6; by++)
            {
                data[x << 11 | z << 7 | by] = (byte)BlockData.Blocks.Log;
                c.Data.setNibble(x, by, z, 0);
            }
            //}
            // TODO: other tree types
            /*else if (r2 > 1 && r2 < 2)
            {
                c.setBlock(x, y + 1, z, (byte)0x0);
                c.getParent().getObjectGenerator("firTree").generate(c.getBlockWorldPosX(x), y + 1, c.getBlockWorldPosZ(z), false);
            }
            else
            {
                c.setBlock(x, y + 1, z, (byte)0x0);
                c.getParent().getObjectGenerator("tree").generate(c.getBlockWorldPosX(x), y + 1, c.getBlockWorldPosZ(z), false);
            }*/
        }

        private bool CanSeeTheSky(int x, int y, int z, byte[] data)
        {
            int by;
            for (by = y; BlockHelper.Instance(data[x << 11 | z << 7 | by]).Opacity == 0 && by < 128; ++by);

            return by == 128;
        }

        private double CalcForestDensity(double x, double z)
        {
            double result = 0.0;
            result += _Gen1.fBm(0.03 * x, 0, 0.03 * z, 7, 2.3614521, 0.85431);
            return result;
        }

        private double CalcGrassDensity(double x, double z)
        {
            double result = 0.0;
            result += _Gen3.fBm(0.05 * x, 0, 0.05 * z, 4, 2.37152, 0.8571);
            return result;
        }

        private double CalcDensity(double x, double y, double z, BIOME_TYPE type)
        {
            double height = CalcBaseTerrain(x, z);
            double density = CalcMountainDensity(x, y, z);
            double divHeight = (y - 55) * 1.5;

            if (y > 100)
                divHeight *= 2.0;

            if (type == BIOME_TYPE.DESERT)
            {
                divHeight *= 2.5;
            }
            else if (type == BIOME_TYPE.PLAINS)
            {
                divHeight *= 1.6;
            }
            else if (type == BIOME_TYPE.MOUNTAINS)
            {
                divHeight *= 1.1;
            }
            else if (type == BIOME_TYPE.SNOW)
            {
                divHeight *= 1.2;
            }

            return (height + density) / divHeight;
        }

        private double CalcBaseTerrain(double x, double z)
        {
            double result = 0.0;
            result += _Gen2.fBm(0.0009 * x, 0, 0.0009 * z, 3, 2.2341, 0.94321) + 0.4;
            return result;
        }

        private double CalcMountainDensity(double x, double y, double z)
        {
            double result = 0.0;

            double x1, y1, z1;

            x1 = x * 0.0006;
            y1 = y * 0.0008;
            z1 = z * 0.0006;

            double[] freq = { 1.232, 8.4281, 16.371, 32, 64 };
            double[] amp = { 1.0, 1.4, 1.6, 1.8, 2.0 };

            double ampSum = 0.0;
            for (int i = 0; i < freq.Length; i++)
            {
                result += _Gen5.noise(x1 * freq[i], y1 * freq[i], z1 * freq[i]) * amp[i];
                ampSum += amp[i];
            }

            return result / ampSum;
        }

        private double CalcTemperature(double x, double z)
        {
            double result = 0.0;
            result += _Gen4.fBm(x * 0.0008, 0, 0.0008 * z, 7, 2.1836171, 0.7631);

            result = 32.0 + (result) * 64.0;

            return result;
        }

        private BIOME_TYPE CalcBiomeType(int x, int z)
        {
            double temp = CalcTemperature(x, z);

            if (temp >= 60)
            {
                return BIOME_TYPE.DESERT;
            }
            else if (temp >= 32)
            {
                return BIOME_TYPE.MOUNTAINS;
            }
            else if (temp < 8)
            {
                return BIOME_TYPE.SNOW;
            }

            return BIOME_TYPE.PLAINS;
        }

        private void GenerateOuterLayer(int x, int y, int z, int firstBlockHeight, BIOME_TYPE type, Chunk c, byte[] data)
        {
            double heightPercentage = (firstBlockHeight - y) / 128.0;
            short currentIndex = (short)(x << 11 | z << 7 | y);

            switch (type)
            {
                case BIOME_TYPE.PLAINS:
                case BIOME_TYPE.MOUNTAINS:
                    // Beach
                    if (y >= 60 && y <= 66)
                    {
                        data[currentIndex] = (byte)BlockData.Blocks.Sand;
                        break;
                    }

                    if (heightPercentage == 0.0 && y > 66)
                    {
                        // Grass on top
                        data[currentIndex] = (byte)BlockData.Blocks.Grass;
                    }
                    else if (heightPercentage > 0.2)
                    {
                        // Stone
                        data[currentIndex] = (byte)BlockData.Blocks.Stone;
                    }
                    else
                    {
                        // Dirt
                        data[currentIndex] = (byte)BlockData.Blocks.Dirt;
                    }

                    GenerateRiver(c, x, y, z, heightPercentage, type, data);
                    break;

                case BIOME_TYPE.SNOW:

                    if (heightPercentage == 0.0 && y > 65)
                    {
                        // Snow on top
                        data[currentIndex] = (byte)BlockData.Blocks.Snow;
                        // Grass under the snow
                        data[x << 11 | z << 7 | y - 1] = (byte)BlockData.Blocks.Grass;
                    }

                    else if (heightPercentage > 0.2)
                    {
                        // Stone
                        data[currentIndex] = (byte)BlockData.Blocks.Stone;
                    }
                    else if (data[x << 11 | z << 7 | (y + 1)] == (byte)BlockData.Blocks.Air)
                    {
                        // Grass under the snow
                        data[currentIndex] = (byte)BlockData.Blocks.Grass;
                        data[x << 11 | z << 7 | (y + 1)] = (byte)BlockData.Blocks.Snow;
                    }
                    else
                    {
                        // Dirt
                        data[currentIndex] = (byte)BlockData.Blocks.Dirt;
                    }

                    GenerateRiver(c, x, y, z, heightPercentage, type, data);
                    break;

                case BIOME_TYPE.DESERT:
                    /*if (heightPercentage > 0.6 && y < 75)
                    {
                        // Stone
                        data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Stone;
                    }
                    else*/
                    if (y < 80)
                    {
                        data[currentIndex] = (byte)BlockData.Blocks.Sand;
                    }

                    break;

            }
        }

        protected void GenerateRiver(Chunk c, int x, int y, int z, double heightPercentage, BIOME_TYPE type, byte[] data)
        {
            // Rivers under water? Nope.
            if (y <= 63)
                return;

            double lakeIntens = CalcLakeIntensity(x + c.Coords.ChunkX * 16, z + c.Coords.ChunkZ * 16);
            short currentIndex = (short)(x << 11 | z << 7 | y);

            if (lakeIntens < 0.2)
            {
                if(heightPercentage < 0.001)
                    data[currentIndex] = (byte)BlockData.Blocks.Air;
                else if(heightPercentage < 0.04)
                {
                    if (type == BIOME_TYPE.SNOW)
                    {
                        // To be sure that there's no snow above us
                        data[x << 11 | z << 7 | y + 1] = (byte)BlockData.Blocks.Air;
                        data[currentIndex] = (byte)BlockData.Blocks.Ice;
                    }
                    else
                    {
                        data[currentIndex] = (byte)BlockData.Blocks.Still_Water;
                    }
                }
            }
        }

        protected double CalcLakeIntensity(double x, double z)
        {
            double result = 0.0;
            result += _Gen3.fBm(x * 0.004, 0, 0.004 * z, 4, 2.1836171, 0.7631);
            return Math.Sqrt(Math.Abs(result));
        }

        protected double CalcCaveDensity(double x, double y, double z)
        {
            double result = 0.0;
            result += _Gen6.fBm(x * 0.04, y * 0.04, z * 0.04, 2, 2.0, 0.98);
            return result;
        }

        private void GenerateInnerLayer(int x, int y, int z, BIOME_TYPE type, byte[] data)
        {
            data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Stone;

        }

        private static double lerp(double x, double x1, double x2, double q00, double q01)
        {
            return ((x2 - x) / (x2 - x1)) * q00 + ((x - x1) / (x2 - x1)) * q01;
        }

        private static double triLerp(double x, double y, double z, double q000, double q001, double q010, double q011, double q100, double q101, double q110, double q111, double x1, double x2, double y1, double y2, double z1, double z2)
        {
            double x00 = lerp(x, x1, x2, q000, q100);
            double x10 = lerp(x, x1, x2, q010, q110);
            double x01 = lerp(x, x1, x2, q001, q101);
            double x11 = lerp(x, x1, x2, q011, q111);
            double r0 = lerp(y, y1, y2, x00, x01);
            double r1 = lerp(y, y1, y2, x10, x11);
            return lerp(z, z1, z2, r0, r1);
        }

        private void triLerpDensityMap(double[, ,] densityMap)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (!(x % 4 == 0 && y % 8 == 0 && z % 4 == 0))
                        {
                            int offsetX = (x / 4) * 4;
                            int offsetY = (y / 8) * 8;
                            int offsetZ = (z / 4) * 4;
                            densityMap[x, y, z] = triLerp(x, y, z, densityMap[offsetX, offsetY, offsetZ], densityMap[offsetX, offsetY + 8, offsetZ], densityMap[offsetX, offsetY, offsetZ + 4], densityMap[offsetX, offsetY + 8, offsetZ + 4], densityMap[4 + offsetX, offsetY, offsetZ], densityMap[4 + offsetX, offsetY + 8, offsetZ], densityMap[4 + offsetX, offsetY, offsetZ + 4], densityMap[4 + offsetX, offsetY + 8, offsetZ + 4], offsetX, 4 + offsetX, offsetY, 8 + offsetY, offsetZ, offsetZ + 4);
                        }
                    }
                }
            }
        }
    }
}