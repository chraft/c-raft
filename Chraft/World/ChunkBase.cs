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
using Chraft.World.Blocks;

namespace Chraft.World
{
	public partial class ChunkBase
	{
		public const int SIZE = 16 * 16 * 128;

		public delegate void ForEachBlock(UniversalCoords coords);

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

	    public UniversalCoords Coords { get; set; }

        internal ChunkBase(WorldManager world, UniversalCoords coords)
		{
			World = world;
		    Coords = coords;
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

		public unsafe byte this[UniversalCoords coords]
		{
			get
			{
				fixed (byte* types = Types)
					return types[coords.BlockPackedCoords];
			}
			set
			{
				fixed (byte* types = Types)
                    types[coords.BlockPackedCoords] = value;
			}
		}

        public unsafe byte this[int blockX, int blockY, int blockZ]
        {
            get
            {
                fixed (byte* types = Types)
                    return types[blockX << 11 | blockZ << 7 | blockY];
            }
            set
            {
                fixed (byte* types = Types)
                    types[blockX << 11 | blockZ << 7 | blockY] = value;
            }
        }

        public byte GetBlockLight(UniversalCoords coords)
        {
            return (byte)Light.getNibble(coords.BlockPackedCoords);
        }

        public byte GetBlockLight(int blockX, int blockY, int blockZ)
        {
            return (byte)Light.getNibble(blockX, blockY, blockZ);
        }

        public byte GetSkyLight(UniversalCoords coords)
        {
            return (byte)SkyLight.getNibble(coords.BlockPackedCoords);
        }

        public byte GetSkyLight(int blockX, int blockY, int blockZ)
        {
            return (byte)SkyLight.getNibble(blockX, blockY, blockZ);
        }

        public byte GetData(UniversalCoords coords)
        {
            return (byte)Data.getNibble(coords.BlockPackedCoords);
        }

        public byte GetData(int blockX, int blockY, int blockZ)
        {
            return (byte)Data.getNibble(blockX, blockY, blockZ);
        }

        public byte GetDualLight(UniversalCoords coords)
        {
            return (byte)(Light.getNibble(coords.BlockPackedCoords) << 4 | SkyLight.getNibble(coords.BlockPackedCoords));
        }

        public byte GetDualLight(int blockX, int blockY, int blockZ)
        {
            return (byte)(Light.getNibble(blockX, blockY, blockZ) << 4 | SkyLight.getNibble(blockX, blockY, blockZ));
        }

        public void SetData(UniversalCoords coords, byte value, bool needsUpdate)
        {
            Data.setNibble(coords.BlockPackedCoords, value);

            if (needsUpdate)
                BlockNeedsUpdate(coords.BlockX, coords.BlockY, coords.BlockZ);
        }

        public void SetData(int blockX, int blockY, int blockZ, byte value, bool needsUpdate)
        {
            Data.setNibble(blockX, blockY, blockZ, value);

            if(needsUpdate)
                BlockNeedsUpdate(blockX, blockY, blockZ);
        }

        public void SetDualLight(UniversalCoords coords, byte value)
        {
            byte low = (byte)(value & 0x0F);
            byte high = (byte)((value & 0x0F) >> 4);

            SkyLight.setNibble(coords.BlockPackedCoords, low);
            Light.setNibble(coords.BlockPackedCoords, high);
        }

        public void SetDualLight(int blockX, int blockY, int blockZ, byte value)
        {
            byte low = (byte)(value & 0x0F);
            byte high = (byte)((value & 0x0F) >> 4);

            SkyLight.setNibble(blockX, blockY, blockZ, low);
            Light.setNibble(blockX, blockY, blockZ, high);
        }

        public void SetBlockLight(UniversalCoords coords, byte value)
        {
            Light.setNibble(coords.BlockPackedCoords, value);
        }

        public void SetBlockLight(int blockX, int blockY, int blockZ, byte value)
        {
            Light.setNibble(blockX, blockY, blockZ, value);
        }

        public void SetSkyLight(UniversalCoords coords, byte value)
        {
            SkyLight.setNibble(coords.BlockPackedCoords, value);
        }

        public void SetSkyLight(int blockX, int blockY, int blockZ, byte value)
        {
            SkyLight.setNibble(blockX, blockY, blockZ, value);
        }

        public void SetData(UniversalCoords coords, byte value)
		{
            Data.setNibble(coords.BlockPackedCoords, value);
		}

        public void SetData(int blockX, int blockY, int blockZ, byte value)
        {
            Data.setNibble(blockX, blockY, blockZ, value);
        }

        public byte GetLuminance(UniversalCoords coords)
		{
            return BlockHelper.Instance((byte)GetType(coords)).Luminance;
		}

        public byte GetLuminance(int blockX, int blockY, int blockZ)
        {
            return BlockHelper.Instance((byte)GetType(blockX, blockY, blockZ)).Luminance;
        }

        public byte GetOpacity(UniversalCoords coords)
        {
            return BlockHelper.Instance((byte)GetType(coords)).Opacity;
        }

        public byte GetOpacity(int blockX, int blockY, int blockZ)
		{
            return BlockHelper.Instance((byte)GetType(blockX, blockY, blockZ)).Opacity;
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
						predicate(UniversalCoords.FromBlock(Coords.ChunkX, Coords.ChunkZ, x, y, z));
		}

		public BlockData.Blocks GetType(UniversalCoords coords)
		{
			return (BlockData.Blocks)this[coords];
		}

        public BlockData.Blocks GetType(int blockX, int blockY, int blockZ)
        {
            return (BlockData.Blocks)this[blockX, blockY, blockZ];
        }

        public void SetType(UniversalCoords coords, BlockData.Blocks value)
		{
			this[coords] = (byte)value;
            BlockNeedsUpdate(coords.BlockX, coords.BlockY, coords.BlockZ);
		}

        public void SetType(int blockX, int blockY, int blockZ, BlockData.Blocks value)
        {
            this[blockX, blockY, blockZ] = (byte)value;
            BlockNeedsUpdate(blockX, blockY, blockZ);
        }

        public bool IsAir(UniversalCoords coords)
		{
			return BlockHelper.Instance((byte)GetType(coords)).IsAir;
		}

        public void BlockNeedsUpdate(int blockX, int blockY, int blockZ)
        {
            int num = Interlocked.Increment(ref NumBlocksToUpdate);

            BlocksUpdateLock.EnterReadLock();
            if (num <= 20)
            {
                short packedCoords = (short) (blockX << 12 | blockZ << 8 | blockY);
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
