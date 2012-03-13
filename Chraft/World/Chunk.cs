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
using System.IO;
using System.Threading;
using Chraft.Entity;
using Chraft.Interfaces.Containers;
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Blocks; 
using Chraft.Utilities;
using Chraft.Utils.Config;
using Chraft.World.Blocks;
using Ionic.Zlib;
using Chraft.World.Weather;
using System.Collections;
using System.Diagnostics;
using System.Collections.Concurrent;
using Chraft.Net.Packets;

namespace Chraft.World
{
    public class Chunk : IChunk
    {
        private static object _SavingLock = new object();
        private static volatile bool Saving = false;

        internal bool IsRecalculating { get; set; }
        internal volatile bool Deleted;

        private int MaxHeight;

        internal byte[,] HeightMap { get; private set; }
        internal string DataFile { get { return World.Folder + "/x" + Coords.ChunkX + "_z" + Coords.ChunkZ + ".gz"; } }
        internal bool Persistent { get; set; }
        internal DateTime CreationDate;
        public bool LightToRecalculate { get; set; }
        internal int SpreadingSkylight;

        internal int ChangesToSave;
        internal DateTime LastSaveTime;
        internal DateTime EnqueuedForSaving;
        internal static TimeSpan SaveSpan = TimeSpan.FromSeconds(1.0);

        private ConcurrentDictionary<short, short> BlocksUpdating = new ConcurrentDictionary<short, short>();

        private ConcurrentDictionary<short, short> GrowableBlocks = new ConcurrentDictionary<short, short>();
        private ConcurrentDictionary<short, short> _tempGrowableBlocks = new ConcurrentDictionary<short, short>();

        public ConcurrentDictionary<short, string> SignsText = new ConcurrentDictionary<short, string>();

        public ConcurrentDictionary<short, PersistentContainer> Containers = new ConcurrentDictionary<short, PersistentContainer>();

        public const int SIZE = 16 * 16 * 128;
        public const int HALFSIZE = SIZE / 2;

        internal delegate void ForEachBlock(UniversalCoords coords);

        protected List<Client> Clients = new List<Client>();
        protected List<EntityBase> Entities = new List<EntityBase>();
        protected List<TileEntity> TileEntities = new List<TileEntity>();
        internal byte[] Types = new byte[SIZE];

        internal NibbleArray Light = new NibbleArray(HALFSIZE);
        internal NibbleArray Data = new NibbleArray(HALFSIZE);
        internal NibbleArray SkyLight = new NibbleArray(HALFSIZE);

        protected int NumBlocksToUpdate;

        protected int _TimerStarted;
        protected Timer _UpdateTimer;
        protected ConcurrentDictionary<short, short> BlocksToBeUpdated = new ConcurrentDictionary<short, short>();
        protected ReaderWriterLockSlim BlocksUpdateLock = new ReaderWriterLockSlim();

        internal WorldManager World { get; private set; }

        public UniversalCoords Coords { get; set; }

        /// <summary>
        /// Gets a thead-safe array of clients that have the chunk loaded.
        /// </summary>
        /// <returns>Array of clients that have the chunk loaded.</returns>
        public IClient[] GetClients()
        {
            lock (Clients)
                return Clients.ToArray();
        }

        /// <summary>
        /// Gets a thead-safe array of entities in the chunk.
        /// </summary>
        /// <returns>Array of entities in the chunk.</returns>
        public IEntityBase[] GetEntities()
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

        internal unsafe byte this[UniversalCoords coords]
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

        internal unsafe byte this[int blockX, int blockY, int blockZ]
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

        public void SetLightToRecalculate()
        {
            
        } 

        public IStructBlock GetBlock(UniversalCoords coords)
        {
            byte blockId = (byte)GetType(coords);
            byte blockData = GetData(coords);

            return new StructBlock(coords, blockId, blockData, World);
        }

        public IStructBlock GetBlock(int blockX, int blockY, int blockZ)
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
            return BlockHelper.Instance.Luminance((byte)GetType(coords));
        }

        public byte GetLuminance(int blockX, int blockY, int blockZ)
        {
            return BlockHelper.Instance.Luminance((byte)GetType(blockX, blockY, blockZ));
        }

        public byte GetOpacity(UniversalCoords coords)
        {
            return BlockHelper.Instance.Opacity((byte)GetType(coords));
        }

        public byte GetOpacity(int blockX, int blockY, int blockZ)
        {
            return BlockHelper.Instance.Opacity((byte)GetType(blockX, blockY, blockZ));
        }

        public void SetAllBlocks(byte[] data)
        {
            Types = data;
        }

        internal void ForEach(ForEachBlock predicate)
        {
            for (int x = 0; x < 16; x++)
                for (int z = 0; z < 16; z++)
                    for (int y = 127; y >= 0; --y)
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

            if (needsUpdate)
                BlockNeedsUpdate(coords.BlockX, coords.BlockY, coords.BlockZ);
        }

        public void SetType(int blockX, int blockY, int blockZ, BlockData.Blocks value, bool needsUpdate = true)
        {
            this[blockX, blockY, blockZ] = (byte)value;
            OnSetType(blockX, blockY, blockZ, value);

            if (needsUpdate)
                BlockNeedsUpdate(blockX, blockY, blockZ);
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
            return BlockHelper.Instance.IsAir((byte)GetType(coords));
        }

