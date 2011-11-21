#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
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
	    public int ChangesToSave;

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

        public StructBlock GetBlock(UniversalCoords coords)
        {
            byte blockId = (byte)GetType(coords);
            byte blockData = GetData(coords);

            return new StructBlock(coords, blockId, blockData, World);
        }

        public StructBlock GetBlock(int blockX, int blockY, int blockZ)
        {
            byte blockId = (byte)GetType(blockX, blockY, blockZ);
            byte blockData = GetData(blockX, blockY, blockZ);

            return new StructBlock(blockX + Coords.WorldX, blockY, blockZ + Coords.WorldZ, blockId, blockData, World);
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

        public byte GetLuminance(UniversalCoords coords)
		{
            return BlockHelper.Luminance((byte)GetType(coords));
		}

        public byte GetLuminance(int blockX, int blockY, int blockZ)
        {
            return BlockHelper.Luminance((byte)GetType(blockX, blockY, blockZ));
        }

        public byte GetOpacity(UniversalCoords coords)
        {
            return BlockHelper.Opacity((byte)GetType(coords));
        }

        public byte GetOpacity(int blockX, int blockY, int blockZ)
		{
            return BlockHelper.Opacity((byte)GetType(blockX, blockY, blockZ));
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

        public void SetType(UniversalCoords coords, BlockData.Blocks value, bool needsUpdate = true)
		{
			this[coords] = (byte)value;            
            OnSetType(coords, value);

            if(needsUpdate)
                BlockNeedsUpdate(coords.BlockX, coords.BlockY, coords.BlockZ);
		}

        public void SetType(int blockX, int blockY, int blockZ, BlockData.Blocks value, bool needsUpdate = true)
        {
            this[blockX, blockY, blockZ] = (byte)value;            
            OnSetType(blockX, blockY, blockZ, value);

            if(needsUpdate)
                BlockNeedsUpdate(blockX, blockY, blockZ);
        }

        public virtual void OnSetType(UniversalCoords coords, BlockData.Blocks value)
        {
            
        }

        public virtual void OnSetType(int blockX, int blockY, int blockZ, BlockData.Blocks value)
        {

        }

        public void SetBlockAndData(UniversalCoords coords, byte type, byte data, bool needsUpdate = true)
        {
            SetType(coords, (BlockData.Blocks)type, false);
            SetData(coords, data, false);

            if (needsUpdate)
                BlockNeedsUpdate(coords.BlockX, coords.BlockY, coords.BlockZ);
        }

        public void SetBlockAndData(int blockX, int blockY, int blockZ, byte type, byte data, bool needsUpdate = true)
        {
            SetType(blockX, blockY, blockZ, (BlockData.Blocks)type, false);
            SetData(blockX, blockY, blockZ, data, false);

            if (needsUpdate)
                BlockNeedsUpdate(blockX, blockY, blockZ);
        }

        public void SetData(UniversalCoords coords, byte value, bool needsUpdate = true)
        {
            Data.setNibble(coords.BlockPackedCoords, value);

            if (needsUpdate)
                BlockNeedsUpdate(coords.BlockX, coords.BlockY, coords.BlockZ);
        }

        public void SetData(int blockX, int blockY, int blockZ, byte value, bool needsUpdate = true)
        {
            Data.setNibble(blockX, blockY, blockZ, value);

            if (needsUpdate)
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

        public bool IsAir(UniversalCoords coords)
		{
            return BlockHelper.IsAir((byte)GetType(coords));
		}

        public void MarkToSave()
        {
            int changes = Interlocked.Increment(ref ChangesToSave);

            if(changes == 1)
                World.ChunksToSave.Enqueue(this);
        }

        public virtual void Save()
        {
            
        }

        public void BlockNeedsUpdate(int blockX, int blockY, int blockZ)
        {
            int num = Interlocked.Increment(ref NumBlocksToUpdate);

            MarkToSave();

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

        public void Dispose()
        {
            if (_UpdateTimer != null)
            {
                _UpdateTimer.Dispose();
                _UpdateTimer = null;
            }
        }
	}
}
