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
using Chraft.Interfaces.Containers;
using Chraft.Net;
using Chraft.Properties;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Interfaces;
using Ionic.Zlib;
using Chraft.World.Weather;
using System.Collections;
using System.Diagnostics;
using System.Collections.Concurrent;
using Chraft.Net.Packets;

namespace Chraft.World
{
    public class Chunk : ChunkBase
    {
        private static object _SavingLock = new object();
        private static volatile bool Saving = false;

        public bool IsRecalculating {get; set;}
        public volatile bool Deleted;

        private int MaxHeight;

        public byte[,] HeightMap { get; private set; }
        public string DataFile { get { return World.Folder + "/x" + Coords.ChunkX + "_z" + Coords.ChunkZ + ".gz"; } }
        public bool Persistent { get; set; }
        public DateTime CreationDate;
        public bool LightToRecalculate;
        public int SpreadingSkylight;

        private ConcurrentDictionary<short, short> BlocksUpdating = new ConcurrentDictionary<short, short>();

        private ConcurrentDictionary<short, short> GrowableBlocks = new ConcurrentDictionary<short, short>();
        private ConcurrentDictionary<short, short> _tempGrowableBlocks = new ConcurrentDictionary<short, short>();

        public ConcurrentDictionary<short, string> SignsText = new ConcurrentDictionary<short, string>();

        public ConcurrentDictionary<short, PersistentContainer> Containers = new ConcurrentDictionary<short, PersistentContainer>();

        internal Chunk(WorldManager world, UniversalCoords coords)
            : base(world, coords)
        {
           
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
                    SpreadSkyLightFromBlock((byte)x, y, (byte)z, true);
            }
            while (--y > 0 && sky > 0);

            StackSize = 0;

            while (World.ChunksToRecalculate.Count > 0)
            {
                ChunkLightUpdate chunkUpdate;
                World.ChunksToRecalculate.TryDequeue(out chunkUpdate);
                if (chunkUpdate != null && chunkUpdate.Chunk != null && !chunkUpdate.Chunk.Deleted)
                {
                    chunkUpdate.Chunk.StackSize = 0;                   
                    chunkUpdate.Chunk.SpreadSkyLightFromBlock((byte)chunkUpdate.X, (byte)chunkUpdate.Y, (byte)chunkUpdate.Z);
                }
            }

            LightToRecalculate = false;
        }

        public int StackSize;