        public void BlockNeedsUpdate(int blockX, int blockY, int blockZ)
        {
            int num = Interlocked.Increment(ref NumBlocksToUpdate);

            MarkToSave();

            BlocksUpdateLock.EnterReadLock();
            if (num <= 20)
            {
                short packedCoords = (short)(blockX << 12 | blockZ << 8 | blockY);
                BlocksToBeUpdated.AddOrUpdate(packedCoords, packedCoords, (key, oldValue) => packedCoords);
            }
            BlocksUpdateLock.ExitReadLock();

            int started = Interlocked.CompareExchange(ref _TimerStarted, 1, 0);

            if (started == 0)
            {
                _UpdateTimer.Change(100, Timeout.Infinite);
            }
        }

        public void Dispose()
        {
            if (_UpdateTimer != null)
            {
                _UpdateTimer.Dispose();
                _UpdateTimer = null;
            }
        }

        public Chunk(WorldManager world, UniversalCoords coords)
        {
            LightToRecalculate = true;
            World = world;
            Coords = coords;
        }

        internal void InitBlockChangesTimer()
        {
            _UpdateTimer = new Timer(UpdateBlocksToNearbyPlayers, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void RecalculateLight()
        {
            
        }

        public void RecalculateHeight()
        {
            MaxHeight = 0;
            HeightMap = new byte[16, 16];
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                    RecalculateHeight(x, z);
            }
        }

        public void RecalculateHeight(UniversalCoords coords)
        {
            RecalculateHeight(coords.BlockX, coords.BlockZ);
        }

        public void RecalculateHeight(int x, int z)
        {
            int height;
            BlockData.Blocks blockType;
            for (height = 127; height > 0 && (GetOpacity(x, height - 1, z) == 0 || (blockType = GetType(x, height - 1, z)) == BlockData.Blocks.Leaves || blockType == BlockData.Blocks.Water || blockType == BlockData.Blocks.Still_Water); height--) ;
            HeightMap[x, z] = (byte)height;

            if (height > MaxHeight)
                MaxHeight = height;
        }

        public int StackSize;

        public void RecalculateSky()
        {
            int state = Interlocked.Exchange(ref SpreadingSkylight, 1);

            if (state == 1)
                return;

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    RecalculateSky(x, z);
                }
            }

            StackSize = 0;

            while (!World.InitialChunkLightToRecalculate.IsEmpty)
            {
                ChunkLightUpdate chunkUpdate;
                World.InitialChunkLightToRecalculate.TryDequeue(out chunkUpdate);
                if (chunkUpdate != null && chunkUpdate.Chunk != null && !chunkUpdate.Chunk.Deleted)
                {
                    // TODO: check if lightupdate type is sky or block and call the correct spread function
                    chunkUpdate.Chunk.StackSize = 0;
                    chunkUpdate.Chunk.SpreadInitialSkyLightFromBlock((byte)chunkUpdate.X, (byte)chunkUpdate.Y, (byte)chunkUpdate.Z, false);
                }
            }

            LightToRecalculate = false;

