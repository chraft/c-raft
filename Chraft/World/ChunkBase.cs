using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;

namespace Chraft.World
{
	public partial class ChunkBase
	{
		public const int SIZE = 16 * 16 * 128;

		public delegate void ForEachBlock(int x, int y, int z);

		protected List<Client> Clients = new List<Client>();
		protected List<EntityBase> Entities = new List<EntityBase>();
		protected List<TileEntity> TileEntities = new List<TileEntity>();
		protected unsafe byte[] Types = new byte[SIZE];
		protected unsafe byte[] Light = new byte[SIZE];
		protected unsafe byte[] Data = new byte[SIZE];

		public WorldManager World { get; private set; }
		public int X { get; set; }
		public int Z { get; set; }

		internal ChunkBase(WorldManager world, int x, int z)
		{
			World = world;
			X = x;
			Z = z;
		}

		/// <summary>
		/// Gets a thead-safe array of clients that have the chunk loaded.
		/// </summary>
		/// <returns>Array of clients that have the chunk loaded.</returns>
		public Client[] GetClients()
		{
			lock (Clients)
				return Clients.ToArray();
		}

		/// <summary>
		/// Gets a thead-safe array of entities in the chunk.
		/// </summary>
		/// <returns>Array of entities in the chunk.</returns>
		public EntityBase[] GetEntities()
		{
			lock (Entities)
				return Entities.ToArray();
		}

		/// <summary>
		/// Gets a thead-safe array of tile entities in the chunk.
		/// </summary>
		/// <returns>Array of tile entities in the chunk.</returns>
		public TileEntity[] GetTileEntities()
		{
			lock (TileEntities)
				return TileEntities.ToArray();
		}

		public unsafe byte this[int x, int y, int z]
		{
			get
			{
				fixed (byte* types = Types)
					return types[Translate(x, y, z)];
			}
			set
			{
				fixed (byte* types = Types)
					types[Translate(x, y, z)] = value;
			}
		}

		private int Translate(int x, int y, int z)
		{
			return x << 11 | z << 7 | y;
		}

		public unsafe byte GetBlockLight(int x, int y, int z)
		{
			fixed (byte* light = Light)
				return unchecked((byte)(light[Translate(x, y, z)] >> 4 & 0xf));
		}

		public unsafe byte GetSkyLight(int x, int y, int z)
		{
			fixed (byte* light = Light)
				return unchecked((byte)(light[Translate(x, y, z)] & 0xf));
		}

		public unsafe byte GetData(int x, int y, int z)
		{
			fixed (byte* data = Data)
				return unchecked(data[Translate(x, y, z)]);
		}

		public unsafe byte GetDualLight(int x, int y, int z)
		{
			fixed (byte* light = Light)
				return light[Translate(x, y, z)];
		}

		public unsafe void SetBlockLight(int x, int y, int z, byte value)
		{
			int i = Translate(x, y, z);
			fixed (byte* light = Light)
				light[i] = unchecked((byte)(light[i] & 0xf | (value << 4)));
		}

		public unsafe void SetSkyLight(int x, int y, int z, byte value)
		{
			int i = Translate(x, y, z);
			fixed (byte* light = Light)
				light[i] = unchecked((byte)(light[i] & 0xf0 | value));
		}

		public unsafe void SetDualLight(int x, int y, int z, byte value)
		{
			fixed (byte* light = Light)
				light[Translate(x, y, z)] = value;
		}

		public unsafe void SetData(int x, int y, int z, byte value)
		{
			fixed (byte* data = Data)
				data[Translate(x, y, z)] = value;
		}

		public byte GetLuminence(int x, int y, int z)
		{
			return BlockData.Luminence[this[x, y, z]];
		}

		public byte GetOpacity(int x, int y, int z)
		{
			return BlockData.Opacity[this[x, y, z]];
		}

		public void SetAllBlocks(byte[] data)
		{
			Types = data;
		}

		public void ForAdjacentSameChunk(int x, int y, int z, ForEachBlock predicate)
		{
			if (x > 0)
				predicate(x - 1, y, z);
			if (x < 15)
				predicate(x + 1, y, z);
			if (y > 0)
				predicate(x, y - 1, z);
			if (y < 127)
				predicate(x, y + 1, z);
			if (z > 0)
				predicate(x, y, z - 1);
			if (z < 15)
				predicate(x, y, z + 1);
		}

		public void ForEach(ForEachBlock predicate)
		{
			for (int x = 0; x < 16; x++)
				for (int z = 0; z < 16; z++)
					for (int y = 0; y < 128; y++)
						predicate(x, y, z);
		}

		public BlockData.Blocks GetType(int x, int y, int z)
		{
			return (BlockData.Blocks)this[x, y, z];
		}

		public void SetType(int x, int y, int z, BlockData.Blocks value)
		{
			this[x, y, z] = (byte)value;
		}

		public bool IsAir(int x, int y, int z)
		{
			return BlockData.Air.Contains(GetType(x, y, z));
		}
	}
}
