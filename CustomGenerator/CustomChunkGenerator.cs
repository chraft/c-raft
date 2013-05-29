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

using System.Threading.Tasks;
using Chraft.PluginSystem;
using System;
using Chraft.PluginSystem.World;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;

namespace CustomGenerator
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
        private IWorldManager _World;
        Random r = new Random();

        private IBlockHelper _blockHelper;

        public enum BIOME_TYPE : byte
        {
            PLAINS = 1, DESERT, MOUNTAINS, SNOW = 12 
        }

        public void Init(IWorldManager world, long seed)
        {
            _Seed = seed;
            _World = world;
            _blockHelper = _World.GetServer().GetBlockHelper();
        }

        public void Init(long seed, IBlockHelper helper)
        {
            _blockHelper = helper;
            _Seed = seed;
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

        public IChunk GenerateChunk(IChunk chunk, int x, int z, bool external)
        {

            InitGen();
#if PROFILE
            Stopwatch watch = new Stopwatch();
            watch.Start();
#endif
            GenerateTerrain(chunk, x, z);
            GenerateFlora(chunk, x, z);

            if (!external)
            {
                chunk.RecalculateHeight();
                chunk.LightToRecalculate = true;
#if PROFILE
            watch.Stop();

            _World.Logger.Log(Logger.LogLevel.Info, "Chunk {0} {1}, {2}", false, x, z, watch.ElapsedMilliseconds);
#endif


                _World.AddChunk(chunk);
                chunk.MarkToSave();
            }

            return chunk;
        }

        private void GenerateTerrain(IChunk c, int x, int z)
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
                        //int index = bx << 11 | bz << 7 | by;
                        if (by == 0) // First bedrock Layer
                            c.SetType(bx, by, bz, BlockData.Blocks.Bedrock, false);

                        else if (by > 0 && by < 5 && _FastRandom.randomDouble() > 0.3) // Randomly put blocks of the remaining 4 layers of bedrock
                            c.SetType(bx, by, bz, BlockData.Blocks.Bedrock, false);

                        else if (by <= 55)
                            c.SetType(bx, by, bz, BlockData.Blocks.Stone, false);
                        else
                        {
                            if (by > 55 && by < 64)
                            {
                                c.SetType(bx, by, bz, BlockData.Blocks.Still_Water, false);
                                if (by == 63 && type == BIOME_TYPE.SNOW)
                                {
                                    c.SetBiomeColumn(bx, bz, (byte)BIOME_TYPE.SNOW);
                                    c.SetType(bx, by, bz, BlockData.Blocks.Ice, false);
                                }
                            }

                            double dens = density[bx, by, bz];

                            if (dens >= 0.009 && dens <= 0.02)
                            {
                                // Some block was set...
                                if (firstBlockHeight == -1)
                                    firstBlockHeight = by;

                                GenerateOuterLayer(bx, by, bz, firstBlockHeight, type, c);
                            }
                            else if (dens > 0.02)
                            {
                                // Some block was set...
                                if (firstBlockHeight == -1)
                                    firstBlockHeight = by;

                                if (CalcCaveDensity(worldX, by, worldZ) > -0.6)
                                    GenerateInnerLayer(bx, by, bz, type, c);
                            }
                            else
                                firstBlockHeight = -1;
                        }

                        if (c.GetType(bx, by, bz) == BlockData.Blocks.Stone)
                            GenerateResource(bx, by, bz, c);
                    }
                }
            }
        }

        private void GenerateResource(int x, int y, int z, IChunk c)
        {


            if (r.Next(100 * y) == 0)
                c.SetType(x, y, z, BlockData.Blocks.Diamond_Ore, false);
            else if (r.Next(100 * y) == 0)
                c.SetType(x, y, z, BlockData.Blocks.Lapis_Lazuli_Ore, false);
            else if (r.Next(40 * y) == 0)
                c.SetType(x, y, z, BlockData.Blocks.Gold_Ore, false);
            else if (r.Next(10 * y) == 0)
                c.SetType(x, y, z, BlockData.Blocks.Redstone_Ore_Glowing, false);
            else if (r.Next(4 * y) == 0)
                c.SetType(x, y, z, BlockData.Blocks.Iron_Ore, false);
            else if (r.Next(2 * y) == 0)
                c.SetType(x, y, z, BlockData.Blocks.Coal_Ore, false);
            
        }

        private void GenerateFlora(IChunk c, int x, int z)
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
                        //int index = bx << 11 | bz << 7 | by + 1;

                        if (c.GetType(bx, by, bz) == BlockData.Blocks.Grass && c.GetType(bx, by + 1, bz) == (byte)BlockData.Blocks.Air)
                        {
                            double grassDens = CalcGrassDensity(worldX, worldZ);
                            if (grassDens > 0.0)
                            {
                                // Generate high grass.
                                double rand = _FastRandom.standNormalDistrDouble();
                                if (rand > -0.2 && rand < 0.2)
                                {
                                    c.SetType(bx, by + 1, bz, BlockData.Blocks.TallGrass, false);
                                    c.SetData(bx, by + 1, bz, 1, false);
                                }


                                //Generate flowers.
                                if (_FastRandom.standNormalDistrDouble() < -2)
                                {
                                    if (_FastRandom.randomBoolean())
                                        c.SetType(bx, by + 1, bz, BlockData.Blocks.Rose, false);
                                    else
                                        c.SetType(bx, by + 1, bz, BlockData.Blocks.Yellow_Flower, false);
                                }
                            }

                            if (by < 110 && bx % 4 == 0 && bz % 4 == 0)
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

                                    if (c.GetType(randX, by, randZ) == BlockData.Blocks.Grass)
                                        GenerateTree(c, randX, by, randZ);
                                    
                                    else if (biome == BIOME_TYPE.DESERT && c.GetType(randX, by, randZ) == BlockData.Blocks.Sand)
                                        GenerateCactus(c, randX, by, randZ);
                                    
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GenerateCactus(IChunk c, int x, int y, int z)
        {
            int height = (_FastRandom.randomInt() + 1) % 3;

            if (!CanSeeTheSky(x, y + 1, z, c))
                return;

            for (int by = height; by < y + height; ++y)
                c.SetType(x, y, z, BlockData.Blocks.Cactus, false);
        }

        private void GenerateTree(IChunk c, int x, int y, int z)
        {
            // Trees should only be placed in direct sunlight
            if (!CanSeeTheSky(x, y + 1, z, c))
                return;

            double r2 = _FastRandom.standNormalDistrDouble();
            /*if (r2 > -2 && r2 < -1)
            {*/
            // Standard tree

            for (int by = y + 4; by < y + 6; by++)
                for (int bx = x - 2; bx <= x + 2; bx++)
                    for (int bz = z - 2; bz <= z + 2; bz++)
                    {
                        c.SetType(bx, by, bz, BlockData.Blocks.Leaves, false);
                        c.SetData(bx, by, bz, 0, false);
                    }

            for (int bx = x - 1; bx <= x + 1; bx++)
                for (int bz = z - 1; bz <= z + 1; bz++)
                {
                    c.SetType(bx, y + 6, bz, BlockData.Blocks.Leaves, false);
                    c.SetData(bx, y + 6, bz, 0, false);
                }

            for (int by = y + 1; by < y + 6; by++)
            {
                c.SetType(x, by, z, BlockData.Blocks.Wood, false);
                c.SetData(x, by, z, 0, false);
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

        private bool CanSeeTheSky(int x, int y, int z, IChunk c)
        {
            int by;
            for (by = y; _blockHelper.Opacity(c.GetType(x, by, z)) == 0 && by < 128; ++by) ;

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

        private void GenerateOuterLayer(int x, int y, int z, int firstBlockHeight, BIOME_TYPE type, IChunk c)
        {
            double heightPercentage = (firstBlockHeight - y) / 128.0;

            switch (type)
            {
                case BIOME_TYPE.PLAINS:
                case BIOME_TYPE.MOUNTAINS:
                // Beach
                if (y >= 60 && y <= 66)
                {
                    c.SetBiomeColumn(x, z, (byte)BIOME_TYPE.MOUNTAINS);
                    c.SetType(x, y, z, BlockData.Blocks.Sand, false);
                    break;
                }

                c.SetBiomeColumn(x, z, (byte)BIOME_TYPE.MOUNTAINS);
                if (heightPercentage == 0.0 && y > 66)
                {
                    // Grass on top
                    c.SetType(x, y, z, BlockData.Blocks.Grass, false);
                }
                else if (heightPercentage > 0.2)
                {
                    // Stone
                    c.SetType(x, y, z, BlockData.Blocks.Stone, false);
                }
                else
                {
                    // Dirt
                    c.SetType(x, y, z, BlockData.Blocks.Dirt, false);
                }

                GenerateRiver(c, x, y, z, heightPercentage, type);
                break;

                case BIOME_TYPE.SNOW:
                c.SetBiomeColumn(x, z, (byte)BIOME_TYPE.SNOW);
                if (heightPercentage == 0.0 && y > 65)
                {
                    // Snow on top
                    c.SetType(x, y, z, BlockData.Blocks.Snow, false);
                    // Grass under the snow
                    c.SetType(x, y - 1, z, BlockData.Blocks.Grass, false);
                }

                else if (heightPercentage > 0.2)
                    // Stone
                    c.SetType(x, y, z, BlockData.Blocks.Stone, false);
                else
                    // Dirt
                    c.SetType(x, y, z, BlockData.Blocks.Dirt, false);
                

                GenerateRiver(c, x, y, z, heightPercentage, type);
                break;

                case BIOME_TYPE.DESERT:
                c.SetBiomeColumn(x, z, (byte)BIOME_TYPE.DESERT);
                /*if (heightPercentage > 0.6 && y < 75)
                {
                    // Stone
                    data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Stone;
                }
                else*/
                if (y < 80)
                    c.SetType(x, y, z, BlockData.Blocks.Sand, false);              

                break;

            }
        }

        protected void GenerateRiver(IChunk c, int x, int y, int z, double heightPercentage, BIOME_TYPE type)
        {
            // Rivers under water? Nope.
            if (y <= 63)
                return;

            double lakeIntens = CalcLakeIntensity(x + c.Coords.ChunkX * 16, z + c.Coords.ChunkZ * 16);
            short currentIndex = (short)(x << 11 | z << 7 | y);

            if (lakeIntens < 0.2)
            {
                if (heightPercentage < 0.001)
                    c.SetType(x, y, z, BlockData.Blocks.Air, false);
                else if (heightPercentage < 0.02)
                {
                    if (type == BIOME_TYPE.SNOW)
                    {
                        // To be sure that there's no snow above us
                        c.SetType(x, y + 1, z, BlockData.Blocks.Air, false);
                        c.SetType(x, y, z, BlockData.Blocks.Ice, false);
                    }
                    else
                        c.SetType(x, y, z, BlockData.Blocks.Still_Water, false);
                    
                }
            }
        }

        protected double CalcLakeIntensity(double x, double z)
        {
            double result = 0.0;
            result += _Gen3.fBm(x * 0.0085, 0, 0.0085 * z, 2, 1.98755, 0.98);
            return Math.Sqrt(Math.Abs(result));
        }

        protected double CalcCaveDensity(double x, double y, double z)
        {
            double result = 0.0;
            result += _Gen6.fBm(x * 0.04, y * 0.04, z * 0.04, 2, 2.0, 0.98);
            return result;
        }

        private void GenerateInnerLayer(int x, int y, int z, BIOME_TYPE type, IChunk c)
        {
            c.SetType(x, y, z, BlockData.Blocks.Stone, false);

        }

        private static double lerp(double t, double q00, double q01)
        {
            return q00 + t * (q01 - q00);
        }

        private static double triLerp(double x, double y, double z, double q000, double q001, double q010, double q011, double q100, double q101, double q110, double q111, double x1, double x2, double y1, double y2, double z1, double z2)
        {
            double distanceX = x2 - x1;
            double distanceY = y2 - y1;
            double distanceZ = z2 - z1;

            double tX = (x - x1) / distanceX;

            double tY = (y - y1) / distanceY;

            double x00 = lerp(tX, q000, q100);
            double x10 = lerp(tX, q010, q110);
            double x01 = lerp(tX, q001, q101);
            double x11 = lerp(tX, q011, q111);
            double r0 = lerp(tY, x00, x01);
            double r1 = lerp(tY, x10, x11);
            return lerp((z - z1) / distanceZ, r0, r1);
        }

        private void triLerpDensityMap(double[, ,] densityMap)
        {
            Parallel.For(0, 16, x =>
            {
                int offsetX = (x/4)*4;
                for (int y = 0; y < 128; y++)
                {
                    int offsetY = (y/8)*8;
                    for (int z = 0; z < 16; z++)
                    {
                        if (!(x%4 == 0 && y%8 == 0 && z%4 == 0))
                        {
                            int offsetZ = (z/4)*4;
                            densityMap[x, y, z] = triLerp(x, y, z, densityMap[offsetX, offsetY, offsetZ],
                                                          densityMap[offsetX, offsetY + 8, offsetZ],
                                                          densityMap[offsetX, offsetY, offsetZ + 4],
                                                          densityMap[offsetX, offsetY + 8, offsetZ + 4],
                                                          densityMap[4 + offsetX, offsetY, offsetZ],
                                                          densityMap[4 + offsetX, offsetY + 8, offsetZ],
                                                          densityMap[4 + offsetX, offsetY, offsetZ + 4],
                                                          densityMap[4 + offsetX, offsetY + 8, offsetZ + 4], offsetX,
                                                          4 + offsetX, offsetY, 8 + offsetY, offsetZ, offsetZ + 4);
                        }
                    }
                }
            });
        }
    }
}