            MarkToSave();
        }

        public void RecalculateSky(int x, int z)
        {
            int sky = 15;

            byte y = 127;

            do
            {
                sky -= GetOpacity(x, y, z);

                if (sky < 0)
                    sky = 0;

                SkyLight.setNibble(x, y, z, (byte)sky); 
                
                if(y <= MaxHeight)
                    SpreadInitialSkyLightFromBlock((byte)x, y, (byte)z, false);
            }
            while (--y > 0 && sky > 0);
        }

        

        private void CheckNeighbourHeightAndLight(byte x, byte y, byte z, byte[] skylights, byte[] heights, BitArray directionChunkExist)
        {
            int chunkX = Coords.ChunkX;
            int chunkZ = Coords.ChunkZ;
            Chunk chunk;
            // Take the skylight value of our neighbor blocks
            // Left
            if (x > 0)
            {
                skylights[1] = (byte)SkyLight.getNibble((x - 1), y, z);
                heights[1] = HeightMap[x - 1, z];
            }
            else if ((chunk = World.GetChunkFromChunkSync(chunkX - 1, chunkZ, false, true) as Chunk) != null)
            {
                skylights[1] = (byte)chunk.SkyLight.getNibble((x - 1) & 0xf, y, z);
                heights[1] = chunk.HeightMap[(x - 1) & 0xf, z];
                directionChunkExist[0] = true;
            }

            // Right

            if (x < 15)
            {
                skylights[2] = (byte)SkyLight.getNibble(x + 1, y, z);
                heights[2] = HeightMap[x + 1, z];
            }
            else if ((chunk = World.GetChunkFromChunkSync(chunkX + 1, chunkZ, false, true) as Chunk) != null)
            {
                skylights[2] = (byte)chunk.SkyLight.getNibble((x + 1) & 0xf, y, z);
                heights[2] = chunk.HeightMap[(x + 1) & 0xf, z];
                directionChunkExist[1] = true;
            }

            // Back

            if (z > 0)
            {
                skylights[3] = (byte)SkyLight.getNibble(x, y, z - 1);
                heights[3] = HeightMap[x, z - 1];
            }
            else if ((chunk = World.GetChunkFromChunkSync(chunkX, chunkZ - 1, false, true) as Chunk) != null)
            {
                skylights[3] = (byte)chunk.SkyLight.getNibble(x, y, (z - 1) & 0xf);
                heights[3] = chunk.HeightMap[x, (z - 1) & 0xf];
                directionChunkExist[2] = true;
            }


            // Front

            if (z < 15)
            {
                skylights[4] = (byte)SkyLight.getNibble(x, y, z + 1);
                heights[4] = HeightMap[x, z + 1];
            }
            else if ((chunk = World.GetChunkFromChunkSync(chunkX, chunkZ + 1, false, true) as Chunk) != null)
            {
                skylights[4] = (byte)chunk.SkyLight.getNibble(x, y, (z + 1) & 0xf);
                heights[4] = chunk.HeightMap[x, (z + 1) & 0xf];
                directionChunkExist[3] = true;
            }

            // Up
            skylights[5] = (byte)SkyLight.getNibble(x, y + 1, z);

            // Down
            if (y > 0)
                skylights[6] = (byte)SkyLight.getNibble(x, y - 1, z);
        }

        private byte ChooseHighestHeight(byte[] heights)
        {
            byte maxHeight = heights[0];
           
            if (heights[1] > maxHeight)
                maxHeight = heights[1];

            if (heights[2] > maxHeight)
                maxHeight = heights[2];

            if (heights[3] > maxHeight)
                maxHeight = heights[3];

            if (heights[4] > maxHeight)
                maxHeight = heights[4];
            

            return maxHeight;
        }

        private byte ChooseHighestNeighbourLight(byte[] lights, out byte vertical)
        {
            vertical = 0;

            // Left
            byte newLight = lights[1];

            // Right
            if (lights[2] > newLight)
                newLight = lights[2];

            // Back
            if (lights[3] > newLight)
                newLight = lights[3];

            // Front
            if (lights[4] > newLight)
                newLight = lights[4];

            // Up
            if ((lights[5] + 1) > newLight)
            {
                newLight = lights[5];
                vertical = 1;
            }

            // Down
            if (lights[6] > newLight)
            {
                newLight = lights[6];
            }

            return newLight;

        }

        private void SpreadInitialSkyLightFromBlock(byte x, byte y, byte z, bool checkChange = true)
        {
            if (StackSize > 200)
            {
                World.InitialChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(this, x, y, z));
#if DEBUG
                Console.WriteLine("Rescheduling chunk");
#endif
                return;
            }

            /*if(Coords.ChunkX == -1 && Coords.ChunkZ == -5 && x == -10 && y == 87 && z == -74)
                Console.WriteLine("asd");*/

            BitArray directionChunkExist = new BitArray(4);
            directionChunkExist.SetAll(false);

            byte[] skylights = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
            byte[] heights = new byte[5] { 0, 0, 0, 0, 0 };

            // Take the current block skylight
            skylights[0] = (byte)SkyLight.getNibble(x, y, z);
            heights[0] = HeightMap[x, z];

            int newSkylight = skylights[0];

            CheckNeighbourHeightAndLight(x, y, z, skylights, heights, directionChunkExist);

            if (skylights[0] != 15)
            {
                
                byte vertical;
                newSkylight = ChooseHighestNeighbourLight(skylights, out vertical);

                if (skylights[0] > newSkylight)
                    newSkylight = skylights[0];

                byte opacity = BlockHelper.Instance.CreateBlockInstance(Types[x << 11 | z << 7 | y]).Opacity;

                byte toSubtract = (byte) (1 - vertical + opacity);
                newSkylight -= toSubtract;

                if (newSkylight < 0)
                    newSkylight = 0;


                if (skylights[0] < newSkylight)
                    SetSkyLight(x, y, z, (byte) newSkylight);
                else if (checkChange)
                    return;
                
            }

            // This is the light value we should spread/set to our nearby blocks
            --newSkylight;

            if (newSkylight < 0)
                newSkylight = 0;


            Chunk chunk;
            int chunkX = Coords.ChunkX;
            int chunkZ = Coords.ChunkZ;

            // Spread the light to our neighbor if the nearby has a lower skylight value
            byte neighborCoord;

            neighborCoord = (byte)(x - 1);

            if (x > 0)
            {
                skylights[0] = (byte)SkyLight.getNibble(x - 1, y, z);
                if (skylights[0] < newSkylight && (skylights[0] != 0 || (y + 1) < heights[1]))
                {                   
                    ++StackSize;
                    SpreadInitialSkyLightFromBlock(neighborCoord, y, z);
                    --StackSize;                    
                }
            }
            else if (directionChunkExist[0])
            {
                chunk = World.GetChunkFromChunkSync(chunkX - 1, chunkZ, false, true) as Chunk;
                skylights[0] = (byte)chunk.SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight && (skylights[0] != 0 || (y + 1) < heights[1]))
                    World.InitialChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(chunk, neighborCoord & 0xf, y, z));
            }

            neighborCoord = (byte)(z - 1);

            if (z > 0)
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(x, y, neighborCoord);

                if (skylights[0] < newSkylight && (skylights[0] != 0 || (y + 1) < heights[2]))
                {
                    ++StackSize;
                    SpreadInitialSkyLightFromBlock(x, y, neighborCoord);
                    --StackSize;
                }
            }
            else if (directionChunkExist[2])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                chunk = World.GetChunkFromChunkSync(chunkX, chunkZ - 1, false, true) as Chunk;
                skylights[0] = (byte)chunk.SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight && (skylights[0] != 0 || (y + 1) < heights[2]))
                    World.InitialChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(chunk, x, y, neighborCoord & 0xf));
            }

            neighborCoord = (byte)(x + 1);

            if (x < 15)
            {

                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(neighborCoord, y, z);

                if (skylights[0] < newSkylight && (skylights[0] != 0 || (y + 1) < heights[3]))
                {
                    ++StackSize;
                    SpreadInitialSkyLightFromBlock(neighborCoord, y, z);
                    --StackSize;
                }
            }
            else if (directionChunkExist[1])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                chunk = World.GetChunkFromChunkSync(chunkX + 1, chunkZ, false, true) as Chunk;
                skylights[0] = (byte)chunk.SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight && (skylights[0] != 0 || (y + 1) < heights[3]))
                    World.InitialChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(chunk, neighborCoord & 0xf, y, z));
            }

            neighborCoord = (byte)(z + 1);

            if (z < 15)
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(x, y, neighborCoord);

                if (skylights[0] < newSkylight && (skylights[0] != 0 || (y + 1) < heights[4]))
                {
                    ++StackSize;
                    SpreadInitialSkyLightFromBlock(x, y, neighborCoord);
                    --StackSize;
                }
            }
            else if (directionChunkExist[3])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                chunk = World.GetChunkFromChunkSync(chunkX, chunkZ + 1, false, true) as Chunk;
                skylights[0] = (byte)chunk.SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight && (skylights[0] != 0 || (y + 1) < heights[4]))
                    World.InitialChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(chunk, x, y, neighborCoord & 0xf));
            }

            if ((y + 1) < HeightMap[x, z])
            {
                skylights[0] = (byte)SkyLight.getNibble(x, y + 1, z);

                if (skylights[0] < newSkylight)
                {
                    ++StackSize;
                    SpreadInitialSkyLightFromBlock(x, (byte)(y + 1), z);
                    --StackSize;
                }
            }

            if (y < HeightMap[x, z] && y > 0)
            {
                byte vertical;
                bool top;
                skylights[0] = (byte)SkyLight.getNibble(x, y - 1, z);

                if (skylights[0] < newSkylight)
                {                   
                    ++StackSize;
                    SpreadInitialSkyLightFromBlock(x, (byte) (y - 1), z);
                    --StackSize;                    
                }
            }
        }

        public void SpreadLightFromBlock(byte x, byte y, byte z, byte light, byte oldHeight)
        {
            int newHeight = HeightMap[x, z];
            if (newHeight > oldHeight)
            {
                for (int i = oldHeight; i < newHeight; ++i)
                    SkyLight.setNibble(x, i, z, 0);

                for (int i = oldHeight; i < newHeight; ++i)
                    SpreadSkyLightFromBlock(x, (byte)i, z, true);
                
            }
            else if (newHeight < oldHeight)
            {
                for (int i = newHeight; i < oldHeight; ++i)
                    SkyLight.setNibble(x, i, z, 15);

                for (int i = newHeight; i < oldHeight; ++i)
                    SpreadSkyLightFromBlock(x, (byte)i, z, true);
                
            }
            else
            {
                SpreadSkyLightFromBlock(x, y, z, true);
            }

            // TODO: if light > 0 than the block emits light, so spread it

            while (!World.ChunkLightToRecalculate.IsEmpty)
            {
                ChunkLightUpdate chunkUpdate;
                World.ChunkLightToRecalculate.TryDequeue(out chunkUpdate);
                if (chunkUpdate != null && chunkUpdate.Chunk != null && !chunkUpdate.Chunk.Deleted)
                {
                    // TODO: check if lightupdate type is sky or block and call the correct spread function
                    chunkUpdate.Chunk.StackSize = 0;
                    chunkUpdate.Chunk.SpreadSkyLightFromBlock((byte)chunkUpdate.X, (byte)chunkUpdate.Y, (byte)chunkUpdate.Z, true);
                }
            }
        }

        public void SpreadSkyLightFromBlock(byte x, byte y, byte z, bool sourceBlock=false)
        {
            if (StackSize > 200)
            {
                World.ChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(this, x, y, z));
#if DEBUG
                Console.WriteLine("Rescheduling chunk");
#endif
                return;
            }

            BitArray directionChunkExist = new BitArray(4);
            directionChunkExist.SetAll(false);

            byte[] skylights = new byte[7]{0,0,0,0,0,0,0};
            byte[] heights = new byte[5]{0,0,0,0,0};

            // Take the current block skylight
            skylights[0] = (byte)SkyLight.getNibble(x, y, z);
            heights[0] = HeightMap[x, z];

            int newSkylight = skylights[0];
            
            Chunk chunk;

            int chunkX = Coords.ChunkX;
            int chunkZ = Coords.ChunkZ;

            CheckNeighbourHeightAndLight(x, y, z, skylights, heights, directionChunkExist);
            
            byte vertical;
            newSkylight = ChooseHighestNeighbourLight(skylights, out vertical);

            if (!sourceBlock && skylights[0] > newSkylight)
                newSkylight = skylights[0];

            // Our light value should be the highest neighbour light value - (1 + our opacity)            
            byte opacity = BlockHelper.Instance.CreateBlockInstance(Types[x << 11 | z << 7 | y]).Opacity;

            byte toSubtract = (byte) (1 - vertical + opacity);
            newSkylight -= toSubtract;

            if (newSkylight < 0)
                newSkylight = 0;

            if (skylights[0] != newSkylight)
                SetSkyLight(x, y, z, (byte) newSkylight);
            else if(!sourceBlock)
                return;
            

            // This is the light value we should spread/set to our nearby blocks
            --newSkylight;

            if (newSkylight < 0)
                newSkylight = 0;

            if ((y+1) < HeightMap[x,z])
            {
                if (skylights[5] < newSkylight)
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, (byte)(y + 1), z);
                    --StackSize;
                }
            }

            if (!sourceBlock && y > 0 && y< HeightMap[x, z])
            {
                skylights[0] = (byte)SkyLight.getNibble(x, y - 1, z);

                if (skylights[0] != newSkylight)
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, (byte)(y - 1), z);
                    --StackSize;
                }
            }

            // Spread the light to our neighbor if the nearby has a lower skylight value
            byte neighborCoord;
            
            neighborCoord = (byte)(x - 1);

            if (x > 0)
            {
                skylights[0] = (byte)SkyLight.getNibble(x - 1, y, z);
                if (skylights[0] < newSkylight || (skylights[0] > newSkylight && y < heights[1]))
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(neighborCoord, y, z);
                    --StackSize;
                }
            }
            else if (directionChunkExist[0])
            {
                chunk = World.GetChunkFromChunkSync(chunkX - 1, chunkZ, false, true) as Chunk;
                skylights[0] = (byte)chunk.SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight || (skylights[0] > newSkylight && y < heights[1]))
                    World.ChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(chunk, neighborCoord & 0xf, y, z));
            }

            neighborCoord = (byte)(z - 1);

            if (z > 0)
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(x, y, neighborCoord);

                if (skylights[0] < newSkylight || (skylights[0] > newSkylight && y < heights[2]))
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, y, neighborCoord);
                    --StackSize;
                }
            }
            else if (directionChunkExist[2])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                chunk = World.GetChunkFromChunkSync(chunkX, chunkZ - 1, false, true) as Chunk;
                skylights[0] = (byte)chunk.SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight || (skylights[0] > newSkylight && y < heights[2]))
                    World.ChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(chunk, x, y, neighborCoord & 0xf));
            }

            neighborCoord = (byte)(x + 1);

            if (x < 15)
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(neighborCoord, y, z);

                if (skylights[0] < newSkylight || (skylights[0] > newSkylight && y < heights[3]))
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(neighborCoord, y, z);
                    --StackSize;
                }
            }
            else if (directionChunkExist[1])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                chunk = World.GetChunkFromChunkSync(chunkX + 1, chunkZ, false, true) as Chunk;
                skylights[0] = (byte)chunk.SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight || (skylights[0] > newSkylight && y < heights[3]))
                    World.ChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(chunk, neighborCoord & 0xf, y, z));
            }

            neighborCoord = (byte)(z + 1);

            if (z < 15)
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(x, y, neighborCoord);

                if (skylights[0] < newSkylight || (skylights[0] > newSkylight && y < heights[4]))
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, y, neighborCoord);
                    --StackSize;
                }
            }
            else if (directionChunkExist[3])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                chunk = World.GetChunkFromChunkSync(chunkX, chunkZ + 1, false, true) as Chunk;
                skylights[0] = (byte)chunk.SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight || (skylights[0] > newSkylight && y < heights[4]))
                    World.ChunkLightToRecalculate.Enqueue(new ChunkLightUpdate(chunk, x, y, neighborCoord & 0xf));
            } 
        }

        private static bool CanLoad(string path)
        {
            return ChraftConfig.LoadFromSave && File.Exists(path);
        }

        public static Stopwatch watch = new Stopwatch();
        public static Chunk Load(UniversalCoords coords, WorldManager world)
        {  
            string path = world.Folder + "/x" + coords.ChunkX + "_z" + coords.ChunkZ + ".gz";

            if (!CanLoad(path))
                return null;

            Stream zip = null;

            Chunk chunk = new Chunk(world, coords);
            
            try
            {
                zip = new DeflateStream(File.Open(path, FileMode.Open), CompressionMode.Decompress);

                int version = zip.ReadByte();

                switch(version)
                {
                    /* When there's a new mod you do:
                    case 1:
                    {
                     * dosomething
                     * goto case 0;
                    }*/
                    case 0:
                    {
                        chunk.LightToRecalculate = Convert.ToBoolean(zip.ReadByte());
                        chunk.HeightMap = new byte[16,16];
                        int height;
                        chunk.MaxHeight = 0;
                        for (int x = 0; x < 16; ++x)
                        {
                            for (int z = 0; z < 16; ++z)
                            {
                                height = chunk.HeightMap[x, z] = (byte) zip.ReadByte();

                                if (chunk.MaxHeight < height)
                                    chunk.MaxHeight = height;
                            }
                        }
                        chunk.LoadAllBlocks(zip);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                world.Logger.Log(ex);
                return null;
            }
            if (zip != null)
                zip.Dispose();

            (BlockHelper.Instance.CreateBlockInstance((byte)BlockData.Blocks.Sign_Post) as BlockSignBase).LoadSignsFromDisk(chunk, world.SignsFolder);

            return chunk;

        }
        private static byte[] _readBuffer = new byte[16*16*128*3];
        private int _index;

        private void LoadAllBlocks(Stream strm)
        {
            strm.Read(Types, 0, SIZE);
            strm.Read(Data.Data, 0, HALFSIZE);
            strm.Read(Light.Data, 0, HALFSIZE);
            strm.Read(SkyLight.Data, 0, HALFSIZE);
        }

        /*private void LoadBlock(int x, int y, int z, Stream strm)
        {
            byte type = (byte)strm.ReadByte();
            byte data = (byte)strm.ReadByte();
            byte ls = (byte)strm.ReadByte();
            this[x, y, z] = type;
            SetData(x, y, z, data, false);
            SetDualLight(x, y, z, ls);
            if (BlockHelper.IsGrowable(type))
            {
                short packedCoords = (short) (x << 12 | z << 8 | y);
                _tempGrowableBlocks.TryAdd(packedCoords, packedCoords);
            }
        }*/

        private bool EnterSave()
        {
            Monitor.Enter(_SavingLock);
            if (Saving)
            {
                Monitor.Exit(_SavingLock);
                return false;
            }
            Saving = true;
            return true;
        }

        private void ExitSave()
        {
            Saving = false;
            Monitor.Exit(_SavingLock);
        }

        private void WriteAllBlocks(Stream strm)
        {
            strm.Write(Types, 0, SIZE);
            strm.Write(Data.Data, 0, HALFSIZE);
            strm.Write(Light.Data, 0, HALFSIZE);
            strm.Write(SkyLight.Data, 0, HALFSIZE);
        }

        public void MarkToSave()
        {
            int changes = Interlocked.Increment(ref ChangesToSave);

            if (changes == 1)
            {
                EnqueuedForSaving = DateTime.Now;
                if ((DateTime.Now - LastSaveTime) > SaveSpan)
                    World.ChunksToSave.Enqueue(this);                
                else
                    World.ChunksToSavePostponed.Enqueue(this);                
            }
        }

        internal void Save()
        {
            if (!EnterSave())
                return;

            Stream zip = new DeflateStream(File.Create(DataFile + ".tmp"), CompressionMode.Compress);
            try
            {
                zip.WriteByte(0); // version

                zip.WriteByte(Convert.ToByte(LightToRecalculate));
                for (int x = 0; x < 16; ++x)
                {
                    for (int z = 0; z < 16; ++z)
                    {
                        zip.WriteByte(HeightMap[x, z]);
                    }
                }
                WriteAllBlocks(zip);
                zip.Flush();
            }
            finally
            {
                try
                {
                    zip.Dispose();
                    File.Delete(DataFile);
                    File.Move(DataFile + ".tmp", DataFile);
                }
                catch
                {
                }
                finally
                {
                    ExitSave();
                }
            }
        }

        internal void AddClient(Client client)
        {
            lock (Clients)
                Clients.Add(client);
            lock (Entities)
                Entities.Add(client.Owner);
        }

        internal void RemoveClient(Client client)
        {
            lock (Clients)
                Clients.Remove(client);
            lock (Entities)
                Entities.Remove(client.Owner);

            if (Clients.Count == 0 && !Persistent)
            {
                ContainerFactory.UnloadContainers(this);
                Save();
                World.RemoveChunk(this);
            }
        }

        internal void OnSetType(UniversalCoords coords, BlockData.Blocks value)
        {
            byte blockId = (byte)value;

            if (GrowableBlocks.ContainsKey(coords.BlockPackedCoords))
            {
                short unused;

                if (!BlockHelper.Instance.IsGrowable(blockId))
                {
                    GrowableBlocks.TryRemove(coords.BlockPackedCoords, out unused);
                }
                else
                {
                    StructBlock block = new StructBlock(coords, blockId, GetData(coords), World);
                    if (!(BlockHelper.Instance.CreateBlockInstance(blockId) as IBlockGrowable).CanGrow(block, this))
                    {
                        GrowableBlocks.TryRemove(coords.BlockPackedCoords, out unused);
                    }
                }
            }
            else
            {
                if (BlockHelper.Instance.IsGrowable(blockId))
                {
                    StructBlock block = new StructBlock(coords, blockId, GetData(coords), World);
                    if ((BlockHelper.Instance.CreateBlockInstance(blockId) as IBlockGrowable).CanGrow(block, this))
                    {
                        GrowableBlocks.TryAdd(coords.BlockPackedCoords, coords.BlockPackedCoords);
                    }
                }
            }
        }

        internal void OnSetType(int blockX, int blockY, int blockZ, BlockData.Blocks value)
        {
            byte blockId = (byte)value;
            short blockPackedCoords = (short)(blockX << 11 | blockZ << 7 | blockY);


            if (GrowableBlocks.ContainsKey(blockPackedCoords))
            {
                short unused;

                if (!BlockHelper.Instance.IsGrowable(blockId))
                {
                    GrowableBlocks.TryRemove(blockPackedCoords, out unused);
                }
                else
                {
                    byte metaData = GetData(blockX, blockY, blockZ);
                    StructBlock block = new StructBlock(UniversalCoords.FromBlock(Coords.ChunkX, Coords.ChunkZ, blockX, blockY, blockZ), blockId, metaData, World);
                    if (!(BlockHelper.Instance.CreateBlockInstance(blockId) as IBlockGrowable).CanGrow(block, this))
                    {
                        GrowableBlocks.TryRemove(blockPackedCoords, out unused);
                    }
                }
            }
            else
            {
                if (BlockHelper.Instance.IsGrowable(blockId))
                {
                    byte metaData = GetData(blockX, blockY, blockZ);
                    UniversalCoords blockCoords = UniversalCoords.FromBlock(Coords.ChunkX, Coords.ChunkZ, blockX, blockY,
                                                                            blockZ);
                    StructBlock block = new StructBlock(blockCoords, blockId, metaData, World);
                    if ((BlockHelper.Instance.CreateBlockInstance(blockId) as IBlockGrowable).CanGrow(block, this))
                        GrowableBlocks.TryAdd(blockPackedCoords, blockPackedCoords);
                }
            }
        }

        internal void InitGrowableCache()
        {
            byte blockId = 0;
            byte blockMeta = 0;
            UniversalCoords blockCoords;
            StructBlock block;
            int blockX, blockY, blockZ;

            foreach (var coord in _tempGrowableBlocks)
            {
                blockX = coord.Key >> 11;
                blockY = (coord.Key & 0xff) % 128;
                blockZ = (coord.Key >> 7) & 0xf;
                blockId = (byte)GetType(blockX, blockY, blockZ);
                blockCoords = UniversalCoords.FromBlock(Coords.ChunkX, Coords.ChunkZ, blockX, blockY, blockZ);
                blockMeta = GetData(blockX, blockY, blockZ);
                block = new StructBlock(blockCoords, blockId, blockMeta, World);
                if ((BlockHelper.Instance.CreateBlockInstance(blockId) as IBlockGrowable).CanGrow(block, this))
                    GrowableBlocks.TryAdd(blockCoords.BlockPackedCoords, blockCoords.BlockPackedCoords);
            }
            _tempGrowableBlocks = new ConcurrentDictionary<short, short>();
        }

        internal void Grow()
        {
            byte blockId = 0;
            byte metaData = 0;
            short unused;
            StructBlock block;
            IBlockGrowable iGrowable;
            int blockX, blockY, blockZ;
            byte light, sky;

            //short blockPackedCoords = (short)(blockX << 11 | blockZ << 7 | blockY);
            foreach (var growableBlock in GrowableBlocks)
            {
                blockX = growableBlock.Key >> 11;
                blockY = (growableBlock.Key & 0xff) % 128;
                blockZ = (growableBlock.Key >> 7) & 0xf;

                blockId = (byte)GetType(blockX, blockY, blockZ);
                light = GetBlockLight(blockX, blockY, blockZ);
                sky = GetSkyLight(blockX, blockY, blockZ);
                if (BlockHelper.Instance.IsGrowable(blockId))
                {
                    metaData = GetData(blockX, blockY, blockZ);
                    block = new StructBlock(UniversalCoords.FromBlock(Coords.ChunkX, Coords.ChunkZ, blockX, blockY, blockZ), blockId, metaData, World);
                    iGrowable = (BlockHelper.Instance.CreateBlockInstance(blockId) as IBlockGrowable);
                    if (iGrowable.CanGrow(block, this))
                    {
                        iGrowable.Grow(block, this);

                        continue;
                    }
                }
                GrowableBlocks.TryRemove(growableBlock.Key, out unused);
            }
        }

        /*private void Grow(UniversalCoords coords)
        {
            BlockData.Blocks type = GetType(coords);
            byte metaData = GetData(coords);

            if (!(BlockHelper.Instance((byte)type) is IBlockGrowable))
                return;

            UniversalCoords oneUp = UniversalCoords.FromAbsWorld(coords.WorldX, coords.WorldY + 1, coords.WorldZ);
            byte light = GetBlockLight(oneUp);
            byte sky = GetSkyLight(oneUp);

            StructBlock thisBlock = new StructBlock(coords, (byte)type, metaData, this.World);
            IBlockGrowable blockToGrow = (BlockHelper.Instance((byte)type) as IBlockGrowable);
            blockToGrow.Grow(thisBlock);

            switch (type)
            {
                case BlockData.Blocks.Grass:
                    GrowDirt(coords);
                    break;
            }

            if (light < 7 && sky < 7)
            {
                SpawnMob(oneUp);
                return;
            }
            if (type == BlockData.Blocks.Grass)
                SpawnAnimal(coords);
        }*/

        internal void ForAdjacent(UniversalCoords coords, ForEachBlock predicate)
        {
            predicate(UniversalCoords.FromWorld(coords.WorldX - 1, coords.WorldY, coords.WorldZ));
            predicate(UniversalCoords.FromWorld(coords.WorldX + 1, coords.WorldY, coords.WorldZ));
            predicate(UniversalCoords.FromWorld(coords.WorldX, coords.WorldY, coords.WorldZ - 1));
            predicate(UniversalCoords.FromWorld(coords.WorldX, coords.WorldY, coords.WorldZ + 1));
            if (coords.BlockY > 0)
                predicate(UniversalCoords.FromWorld(coords.WorldX, coords.WorldY - 1, coords.WorldZ));
            if (coords.BlockY < 127)
                predicate(UniversalCoords.FromWorld(coords.WorldX, coords.WorldY + 1, coords.WorldZ));
        }

        internal void ForNSEW(UniversalCoords coords, ForEachBlock predicate)
        {
            predicate(UniversalCoords.FromWorld(coords.WorldX - 1, coords.WorldY, coords.WorldZ));
            predicate(UniversalCoords.FromWorld(coords.WorldX + 1, coords.WorldY, coords.WorldZ));
            predicate(UniversalCoords.FromWorld(coords.WorldX, coords.WorldY, coords.WorldZ - 1));
            predicate(UniversalCoords.FromWorld(coords.WorldX, coords.WorldY, coords.WorldZ + 1));
        }

        public bool IsAdjacentTo(UniversalCoords coords, byte block)
        {
            bool retval = false;
            ForAdjacent(coords, delegate(UniversalCoords uc)
            {
                retval = retval || World.GetBlockId(uc) == block;
            });
            return retval;
        }

        public bool IsNSEWTo(UniversalCoords coords, byte block)
        {
            bool retval = false;
            ForNSEW(coords, delegate(UniversalCoords uc)
            {
                if (World.GetBlockId(uc) == block)
                    retval = true;
            });
            return retval;
        }

        internal void GrowCactus(UniversalCoords coords)
        {
            if (GetType(coords) == BlockData.Blocks.Cactus)
                return;

            if (GetType(UniversalCoords.FromWorld(coords.WorldX, coords.WorldY - 3, coords.WorldZ)) == BlockData.Blocks.Cactus)
                return;

            if (!IsNSEWTo(coords, (byte)BlockData.Blocks.Air))
                return;

            if (World.Server.Rand.Next(60) == 0)
            {
                SetType(coords, BlockData.Blocks.Cactus);
            }
        }

        private void GrowDirt(UniversalCoords coords)
        {
            if (coords.WorldY >= 127 || IsAir(UniversalCoords.FromWorld(coords.WorldX, coords.WorldY + 1, coords.WorldZ)))
                return;

            if (World.Server.Rand.Next(30) != 0)
            {
                SetType(coords, BlockData.Blocks.Dirt);
            }
        }

        internal void SetWeather(WeatherState weather)
        {
            foreach (Client c in GetClients())
            {
                c.SendWeather(weather, Coords);
            }
        }

        protected void UpdateBlocksToNearbyPlayers(object state)
        {
            BlocksUpdateLock.EnterWriteLock();
            int num = Interlocked.Exchange(ref NumBlocksToUpdate, 0);
            ConcurrentDictionary<short, short> temp = BlocksToBeUpdated;
            BlocksToBeUpdated = BlocksUpdating;
            BlocksUpdateLock.ExitWriteLock();

            BlocksUpdating = temp;

            if (num == 1)
            {
                short keyCoords = BlocksUpdating.Keys.First();
                short index;
                BlocksUpdating.TryGetValue(keyCoords, out index);
                int blockX = (index >> 12 & 0xf);
                int blockY = (index & 0xff);
                int blockZ = (index >> 8 & 0xf);
                byte blockId = (byte)GetType(blockX, blockY, blockZ);
                byte data = GetData(blockX, blockY, blockZ);

                World.Server.SendPacketToNearbyPlayers(World, Coords, new BlockChangePacket 
                {X = Coords.WorldX + blockX, Y = (sbyte) blockY, Z = Coords.WorldZ + blockZ, Data = data, Type = blockId});
                
            }
            else if (num < 20)
            {
                sbyte[] data = new sbyte[num];
                sbyte[] types = new sbyte[num];
                short[] blocks = new short[num];

                int count = 0;
                foreach (short key in BlocksUpdating.Keys)
                {
                    short index;
                    BlocksUpdating.TryGetValue(key, out index);
                    int blockX = (index >> 12 & 0xf);
                    int blockY = (index & 0xff);
                    int blockZ = (index >> 8 & 0xf);

                    data[count] = (sbyte)GetData(blockX, blockY, blockZ);
                    types[count] = (sbyte)GetType(blockX, blockY, blockZ);
                    blocks[count] = index;
                    ++count;
                }
                World.Server.SendPacketToNearbyPlayers(World, Coords, new MultiBlockChangePacket { CoordsArray = blocks, Metadata = data, Types = types, ChunkCoords = Coords });
            }
            else
            {
                World.Server.SendPacketToNearbyPlayers(World, Coords, new MapChunkPacket { Chunk = this });
            }

            BlocksUpdating.Clear();
            Interlocked.Exchange(ref _TimerStarted, 0);
        }
    }
}
