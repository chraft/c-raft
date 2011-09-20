using java.util;
using LibNoise;
using LibNoise.Modifiers;
using System;
using System.Diagnostics;


namespace Chraft.World
{
    public class ChunkGenerator
    {
        public ChunkGenerator(WorldManager world, long seed)
        {
            SandNoise = new double[256];
            GravelNoise = new double[256];
            StoneNoise = new double[256];
            CaveGen = new MapGenCaves();
            field_707_i = new int[32][];
            for (int i = 0; i < 32; i++)
            {
                field_707_i[i] = new int[32];
            }
            World = world;
            Rand = new java.util.Random(seed);
            Noise1 = new NoiseGeneratorOctaves(Rand, 16);
            Noise2 = new NoiseGeneratorOctaves(Rand, 16);
            Noise3 = new NoiseGeneratorOctaves(Rand, 8);
            Noise4 = new NoiseGeneratorOctaves(Rand, 4);
            Noise5 = new NoiseGeneratorOctaves(Rand, 4);
            Noise6 = new NoiseGeneratorOctaves(Rand, 10);
            Noise7 = new NoiseGeneratorOctaves(Rand, 16);
            MobSpawnerNoise = new NoiseGeneratorOctaves(Rand, 8);
        }

        public void GenerateTerrain(int startX, int startZ, byte[] data, Biome[] biomes, double[] noise)
        {
            byte byte0 = 4;
            byte byte1 = 64;
            int k = byte0 + 1;
            byte byte2 = 17;
            int l = byte0 + 1;
            Noise = GenerateNoise(Noise, startX * byte0, 0, startZ * byte0, k, byte2, l);
            for (int i1 = 0; i1 < byte0; i1++)
            {
                for (int i2 = 0; i2 < byte0; i2++)
                {
                    for (int i3 = 0; i3 < 16; i3++)
                    {
                        double d = 0.125D;
                        double d1 = Noise[((i1 + 0) * l + (i2 + 0)) * byte2 + (i3 + 0)];
                        double d2 = Noise[((i1 + 0) * l + (i2 + 1)) * byte2 + (i3 + 0)];
                        double d3 = Noise[((i1 + 1) * l + (i2 + 0)) * byte2 + (i3 + 0)];
                        double d4 = Noise[((i1 + 1) * l + (i2 + 1)) * byte2 + (i3 + 0)];
                        double d5 = (Noise[((i1 + 0) * l + (i2 + 0)) * byte2 + (i3 + 1)] - d1) * d;
                        double d6 = (Noise[((i1 + 0) * l + (i2 + 1)) * byte2 + (i3 + 1)] - d2) * d;
                        double d7 = (Noise[((i1 + 1) * l + (i2 + 0)) * byte2 + (i3 + 1)] - d3) * d;
                        double d8 = (Noise[((i1 + 1) * l + (i2 + 1)) * byte2 + (i3 + 1)] - d4) * d;
                        for (int i4 = 0; i4 < 8; i4++)
                        {
                            double d9 = 0.25D;
                            double d10 = d1;
                            double d11 = d2;
                            double d12 = (d3 - d1) * d9;
                            double d13 = (d4 - d2) * d9;
                            for (int i5 = 0; i5 < 4; i5++)
                            {
                                int j2 = i5 + i1 * 4 << 11 | 0 + i2 * 4 << 7 | i3 * 8 + i4;
                                char c = '\x0080'; // '\200';
                                double d14 = 0.25D;
                                double stone = d10;
                                double d16 = (d11 - d10) * d14;
                                for (int k2 = 0; k2 < 4; k2++)
                                {
                                    double temperature = noise[(i1 * 4 + i5) * 16 + (i2 * 4 + k2)];
                                    int l2 = 0;
                                    if (i3 * 8 + i4 < byte1)
                                    {
                                        if (temperature < 0.5D && i3 * 8 + i4 >= byte1 - 1)
                                        {
                                            l2 = (int)BlockData.Blocks.Ice;
                                        }
                                        else
                                        {
                                            l2 = (byte)BlockData.Blocks.Water;
                                        }
                                    }
                                    if (stone > 0.0D)
                                    {
                                        l2 = (byte)BlockData.Blocks.Stone;
                                    }

                                    data[j2] = (byte)l2;
                                    j2 += c;
                                    stone += d16;
                                }

                                d10 += d12;
                                d11 += d13;
                            }

                            d1 += d5;
                            d2 += d6;
                            d3 += d7;
                            d4 += d8;
                        }
                    }
                }
            }
        }

