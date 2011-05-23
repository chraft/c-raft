using java.util;
using java.lang;
using Chraft.Entity;


namespace Chraft.World
{
	public class Biome
	{
		private static Biome Rainforest = (new Biome()).SetColor(0x8fa36).SetBiomeName("Rainforest").SetSeed(0x1ff458);
		private static Biome Swampland = (new Biome()).SetColor(0x7f9b2).SetBiomeName("Swampland").SetSeed(0x8baf48);
		private static Biome SeasonalForest = (new Biome()).SetColor(0x9be023).SetBiomeName("Seasonal Forest");
		private static Biome Forest = (new Biome()).SetColor(0x56621).SetBiomeName("Forest").SetSeed(0x4eba31);
		private static Biome Savanna = (new Biome()).SetColor(0xd9e023).SetBiomeName("Savanna");
		private static Biome Shrubland = (new Biome()).SetColor(0xa1ad20).SetBiomeName("Shrubland");
		private static Biome Taiga = (new Biome()).SetColor(0x2eb153).SetBiomeName("Taiga").SetSeed(0x7bb731);
		private static Biome Desert = (new Biome()).SetColor(0xfa9418).SetBiomeName("Desert");
		private static Biome Plains = (new Biome()).SetColor(0xffd910).SetBiomeName("Plains");
		private static Biome Tundra = (new Biome()).SetColor(0x57ebf9).SetBiomeName("Tundra").SetSeed(0xc4d339);

		public string BiomeName;
		public int Color;
		public byte TopBlock;
		public byte FillerBlock;
		public int Seed;
		private static Biome[] BiomeLookupTable = new Biome[4096];

		public Biome()
		{
			TopBlock = (byte)BlockData.Blocks.Grass;
			FillerBlock = (byte)BlockData.Blocks.Dirt;
			Seed = 0x4ee031;
		}

		static Biome()
		{
			GenerateBiomeLookup();
		}

		public static void GenerateBiomeLookup()
		{
			for (int x = 0; x < 64; x++)
			{
				for (int z = 0; z < 64; z++)
					BiomeLookupTable[x + z * 64] = GetBiome((float)x / 63.0f, (float)z / 63.0f);
			}

			Desert.TopBlock = Desert.FillerBlock = (byte)BlockData.Blocks.Sand;
		}

		protected virtual Biome Unknown1()
		{
			return this;
		}

		protected virtual Biome SetBiomeName(string s)
		{
			BiomeName = s;
			return this;
		}

		protected virtual Biome SetSeed(int i)
		{
			Seed = i;
			return this;
		}

		protected virtual Biome SetColor(int i)
		{
			Color = i;
			return this;
		}

		public static Biome GetBiomeFromLookup(double d, double d1)
		{
			int x = (int)(d * 63.0);
			int z = (int)(d1 * 63.0);
			if (z < 0) z = 0;
			if (x < 0) x = 0;
			int i = x + z * 64;
			if (i >= BiomeLookupTable.Length) i = BiomeLookupTable.Length - 1;
			return BiomeLookupTable[i];
		}

		public static Biome GetBiome(float temp, float humidity)
		{
			humidity *= temp;
			if (temp < 0.1F)
				return Tundra;

			if (humidity < 0.2F)
			{
				if (temp < 0.5F)
					return Tundra;
				if (temp < 0.95F)
					return Savanna;
				return Desert;
			}

			if (humidity > 0.5F && temp < 0.7F)
				return Swampland;

			if (temp < 0.5F)
				return Taiga;

			if (temp < 0.97F)
			{
				if (humidity < 0.35F)
					return Shrubland;
				return Forest;
			}

			if (humidity < 0.45F)
				return Plains;

			if (humidity < 0.9F)
				return SeasonalForest;

			return Rainforest;
		}
	}
}