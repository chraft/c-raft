using java.util;


namespace Chraft.World
{
	public class WorldChunkManager
	{
		private NoiseGeneratorOctaves2 TempNoise;
		private NoiseGeneratorOctaves2 HumidityNoise;
		private NoiseGeneratorOctaves2 NoiseGen3;
		public double[] Temperatures;
		public double[] Humidities;
		public double[] FactorA;
		public Biome[] Biomes;

		public WorldChunkManager(WorldManager world)
		{
			TempNoise = new NoiseGeneratorOctaves2(new Random(world.GetSeed() * 9871L), 4);
			HumidityNoise = new NoiseGeneratorOctaves2(new Random(world.GetSeed() * 39811L), 4);
			NoiseGen3 = new NoiseGeneratorOctaves2(new Random(world.GetSeed() * 0x84a59L), 2);
		}

        public virtual Biome GetBiomeFromCoords(UniversalCoords coords)
		{
            return GetBiome(coords.ChunkX, coords.ChunkZ);
		}

		public virtual Biome GetBiome(int i, int j)
		{
			return LoadBlockGeneratorData(i, j, 1, 1)[0];
		}

		public virtual Biome[] LoadBlockGeneratorData(int i, int j, int k, int l)
		{
			Biomes = GetBlockGeneratorData(Biomes, i, j, k, l);
			return Biomes;
		}

		public virtual double[] GetTemperatures(double[] ad, int i, int j, int k, int l)
		{
			if (ad == null || ad.Length < k * l)
			{
				ad = new double[k * l];
			}
			ad = TempNoise.GetNoise(ad, i, j, k, l, 0.025, 0.025, 0.25);
			FactorA = NoiseGen3.GetNoise(FactorA, i, j, k, l, 0.25, 0.25, 0.59);
			int i1 = 0;
			for (int j1 = 0; j1 < k; j1++)
			{
				for (int k1 = 0; k1 < l; k1++)
				{
					double d = FactorA[i1] * 1.1 + 0.5D;
					double d1 = 0.01D;
					double d2 = 1.0D - d1;
					double d3 = (ad[i1] * 0.15 + 0.7) * d2 + d * d1;
					d3 = 1.0D - (1.0 - d3) * (1.0 - d3);
					if (d3 < 0.0)
					{
						d3 = 0.0;
					}
					if (d3 > 1.0)
					{
						d3 = 1.0;
					}
					ad[i1] = d3;
					i1++;
				}
			}

			return ad;
		}

		public virtual Biome[] GetBlockGeneratorData(Biome[] biomes, int a, int b, int c, int d)
		{
			if (biomes == null || biomes.Length < c * d)
				biomes = new Biome[c * d];

			Temperatures = TempNoise.GetNoise(Temperatures, a, b, c, c, 0.025, 0.025, 0.25);
			Humidities = HumidityNoise.GetNoise(Humidities, a, b, c, c, 0.05, 0.05, 0.33);
			FactorA = NoiseGen3.GetNoise(FactorA, a, b, c, c, 0.25, 0.25, 0.59);

			int i = 0;
			for (int j1 = 0; j1 < c; j1++)
			{
				for (int k1 = 0; k1 < d; k1++)
				{
					double d0 = FactorA[i] * 1.1 + 0.5;
					double d1 = 0.01;
					double d2 = 1.0 - d1;
					double temp = (Temperatures[i] * 0.15 + 0.7) * d2 + d0 * d1;
					d1 = 0.002;
					d2 = 1.0 - d1;
					double humidity = (Humidities[i] * 0.15 + 0.5) * d2 + d0 * d1;
					temp = 1.0d - (1.0d - temp) * (1.0D - temp);

					Temperatures[i] = temp <= 0.0d ? 0.0d : (temp >= 1.0d ? 1.0d : temp);
					Humidities[i] = humidity <= 0.0d ? 0.0d : (humidity >= 1.0d ? 1.0d : humidity);

					biomes[i++] = Biome.GetBiomeFromLookup(temp, humidity);
				}
			}

			return biomes;
		}
	}
}