        public void SpreadSkyLightFromBlock(byte x, byte y, byte z, bool checkHeight = false)
        {
            if (StackSize > 200)
            {
                World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(this, x, y, z));
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
            skylights[0] = (byte)SkyLight.getNibble(x,y,z);
            heights[0] = HeightMap[x, z];

            int newSkylight = skylights[0];
            int chunkX = Coords.ChunkX;
            int chunkZ = Coords.ChunkZ;
            Chunk chunk;

            // Take the skylight value of our neighbor blocks
            // Left
            if (x > 0)
            {
                skylights[1] = (byte) SkyLight.getNibble((x - 1), y, z);
                heights[1] = HeightMap[x - 1, z];
            }
            else if ((chunk = World.GetChunkFromChunkSync(chunkX - 1, chunkZ, false, true)) != null)
            {
                skylights[1] = (byte)chunk.SkyLight.getNibble((x - 1) & 0xf, y, z);
                heights[1] = chunk.HeightMap[(x - 1) & 0xf, z];
                directionChunkExist[0] = true;
            }

            // Right

            if (x < 15)
            {
                skylights[2] = (byte) SkyLight.getNibble(x + 1, y, z);
                heights[2] = HeightMap[x + 1, z];
            }
            else if ((chunk = World.GetChunkFromChunkSync(chunkX + 1, chunkZ, false, true)) != null)
            {
                skylights[2] = (byte)chunk.SkyLight.getNibble((x + 1) & 0xf, y, z);
                heights[2] = chunk.HeightMap[(x + 1) & 0xf, z];
                directionChunkExist[1] = true;
            }

            // Back

            if (z > 0)
            {
                skylights[3] = (byte) SkyLight.getNibble(x, y, z - 1);
                heights[3] = HeightMap[x, z - 1];
            }
            else if ((chunk = World.GetChunkFromChunkSync(chunkX, chunkZ - 1, false, true)) != null)
            {
                skylights[3] = (byte)chunk.SkyLight.getNibble(x, y, (z - 1) & 0xf);
                heights[3] = chunk.HeightMap[x, (z - 1) & 0xf];
                directionChunkExist[2] = true;
            }


            // Front

            if (z < 15)
            {
                skylights[4] = (byte) SkyLight.getNibble(x, y, z + 1);
                heights[4] = HeightMap[x, z + 1];
            }
            else if ((chunk = World.GetChunkFromChunkSync(chunkX, chunkZ + 1, false, true)) != null)
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


            if (HeightMap == null)
                Console.WriteLine("null: {0}, {1} {2}", chunkX, chunkZ, Thread.CurrentThread.ManagedThreadId);

            byte vertical = 0;

            byte maxHeight = heights[0];

            if (checkHeight)
            {
                if (heights[1] > maxHeight)
                    maxHeight = heights[1];

                if (heights[2] > maxHeight)
                    maxHeight = heights[2];

                if (heights[3] > maxHeight)
                    maxHeight = heights[3];

                if (heights[4] > maxHeight)
                    maxHeight = heights[4];
            }

            // Take the highest light value if we are under the highest block
            if (!checkHeight || HeightMap[x, z] > y)
            {
                if (skylights[1] > newSkylight)
                    newSkylight = skylights[1];

                if (skylights[2] > newSkylight)
                    newSkylight = skylights[2];

                if (skylights[3] > newSkylight)
                    newSkylight = skylights[3];

                if (skylights[4] > newSkylight)
                    newSkylight = skylights[4];

                if (skylights[5] > newSkylight)
                {
                    newSkylight = skylights[5];
                    vertical = 1;
                }

                if (skylights[6] > newSkylight)
                {
                    newSkylight = skylights[6];
                    vertical = 1;
                }
            }
            
            // Our light value should be the highest neighbour light value - (1 + our opacity)
            if (!checkHeight || HeightMap[x, z] > y)
            {
                byte opacity = BlockHelper.Instance(Types[x << 11 | z << 7 | y]).Opacity;

                byte toSubtract = (byte) (1 - vertical + opacity);
                newSkylight -= toSubtract;

                if (newSkylight < 0)
                    newSkylight = 0;
            }


            if (skylights[0] != newSkylight)
                SetSkyLight(x, y, z, (byte) newSkylight);
            
            // This is the light value we should spread/set to our nearby blocks
            --newSkylight;

            if (newSkylight < 0)
                newSkylight = 0;

            if (y < HeightMap[x,z])
            {
                if (skylights[5] < newSkylight)
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, (byte)(y + 1), z, checkHeight);
                    --StackSize;
                }
            }

            // Spread the light to our neighbor if the nearby has a lower skylight value
            byte neighborCoord;
            
            neighborCoord = (byte)(x - 1);

            if (x > 0)
            {
                skylights[0] = (byte)SkyLight.getNibble(x - 1, y, z);
                if (skylights[0] < newSkylight && (!checkHeight || y < heights[1]))
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(neighborCoord, y, z, checkHeight);
                    --StackSize;
                }
            }
            else if (directionChunkExist[0])
            {
                chunk = World.GetChunkFromChunkSync(chunkX - 1, chunkZ, false, true);
                skylights[0] = (byte)chunk.SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight && (!checkHeight || y < heights[1]))
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(chunk, neighborCoord & 0xf, y, z));
            }

            neighborCoord = (byte)(z - 1);

            if (z > 0)
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(x, y, neighborCoord);

                if (skylights[0] < newSkylight && (!checkHeight || y < heights[2]))
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, y, neighborCoord, checkHeight);
                    --StackSize;
                }
            }
            else if (directionChunkExist[2])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                chunk = World.GetChunkFromChunkSync(chunkX, chunkZ - 1, false, true);
                skylights[0] = (byte)chunk.SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight && (!checkHeight || y < heights[2]))
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(chunk, x, y, neighborCoord & 0xf));
            }

            neighborCoord = (byte)(x + 1);

            if (x < 15)
            {

                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(neighborCoord, y, z);

                if (skylights[0] < newSkylight && (!checkHeight || y < heights[3]))
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(neighborCoord, y, z, checkHeight);
                    --StackSize;
                }
            }
            else if (directionChunkExist[1])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                chunk = World.GetChunkFromChunkSync(chunkX + 1, chunkZ, false, true);
                skylights[0] = (byte)chunk.SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight && (!checkHeight || y < heights[3]))
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(chunk, neighborCoord & 0xf, y, z));
            }

            neighborCoord = (byte)(z + 1);

            if (z < 15)
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(x, y, neighborCoord);

                if (skylights[0] < newSkylight && (!checkHeight || y < heights[4]))
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, y, neighborCoord, checkHeight);
                    --StackSize;
                }
            }
            else if (directionChunkExist[3])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                chunk = World.GetChunkFromChunkSync(chunkX, chunkZ + 1, false, true);
                skylights[0] = (byte)chunk.SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight && y < heights[4])
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(chunk, x, y, neighborCoord & 0xf));
            } 
        }

        private static bool CanLoad(string path)
        {
            return Settings.Default.LoadFromSave && File.Exists(path);
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
                chunk.HeightMap = new byte[16, 16];
                for (int x = 0; x < 16; ++x)
                {
                    for (int z = 0; z < 16; ++z)
                    {
                        chunk.HeightMap[x, z] = (byte)zip.ReadByte();
                    }
                }
                chunk.LoadAllBlocks(zip);
            }
            catch (Exception ex)
            {
                world.Logger.Log(ex);
                return null;
            }
            if (zip != null)
                zip.Dispose();

            (BlockHelper.Instance((byte) BlockData.Blocks.Sign_Post) as BlockSignBase).LoadSignsFromDisk(chunk, world.SignsFolder);

            return chunk;

        }
        private static byte[] _readBuffer = new byte[16*16*128*3];
        private int _index;

        private void LoadAllBlocks(Stream strm)
        {
            strm.Read(_readBuffer, 0, _readBuffer.Length);
            try
            {
                for (_index = 0; _index < 16 * 16 * 128; ++_index)
                    LoadBlock(_index);
            }
            catch (Exception e)
            {
                World.Server.Logger.Log(Logger.LogLevel.Info, "Index: {0}", _index);
                World.Server.Logger.Log(Logger.LogLevel.Error, "{0}", e);
            }
        }

        private void LoadBlock(int x, int y, int z, Stream strm)
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
        }

        private void LoadBlock(int index)
        {
            int bufferIndex = index*3;
            byte type = _readBuffer[bufferIndex++];
            byte data = _readBuffer[bufferIndex++];
            byte ls = _readBuffer[bufferIndex];
            Types[index] = type;
            Data.setNibble(index, data);

            byte low = (byte)(ls & 0x0F);
            byte high = (byte)((ls & 0x0F) >> 4);

            SkyLight.setNibble(index, low);
            Light.setNibble(index, high);
        }

        private bool EnterSave()
        {
            Monitor.Enter(_SavingLock);
            if (Saving)
                return false;
            Saving = true;
            return true;
        }

        private void ExitSave()
        {
            Saving = false;
            Monitor.Exit(_SavingLock);
        }

        private void WriteBlock(int x, int y, int z, Stream strm)
        {
            strm.WriteByte(this[x, y, z]);
            strm.WriteByte(GetData(x, y, z));
            strm.WriteByte(GetDualLight(x, y, z));
        }

        private void WriteAllBlocks(Stream strm)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    for (int y = 0; y < 128; y++)
                        WriteBlock(x, y, z, strm);
                }
            }
        }

        public override void Save()
        {
            if (!EnterSave())
                return;

            Stream zip = new DeflateStream(File.Create(DataFile + ".tmp"), CompressionMode.Compress);
            try
            {
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

        public override void OnSetType(UniversalCoords coords, BlockData.Blocks value)
        {
            base.OnSetType(coords, value);
            byte blockId = (byte)value;

            if (GrowableBlocks.ContainsKey(coords.BlockPackedCoords))
            {
                short unused;

                if (!BlockHelper.IsGrowable(blockId))
                {
                    GrowableBlocks.TryRemove(coords.BlockPackedCoords, out unused);
                }
                else
                {
                    StructBlock block = new StructBlock(coords, blockId, GetData(coords), World);
                    if (!(BlockHelper.Instance(blockId) as IBlockGrowable).CanGrow(block, this))
                    {
                        GrowableBlocks.TryRemove(coords.BlockPackedCoords, out unused);
                    }
                }
            }
            else
            {
                if (BlockHelper.IsGrowable(blockId))
                {
                    StructBlock block = new StructBlock(coords, blockId, GetData(coords), World);
                    if ((BlockHelper.Instance(blockId) as IBlockGrowable).CanGrow(block, this))
                    {
                        GrowableBlocks.TryAdd(coords.BlockPackedCoords, coords.BlockPackedCoords);
                    }
                }
            }
        }

        public override void OnSetType(int blockX, int blockY, int blockZ, BlockData.Blocks value)
        {
            base.OnSetType(blockX, blockY, blockZ, value);

            byte blockId = (byte)value;
            short blockPackedCoords = (short)(blockX << 11 | blockZ << 7 | blockY);


            if (GrowableBlocks.ContainsKey(blockPackedCoords))
            {
                short unused;

                if (!BlockHelper.IsGrowable(blockId))
                {
                    GrowableBlocks.TryRemove(blockPackedCoords, out unused);
                }
                else
                {
                    byte metaData = GetData(blockX, blockY, blockZ);
                    StructBlock block = new StructBlock(UniversalCoords.FromBlock(Coords.ChunkX, Coords.ChunkZ, blockX, blockY, blockZ), blockId, metaData, World);
                    if (!(BlockHelper.Instance(blockId) as IBlockGrowable).CanGrow(block, this))
                    {
                        GrowableBlocks.TryRemove(blockPackedCoords, out unused);
                    }
                }
            }
            else
            {
                if (BlockHelper.IsGrowable(blockId))
                {
                    byte metaData = GetData(blockX, blockY, blockZ);
                    UniversalCoords blockCoords = UniversalCoords.FromBlock(Coords.ChunkX, Coords.ChunkZ, blockX, blockY,
                                                                            blockZ);
                    StructBlock block = new StructBlock(blockCoords, blockId, metaData, World);
                    if ((BlockHelper.Instance(blockId) as IBlockGrowable).CanGrow(block, this))
                        GrowableBlocks.TryAdd(blockPackedCoords, blockPackedCoords);
                }
            }
        }

        public void InitGrowableCache()
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
                if ((BlockHelper.Instance(blockId) as IBlockGrowable).CanGrow(block, this))
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
                if (BlockHelper.IsGrowable(blockId))
                {
                    metaData = GetData(blockX, blockY, blockZ);
                    block = new StructBlock(UniversalCoords.FromBlock(Coords.ChunkX, Coords.ChunkZ, blockX, blockY, blockZ), blockId, metaData, World);
                    iGrowable = (BlockHelper.Instance(blockId) as IBlockGrowable);
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

        public void ForAdjacent(UniversalCoords coords, ForEachBlock predicate)
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

        public void ForNSEW(UniversalCoords coords, ForEachBlock predicate)
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

        public void GrowCactus(UniversalCoords coords)
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

        protected override void UpdateBlocksToNearbyPlayers(object state)
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
            base.UpdateBlocksToNearbyPlayers(state);
        }
    }
}
