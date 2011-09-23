
namespace Chraft.WorldGen
{
    public class CustomChunkGenerator : IChunkGenerator
    {
        private static bool GenInit = false;

        private static PerlinNoise _pGen1;
        private static PerlinNoise _pGen2;
        private static PerlinNoise _pGen3;
        private static PerlinNoise _pGen4;
        private static PerlinNoise _pGen5;
        private static FastRandom fastRandom;

        public enum BIOME_TYPE
        {
            MOUNTAINS, SNOW, DESERT, PLAINS
        }

        private void InitGen()
        {
            if (GenInit)
                return;

            GenInit = true;

            _pGen1 = new PerlinNoise(123457);
            _pGen2 = new PerlinNoise(123458);
            _pGen3 = new PerlinNoise(123459);
            _pGen4 = new PerlinNoise(123460);
            _pGen5 = new PerlinNoise(123461);
        }

        private Chunk ProvideChunk(int x, int z, Chunk chunk)
        {
            /*Stopwatch watch = new Stopwatch();
                    
            watch.Start();*/
            InitGen();
            byte[] data = new byte[32768];

            double[, ,] density = new double[17, 129, 17];

            for (int bx = 0; bx <= 16; bx += 4)
            {
                int worldX = bx + (x * 16);
                for (int bz = 0; bz <= 16; bz += 4)
                {
                    BIOME_TYPE type = CalcBiomeType((int)x, (int)z);
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
                        if (by <= 55)
                            data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Stone;
                        else
                        {
                            if (by > 55 && by < 64)
                            {
                                data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Still_Water;
                                if (by == 63 && type == BIOME_TYPE.SNOW)
                                {
                                    data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Ice;
                                }
                            }

                            double dens = density[bx, by, bz];

                            if (dens >= 0.009 && dens <= 0.02)
                            {
                                // Some block was set...
                                if (firstBlockHeight == -1)
                                    firstBlockHeight = by;

                                GenerateOuterLayer(bx, by, bz, firstBlockHeight, type, data);
                            }
                            else if (dens > 0.02)
                            {
                                // Some block was set...
                                if (firstBlockHeight == -1)
                                    firstBlockHeight = by;
                                GenerateInnerLayer(bx, by, bz, type, data);
                            }
                        }
                    }
                }
            }

            //watch.Stop();

            //Console.WriteLine(watch.ElapsedMilliseconds);

            chunk.SetAllBlocks(data);

            return chunk;
        }

        private double FilterNoise(double value)
        {
            return (1.0 + value) * 0.5;
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
            result += _pGen2.fBm(0.0009 * x, 0, 0.0009 * z, 3, 2.2341, 0.94321) + 0.4;
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
                result += _pGen5.noise(x1 * freq[i], y1 * freq[i], z1 * freq[i]) * amp[i];
                ampSum += amp[i];
            }

            return result / ampSum;
        }

        private double CalcTemperature(double x, double z)
        {
            double result = 0.0;
            result += _pGen4.fBm(x * 0.0008, 0, 0.0008 * z, 7, 2.1836171, 0.7631);

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
            else if (temp < 12)
            {
                return BIOME_TYPE.SNOW;
            }

            return BIOME_TYPE.PLAINS;
        }

        private void GenerateOuterLayer(int x, int y, int z, int firstBlockHeight, BIOME_TYPE type, byte[] data)
        {
            double heightPercentage = (firstBlockHeight - y) / 128.0;

            switch (type)
            {
                case BIOME_TYPE.PLAINS:
                case BIOME_TYPE.MOUNTAINS:
                    // Beach
                    if (y >= 60 && y <= 66)
                    {
                        data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Sand;
                        break;
                    }

                    if (heightPercentage == 0.0 && y > 66)
                    {
                        // Grass on top
                        data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Grass;
                    }
                    else if (heightPercentage > 0.2)
                    {
                        // Stone
                        data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Stone;
                    }
                    else
                    {
                        // Dirt
                        data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Dirt;
                    }

                    //generateRiver(c, x, y, z, heightPercentage, type);
                    break;

                case BIOME_TYPE.SNOW:

                    if (heightPercentage == 0.0 && y > 65)
                    {
                        // Snow on top
                        data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Snow;
                    }

                    else if (heightPercentage > 0.2)
                    {
                        // Stone
                        //data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Stone;
                    }
                    else if (data[x << 11 | z << 7 | (y + 1)] == (byte)BlockData.Blocks.Air)
                    {
                        // Grass under the snow
                        data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Grass;
                        data[x << 11 | z << 7 | (y + 1)] = (byte)BlockData.Blocks.Snow;
                    }
                    else
                    {
                        // Dirt
                        data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Dirt;
                    }

                    //generateRiver(c, x, y, z, heightPercentage, type);
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
                        data[x << 11 | z << 7 | y] = (byte)BlockData.Blocks.Sand;
                    }

                    break;

            }
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