        private void ReplaceBlocksForBiome(int i, int j, byte[] data, Biome[] amobspawnerbase)
        {
            byte byte0 = 64;
            double d = 0.03125D;
            SandNoise = Noise4.GenerateNoiseOctaves(SandNoise, i * 16, j * 16, 0.0D, 16, 16, 1, d, d, 1.0D);
            GravelNoise = Noise4.GenerateNoiseOctaves(GravelNoise, i * 16, 109.0134D, j * 16, 16, 1, 16, d, 1.0D, d);
            StoneNoise = Noise5.GenerateNoiseOctaves(StoneNoise, i * 16, j * 16, 0.0D, 16, 16, 1, d * 2D, d * 2D, d * 2D);
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    Biome mobspawnerbase = amobspawnerbase[x + z * 16];
                    bool flag = SandNoise[x + z * 16] + Rand.nextDouble() * 0.20000000000000001D > 0.0D;
                    bool flag1 = GravelNoise[x + z * 16] + Rand.nextDouble() * 0.20000000000000001D > 3D;
                    int i1 = (int)(StoneNoise[x + z * 16] / 3D + 3D + Rand.nextDouble() * 0.25D);
                    int j1 = -1;
                    byte byte1 = mobspawnerbase.TopBlock;
                    byte byte2 = mobspawnerbase.FillerBlock;
                    for (int y = 127; y >= 0; y--)
                    {
                        int l1 = (z * 16 + x) * 128 + y;
                        if (y <= 0 + Rand.nextInt(5))
                        {
                            data[l1] = (byte)BlockData.Blocks.Bedrock;
                            continue;
                        }
                        byte byte3 = data[l1];
                        if (byte3 == 0)
                        {
                            j1 = -1;
                            continue;
                        }
                        if (byte3 != (byte)BlockData.Blocks.Stone)
                        {
                            continue;
                        }
                        if (j1 == -1)
                        {
                            if (i1 <= 0)
                            {
                                byte1 = 0;
                                byte2 = (byte)(byte)BlockData.Blocks.Stone;
                            }
                            else if (y >= byte0 - 4 && y <= byte0 + 1)
                            {
                                byte1 = mobspawnerbase.TopBlock;
                                byte2 = mobspawnerbase.FillerBlock;
                                if (flag1)
                                {
                                    byte1 = 0;
                                }
                                if (flag1)
                                {
                                    byte2 = (byte)BlockData.Blocks.Gravel;
                                }
                                if (flag)
                                {
                                    byte1 = (byte)BlockData.Blocks.Sand;
                                    byte2 = (byte)BlockData.Blocks.Sand;
                                }
                            }
                            if (y < byte0 && byte1 == 0)
                            {
                                byte1 = (byte)BlockData.Blocks.Water;
                            }
                            j1 = i1;
                            if (y >= byte0 - 1)
                            {
                                data[l1] = byte1;
                            }
                            else
                            {
                                data[l1] = byte2;
                            }
                            continue;
                        }
                        if (j1 <= 0)
                        {
                            continue;
                        }
                        j1--;
                        data[l1] = byte2;
                        if (j1 == 0 && byte2 == (byte)BlockData.Blocks.Sand)
                        {
                            j1 = Rand.nextInt(4);
                            byte2 = (byte)BlockData.Blocks.Sandstone;
                        }
                    }
                }
            }
        }

        public static TimeSpan minTime = TimeSpan.MaxValue;
        public static TimeSpan maxTime = TimeSpan.MinValue;

        public static bool GenInit = false;

        public static Perlin lowlandPerlin;
        public static Perlin midlandPerlin;
        public static Perlin highlandPerlin;
        public static Perlin elevation;

        public static RidgedMultifractal fractal;

        public static ScaleBiasOutput scaleBiasLowland;
        public static ScaleBiasOutput scaleBiasMidland;
        public static ScaleBiasOutput scaleBiasHighland;
        public static ScaleBiasOutput scaleBiasElevation;
        public static ScaleBiasOutput scaleBiasFractal;

        public static ScaleInput scaleLowLand;
        public static ScaleInput scaleMidLand;
        public static ScaleInput scaleHighLand;
        public static ScaleInput scaleElevation;

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

        public void InitGen()
        {
            if (GenInit)
                return;

            GenInit = true;

            _pGen1 = new PerlinNoise(123457);
            _pGen2 = new PerlinNoise(123458);
            _pGen3 = new PerlinNoise(123459);
            _pGen4 = new PerlinNoise(123460);
            _pGen5 = new PerlinNoise(123461);
            /*fractal = new RidgedMultifractal();
            fractal.OctaveCount = 5;
            fractal.NoiseQuality = NoiseQuality.High;
            fractal.Frequency = 0.01;
            fractal.Seed = 123457;
            fractal.Lacunarity = 2.5;

            lowlandPerlin = new Perlin();
            lowlandPerlin.OctaveCount = 2;
            lowlandPerlin.Frequency = 0.0008;
            lowlandPerlin.Seed = 123457;
            lowlandPerlin.Lacunarity = 1;
            lowlandPerlin.Persistence = 0.1;
            lowlandPerlin.NoiseQuality = NoiseQuality.High;

            midlandPerlin = new Perlin();
            midlandPerlin.OctaveCount = 4;
            midlandPerlin.Frequency = 0.006;
            midlandPerlin.Seed = 123458;
            midlandPerlin.Lacunarity = 2.1;
            midlandPerlin.Persistence = 0.4;
            midlandPerlin.NoiseQuality = NoiseQuality.High;

            highlandPerlin = new Perlin();
            highlandPerlin.OctaveCount = 7;
            highlandPerlin.Frequency = 0.006;
            highlandPerlin.Seed = 123459;
            highlandPerlin.Lacunarity = 1.30;
            highlandPerlin.Persistence = 1.3;
            highlandPerlin.NoiseQuality = NoiseQuality.High;

            elevation = new Perlin();
            elevation.OctaveCount = 1;
            elevation.Frequency = 0.008;
            elevation.Seed = 123457;
            elevation.Lacunarity = 1;
            elevation.NoiseQuality = NoiseQuality.High;

            scaleBiasLowland = new ScaleBiasOutput(lowlandPerlin);
            scaleBiasLowland.Bias = 0.0;
            scaleBiasLowland.Scale = 1.0;

            scaleBiasMidland = new ScaleBiasOutput(midlandPerlin);
            scaleBiasMidland.Bias = 0.0;
            scaleBiasMidland.Scale = 1.0;

            scaleBiasHighland = new ScaleBiasOutput(highlandPerlin);
            scaleBiasHighland.Bias = 0.0;
            scaleBiasHighland.Scale = 1.0;

            scaleBiasFractal = new ScaleBiasOutput(fractal);
            scaleBiasFractal.Bias = 0;
            scaleBiasFractal.Scale = 0.5;

            scaleBiasElevation = new ScaleBiasOutput(elevation);
            scaleBiasElevation.Bias = 0;
            scaleBiasElevation.Scale = 0.5;

            //ScaleInput scaleFractal = new ScaleInput(fractal, 0.5, 0.0, 0.5);
            scaleLowLand = new ScaleInput(scaleBiasLowland, 1.0, 0.6, 1.0);
            scaleMidLand = new ScaleInput(scaleBiasMidland, 1.0, 0.6, 1.0);
            scaleHighLand = new ScaleInput(scaleBiasHighland, 1.0, 0.6, 1.0);
            scaleElevation = new ScaleInput(scaleBiasElevation, 1.0, 1.0, 1.0);*/
        }

        private double FilterNoise(double value)
        {
            return (1.0 + value) * 0.5;
        }

        public double CalcDensity(double x, double y, double z, BIOME_TYPE type)
        {
            double height = CalcBaseTerrain(x, z);
            double density = CalcMountainDensity(x, y, z);
            double divHeight = (y - 55)*1.5;

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

        double CalcBaseTerrain(double x, double z)
        {
            double result = 0.0;
            result += _pGen2.fBm(0.0009 * x, 0, 0.0009 * z, 3, 2.2341, 0.94321) + 0.4;
            return result;
        }

        double CalcMountainDensity(double x, double y, double z)
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

        double CalcTemperature(double x, double z)
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

        public Chunk ProvideChunk(int x, int z, Chunk chunk)
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
                    else if (data[x << 11 | z << 7 | (y+1)] == (byte)BlockData.Blocks.Air)
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
                    else*/ if(y < 80)
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

        public static double triLerp(double x, double y, double z, double q000, double q001, double q010, double q011, double q100, double q101, double q110, double q111, double x1, double x2, double y1, double y2, double z1, double z2)
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

        private double[] GenerateNoise(double[] ad, int i, int j, int k, int l, int i1, int j1)
        {
            if (ad == null)
            {
                ad = new double[l * i1 * j1];
            }
            double seed1 = 684.41200000000003D;
            double seed2 = 684.41200000000003D;
            double[] temperatures = World.GetWorldChunkManager().Temperatures;
            double[] humidities = World.GetWorldChunkManager().Humidities;
            NoiseA = Noise6.GenerateNoise(NoiseA, i, k, l, j1, 1.121D, 1.121D, 0.5D);
            NoiseB = Noise7.GenerateNoise(NoiseB, i, k, l, j1, 200D, 200D, 0.5D);
            NoiseC = Noise3.GenerateNoiseOctaves(NoiseC, i, j, k, l, i1, j1, seed1 / 80D, seed2 / 160D, seed1 / 80D);
            NoiseD = Noise1.GenerateNoiseOctaves(NoiseD, i, j, k, l, i1, j1, seed1, seed2, seed1);
            NoiseE = Noise2.GenerateNoiseOctaves(NoiseE, i, j, k, l, i1, j1, seed1, seed2, seed1);
            int k1 = 0;
            int l1 = 0;
            int i2 = 16 / l;
            for (int j2 = 0; j2 < l; j2++)
            {
                int k2 = j2 * i2 + i2 / 2;
                for (int l2 = 0; l2 < j1; l2++)
                {
                    int i3 = l2 * i2 + i2 / 2;
                    double d2 = temperatures[k2 * 16 + i3];
                    double d3 = humidities[k2 * 16 + i3] * d2;
                    double d4 = 1.0D - d3;
                    d4 *= d4;
                    d4 *= d4;
                    d4 = 1.0D - d4;
                    double d5 = (NoiseA[l1] + 256D) / 512D;
                    d5 *= d4;
                    if (d5 > 1.0D)
                    {
                        d5 = 1.0D;
                    }
                    double d6 = NoiseB[l1] / 8000D;
                    if (d6 < 0.0D)
                    {
                        d6 = -d6 * 0.29999999999999999D;
                    }
                    d6 = d6 * 3D - 2D;
                    if (d6 < 0.0D)
                    {
                        d6 /= 2D;
                        if (d6 < -1D)
                        {
                            d6 = -1D;
                        }
                        d6 /= 1.3999999999999999D;
                        d6 /= 2D;
                        d5 = 0.0D;
                    }
                    else
                    {
                        if (d6 > 1.0D)
                        {
                            d6 = 1.0D;
                        }
                        d6 /= 8D;
                    }
                    if (d5 < 0.0D)
                    {
                        d5 = 0.0D;
                    }
                    d5 += 0.5D;
                    d6 = (d6 * (double)i1) / 16D;
                    double d7 = (double)i1 / 2D + d6 * 4D;
                    l1++;
                    for (int j3 = 0; j3 < i1; j3++)
                    {
                        double d8 = 0.0D;
                        double d9 = (((double)j3 - d7) * 12D) / d5;
                        if (d9 < 0.0D)
                        {
                            d9 *= 4D;
                        }
                        double d10 = NoiseD[k1] / 512D;
                        double d11 = NoiseE[k1] / 512D;
                        double d12 = (NoiseC[k1] / 10D + 1.0D) / 2D;
                        if (d12 < 0.0D)
                        {
                            d8 = d10;
                        }
                        else if (d12 > 1.0D)
                        {
                            d8 = d11;
                        }
                        else
                        {
                            d8 = d10 + (d11 - d10) * d12;
                        }
                        d8 -= d9;
                        if (j3 > i1 - 4)
                        {
                            double d13 = (float)(j3 - (i1 - 4)) / 3F;
                            d8 = d8 * (1.0D - d13) + -10D * d13;
                        }
                        ad[k1] = d8;
                        k1++;
                    }
                }
            }

            return ad;
        }

        private java.util.Random Rand;
        private NoiseGeneratorOctaves Noise1;
        private NoiseGeneratorOctaves Noise2;
        private NoiseGeneratorOctaves Noise3;
        private NoiseGeneratorOctaves Noise4;
        private NoiseGeneratorOctaves Noise5;
        public NoiseGeneratorOctaves Noise6;
        public NoiseGeneratorOctaves Noise7;
        public NoiseGeneratorOctaves MobSpawnerNoise;
        private WorldManager World;
        private double[] Noise;
        private double[] SandNoise;
        private double[] GravelNoise;
        private double[] StoneNoise;
        private MapGenBase CaveGen;
        private Biome[] BiomesForGeneration;
        private double[] NoiseC;
        private double[] NoiseD;
        private double[] NoiseE;
        private double[] NoiseA;
        private double[] NoiseB;
        private int[][] field_707_i;
    }
}