using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using System.Threading;
using System.Collections.Concurrent;
using Chraft.Net;
using Chraft.Properties;
using Chraft.Net.Packets;

namespace Chraft.World
{
	public partial class ChunkBase
	{
		public const int SIZE = 16 * 16 * 128;

		public delegate void ForEachBlock(int x, int y, int z);

		protected List<Client> Clients = new List<Client>();
		protected List<EntityBase> Entities = new List<EntityBase>();
		protected List<TileEntity> TileEntities = new List<TileEntity>();
        public byte[] Types = new byte[SIZE];

        public NibbleArray Light = new NibbleArray(SIZE);
        public NibbleArray Data = new NibbleArray(SIZE);
        public NibbleArray SkyLight = new NibbleArray(SIZE);

        protected int NumBlocksToUpdate;
        protected int _TimerStarted;
        protected Timer _UpdateTimer;
        protected ConcurrentDictionary<short, short> BlocksToBeUpdated = new ConcurrentDictionary<short, short>();
        protected ReaderWriterLockSlim BlocksUpdateLock = new ReaderWriterLockSlim();

		public WorldManager World { get; private set; }
		public int X { get; set; }
		public int Z { get; set; }

		internal ChunkBase(WorldManager world, int x, int z)
		{
			World = world;
			X = x;
			Z = z;
            _UpdateTimer = new Timer(UpdateBlocksToNearbyPlayers, null, Timeout.Infinite, Timeout.Infinite);
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

        public byte GetBlockLight(int x, int y, int z)
        {
            return (byte)Light.getNibble(x, y, z);
        }

        public byte GetSkyLight(int x, int y, int z)
        {
            return (byte)SkyLight.getNibble(x, y, z);
        }

        public byte GetData(int x, int y, int z)
        {
            return (byte)Data.getNibble(x, y, z);
        }

        public byte GetDualLight(int x, int y, int z)
        {
            return (byte)(Light.getNibble(x, y, z) << 4 | SkyLight.getNibble(x, y, z));
        }

        public void SetData(int x, int y, int z, byte value, bool needsUpdate)
        {
            Data.setNibble(x, y, z, value);

            if(needsUpdate)
                BlockNeedsUpdate(x, y, z);
        }

        public void SetDualLight(int x, int y, int z, byte value)
        {
            byte low = (byte)(value & 0x0F);
            byte high = (byte)((value & 0x0F) >> 4);

            SkyLight.setNibble(x, y, z, low);
            Light.setNibble(x, y, z, high);
        }

        public void SetBlockLight(int x, int y, int z, byte value)
        {
            Light.setNibble(x, y, z, value);
        }

        public void SetSkyLight(int x, int y, int z, byte value)
        {
            SkyLight.setNibble(x, y, z, value);
        }

	    public void SetData(int x, int y, int z, byte value)
		{
			Data.setNibble(x, y, z, value);
		}

		public byte GetLuminance(int x, int y, int z)
		{
			return BlockData.Luminance[this[x, y, z]];
		}

		public byte GetOpacity(int x, int y, int z)
		{
            int index = this[x, y, z];
            return BlockData.Opacity[index];
		}

		public void SetAllBlocks(byte[] data)
		{
			Types = data;
		}

		/*public void ForAdjacentSameChunk(int x, int y, int z, ForEachBlock predicate)
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
		}*/

		public void ForEach(ForEachBlock predicate)
		{
			for (int x = 0; x < 16; x++)
				for (int z = 0; z < 16; z++)
					for (int y = 127; y >=0; --y)
						predicate(x, y, z);
		}

		public BlockData.Blocks GetType(int x, int y, int z)
		{
			return (BlockData.Blocks)this[x, y, z];
		}

		public void SetType(int x, int y, int z, BlockData.Blocks value)
		{
			this[x, y, z] = (byte)value;
            BlockNeedsUpdate(x, y, z);
		}

		public bool IsAir(int x, int y, int z)
		{
			return BlockData.Air.Contains(GetType(x, y, z));
		}

        public void BlockNeedsUpdate(int x, int y, int z)
        {
            int num = Interlocked.Increment(ref NumBlocksToUpdate);

            BlocksUpdateLock.EnterReadLock();
            if (num <= 20)
            {
                short packedCoords = (short) (x << 12 | z << 8 | y);
                BlocksToBeUpdated.AddOrUpdate(packedCoords, packedCoords, (key, oldValue) => packedCoords);
            }
            BlocksUpdateLock.ExitReadLock();

            int started = Interlocked.CompareExchange(ref _TimerStarted, 1, 0);

            if (started == 0)
            {
                _UpdateTimer.Change(100, Timeout.Infinite);
            }
        }

        protected virtual void UpdateBlocksToNearbyPlayers(object state)
        {
            Interlocked.Exchange(ref _TimerStarted, 0);
        }
	}
}
