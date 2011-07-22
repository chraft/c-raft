using java.util;


namespace Chraft.World
{
	public class MapGenBase
	{
		public MapGenBase()
		{
			field_947_a = 8;
			rand = new Random();
		}

		public virtual void GenerateA(ChunkGenerator gen, WorldManager world, int x, int z, byte[] data)
		{
			int radius = field_947_a;
			rand.setSeed(world.GetSeed());
			long l = (rand.nextLong() / 2L) * 2L + 1L;
			long l1 = (rand.nextLong() / 2L) * 2L + 1L;
			for (int ix = x - radius; ix <= x + radius; ix++)
			{
				for (int iz = z - radius; iz <= z + radius; iz++)
				{
					rand.setSeed((long)ix * l + (long)iz * l1 ^ world.GetSeed());
					GenerateB(world, ix, iz, x, z, data);
				}
			}
		}

		public virtual void GenerateB(WorldManager world, int i, int j, int k, int l, byte[] abyte0)
		{
		}

		protected int field_947_a;
		protected Random rand;
	}
}