using java.util;


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
            Rand = new Random(seed);
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

        public Chunk ProvideChunk(int x, int z)
        {
            Rand.setSeed((long)x * 0x4f9939f508L + (long)z * 0x1ef1565bd5L);
            Chunk chunk = new Chunk(World, x << 4, z << 4);
            BiomesForGeneration = World.GetWorldChunkManager().GetBlockGeneratorData(BiomesForGeneration, x << 4, z << 4, 16, 16);
            double[] ad = World.GetWorldChunkManager().Temperatures;
            byte[] data = new byte[32768];
            GenerateTerrain(x, z, data, BiomesForGeneration, ad);
            ReplaceBlocksForBiome(x, z, data, BiomesForGeneration);
            CaveGen.GenerateA(this, World, x, z, data);

            for (int bx = 0; bx < 16; bx++)
            {
                for (int by = 0; by < 128; by++)
                {
                    for (int bz = 0; bz < 16; bz++)
                    {
                        if (data[bx << 11 | bz << 7 | by] == 1)
                        {
                            if (Rand.nextInt(100 * by) == 0) data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Diamond_Ore;
                            else if (Rand.nextInt(100 * by) == 0) data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Lapis_Lazuli_Ore;
                            else if (Rand.nextInt(40 * by) == 0) data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Gold_Ore;
                            else if (Rand.nextInt(10 * by) == 0) data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Redstone_Ore_Glowing;
                            else if (Rand.nextInt(4 * by) == 0) data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Iron_Ore;
                            else if (Rand.nextInt(2 * by) == 0) data[bx << 11 | bz << 7 | by] = (byte)BlockData.Blocks.Coal_Ore;
                        }
                        chunk.SetAllBlocks(data);
                    }
                }
            }

            World.Chunks.Add(chunk);

            for (int bx = 0; bx < 16; bx++)
            {
                for (int by = 0; by < 128; by++)
                {
                    for (int bz = 0; bz < 16; bz++)
                    {
                        // TODO: Consider temperature/biome for trees & cacti.
                        if (by > 0 && chunk.GetType(bx, by - 1, bz) == BlockData.Blocks.Grass && Rand.nextInt(140) == 0)
                        {
                            switch (Rand.nextInt(3))
                            {
                                case 0: chunk.GrowTree(bx, by, bz); break;
                                case 1: chunk.GrowTree(bx, by, bz, 2); break;
                                case 2: chunk.GrowTree(bx, by, bz, 1); break;
                            }
                        }

                        //if (by > 63 && chunk.GetType(bx, by - 1, bz) == BlockData.Blocks.Sand && Rand.nextInt(80) == 0)
                        //chunk.PlaceCactus(bx, by, bz);
                    }
                }
            }

            chunk.Recalculate();
            chunk.Save();
            return chunk;
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

        private Random Rand;
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