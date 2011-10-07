using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Chraft.Net;
using Chraft.Properties;
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

        private ConcurrentDictionary<short, short> BlocksUpdating = new ConcurrentDictionary<short, short>();
        
        internal Chunk(WorldManager world, UniversalCoords coords)
            : base(world, coords)
        {
           
        }

        public void Recalculate()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            IsRecalculating = true;
            //Console.WriteLine("Recalculating for: {0}, {1}, Thread: {2}", X, Z, Thread.CurrentThread.ManagedThreadId);
            RecalculateHeight();
            RecalculateSky();
            SpreadSkyLight();

            while (World.ChunksToRecalculate.Count > 0)
            {
                ChunkLightUpdate chunkUpdate;
                World.ChunksToRecalculate.TryDequeue(out chunkUpdate);
                if (chunkUpdate != null && chunkUpdate.Chunk != null && !chunkUpdate.Chunk.Deleted)
                {
                    chunkUpdate.Chunk.StackSize = 0;
                    if (chunkUpdate.X == -1)
                        chunkUpdate.Chunk.SpreadSkyLight();
                    else
                        chunkUpdate.Chunk.SpreadSkyLightFromBlock((byte)chunkUpdate.X, (byte)chunkUpdate.Y, (byte)chunkUpdate.Z);
                } 
            }

            sw.Stop();

            //Console.WriteLine("Chunk ({0},{1}): {2}", Coords.ChunkX, Coords.ChunkZ, sw.ElapsedMilliseconds);
            //Console.WriteLine("Scheduled: {0}, {1}, Thread: {2}", X, Z, Thread.CurrentThread.ManagedThreadId);
        }

        public void SpreadSkyLight()
        {
            
            for (int x = 0; x < 16; ++x)
            {
                for (int z = 0; z < 16; ++z)
                {
                    byte y = HeightMap[x, z];
                    
                    SpreadSkyLightFromBlock((byte)x, y, (byte)z);
                }
            }
            
        }

        public void RecalculateLight()
        {
            
        }

        public void RecalculateHeight()
        {
            MaxHeight = 127;
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
            for (height = 127; height > 0 && GetOpacity(x, height - 1, z) == 0; height--) ;
            HeightMap[x, z] = (byte)height;

            if (height < MaxHeight)
                MaxHeight = height;
        }

        public void RecalculateSky()
        {
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
            int y = 127;
            do
            {
                sky -= GetOpacity(x, y, z);

                if (sky < 0)
                    sky = 0;
                SkyLight.setNibble(x, y, z, (byte)sky);
            }
            while (--y > 0 && sky > 0);
        }

        public int StackSize;

        public void SpreadSkyLightFromBlock(byte x, byte y, byte z)
        {
            if (StackSize > 200)
            {
                World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(this, x, y, z));
                Console.WriteLine("Rescheduling chunk");
                return;
            }
            BitArray directionChunkExist = new BitArray(4);
            directionChunkExist.SetAll(false);

            byte[] skylights = new byte[7]{0,0,0,0,0,0,0};

            skylights[0] = (byte)SkyLight.getNibble(x,y,z);

            int newSkylight = skylights[0];
            byte chunkX = (byte)Coords.ChunkX;
            byte chunkZ = (byte) Coords.ChunkZ;
            // Take the skylight value of our neighbor blocks
            if (x > 0)
                skylights[1] = (byte)SkyLight.getNibble((x - 1), y, z);
            else if (World.ChunkExists(chunkX - 1, chunkZ))
            {
                skylights[1] = (byte)World.GetChunkFromChunk(chunkX - 1, chunkZ, false, true).SkyLight.getNibble((x - 1) & 0xf, y, z);
                directionChunkExist[0] = true;
            }

            if (x < 15)
                skylights[2] = (byte)SkyLight.getNibble(x + 1, y, z);
            else if (World.ChunkExists(chunkX + 1, chunkZ))
            {
                skylights[2] = (byte)World.GetChunkFromChunk(chunkX + 1, chunkZ, false, true).SkyLight.getNibble((x + 1) & 0xf, y, z);
                directionChunkExist[1] = true;
            }

            if (z > 0)
                skylights[3] = (byte)SkyLight.getNibble(x, y, z - 1);
            else if (World.ChunkExists(chunkX, chunkZ - 1))
            {
                skylights[3] = (byte)World.GetChunkFromChunk(chunkX, chunkZ - 1, false, true).SkyLight.getNibble(x, y, (z - 1) & 0xf);
                directionChunkExist[2] = true;
            }

            if (z < 15)
                skylights[4] = (byte)SkyLight.getNibble(x, y, z + 1);
            else if (World.ChunkExists(chunkX, chunkZ + 1))
            {
                skylights[4] = (byte)World.GetChunkFromChunk(chunkX, chunkZ + 1, false, true).SkyLight.getNibble(x, y, (z + 1) & 0xf);
                directionChunkExist[3] = true;
            }

            skylights[5] = (byte)SkyLight.getNibble(x, y + 1, z);

            if (y > 0)
                skylights[6] = (byte)SkyLight.getNibble(x, y - 1, z);


            if (HeightMap == null)
                Console.WriteLine("null: {0}, {1} {2}", chunkX, chunkZ, Thread.CurrentThread.ManagedThreadId);

            byte vertical = 0;
            if(HeightMap[x,z] > y)
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

            if (HeightMap[x, z] <= y)
                newSkylight = 15;
            else
            {
                byte toSubtract = (byte)(1 - vertical + BlockData.Opacity[Types[x << 11 | z << 7 | y]]);
                newSkylight -= toSubtract;

                if (newSkylight < 0)
                    newSkylight = 0;
            }

            if (skylights[0] != newSkylight)
                SetSkyLight(x, y, z, (byte)newSkylight);

            --newSkylight;

            if (newSkylight < 0)
                newSkylight = 0;

            // Then spread the light to our neighbor if the has lower skylight value
            byte neighborCoord;
            
            neighborCoord = (byte)(x - 1);

            if (x > 0)
            {
                if (skylights[1] < newSkylight)
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(neighborCoord, y, z);
                    --StackSize;
                }
            }
            else if (directionChunkExist[0])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)World.GetChunkFromChunk(chunkX - 1, chunkZ, false, true).SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight)
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(World.GetChunkFromChunk(chunkX - 1, chunkZ, false, true), neighborCoord & 0xf, y, z));
            }

            neighborCoord = (byte)(z - 1);

            if (z > 0)
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(x, y, neighborCoord);

                if (skylights[0] < newSkylight)
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, y, neighborCoord);
                    --StackSize;
                }
            }
            else if (directionChunkExist[2])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)World.GetChunkFromChunk(chunkX, chunkZ - 1, false, true).SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight)
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(World.GetChunkFromChunk(chunkX, chunkZ - 1, false, true), x, y, neighborCoord & 0xf));
            }

            // Reread Skylight value since it can be changed in the meanwhile
            
            if (y > 0)
            {
                skylights[0] = (byte)SkyLight.getNibble(x, y - 1, z);
                if (skylights[0] < newSkylight)
                {
                    if (y < 50)
                        Console.WriteLine("Big hole in {0} {1} {2}", x + (chunkX * 16), y, z + (chunkZ * 16));
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, (byte)(y - 1), z);
                    --StackSize;
                }
            }

            neighborCoord = (byte)(x + 1);

            if (x < 15)
            {

                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(neighborCoord, y, z);

                if (skylights[0] < newSkylight)
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(neighborCoord, y, z);
                    --StackSize;
                }
            }
            else if (directionChunkExist[1])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)World.GetChunkFromChunk(chunkX + 1, chunkZ, false, true).SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight)
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(World.GetChunkFromChunk(chunkX + 1, chunkZ, false, true), neighborCoord & 0xf, y, z));
            }

            neighborCoord = (byte)(z + 1);

            if (z < 15)
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(x, y, neighborCoord);

                if (skylights[0] < newSkylight)
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, y, neighborCoord);
                    --StackSize;
                }
            }
            else if (directionChunkExist[3])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)World.GetChunkFromChunk(chunkX, chunkZ + 1, false, true).SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight)
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(World.GetChunkFromChunk(chunkX, chunkZ + 1, false, true), x, y, neighborCoord & 0xf));
            }

            if (y < HeightMap[x, z])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)SkyLight.getNibble(x, y + 1, z);
                if(skylights[0] < newSkylight)
                {
                    ++StackSize;
                    SpreadSkyLightFromBlock(x, (byte)(y + 1), z);
                    --StackSize;
                }
            } 
        }

        private bool CanLoad()
        {
            return Settings.Default.LoadFromSave && File.Exists(DataFile);
        }

        public bool Load()
        {
            if (!CanLoad())
                return false;

            Stream zip = null;
            Monitor.Enter(_SavingLock);
            try
            {
                zip = new DeflateStream(File.Open(DataFile, FileMode.Open), CompressionMode.Decompress);
                HeightMap = new byte[16, 16];
                for (int x = 0; x < 16; ++x)
                {
                    for (int z = 0; z < 16; ++z)
                    {
                        HeightMap[x, z] = (byte)zip.ReadByte();
                    }
                }
                LoadAllBlocks(zip);
                return true;
            }
            catch (Exception ex)
            {
                World.Logger.Log(ex);
                return false;
            }
            finally
            {
                Monitor.Exit(_SavingLock);
                if (zip != null)
                    zip.Dispose();
            }
        }

        private void LoadAllBlocks(Stream strm)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    for (int z = 0; z < 16; z++)
                        LoadBlock(x, y, z, strm);
                }
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
                for (int y = 0; y < 128; y++)
                {
                    for (int z = 0; z < 16; z++)
                        WriteBlock(x, y, z, strm);
                }
            }
        }

        public void Save()
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
                Save();
                World.RemoveChunk(this);
            }
        }

        internal void Grow()
        {
            ForEach(Grow);
        }

        private void Grow(UniversalCoords coords)
        {
            BlockData.Blocks type = GetType(coords);
            UniversalCoords oneUp = UniversalCoords.FromWorld(coords.WorldX, coords.WorldY + 1, coords.WorldZ);
            byte light = GetBlockLight(oneUp);
            byte sky = GetSkyLight(oneUp);

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

            switch (type)
            {
                case BlockData.Blocks.Cactus:
                    GrowCactus(oneUp);
                    break;

                case BlockData.Blocks.Crops:
                    GrowCrops(coords);
                    break;

                case BlockData.Blocks.Dirt:
                    GrowGrass(coords);
                    break;

                case BlockData.Blocks.Cobblestone:
                    GrowCobblestone(coords);
                    break;

                case BlockData.Blocks.Reed:
                    GrowReed(oneUp);
                    break;

                case BlockData.Blocks.Sapling:
                    GrowSapling(coords);
                    break;
            }
        }

        private void GrowSapling(UniversalCoords coords)
        {
            GrowTree(coords);
            /*foreach (Client c in World.Server.GetNearbyPlayers(World, new AbsWorldCoords(Coords.WorldX + coords.WorldX, coords.WorldY, Coords.WorldZ + coords.WorldZ)))
                c.SendBlockRegion((X << 4) + x - 3, y, (Z << 4) + z - 3, 7, 7, 7);*/
        }

        public void GrowTree(UniversalCoords coords, byte treeType = 0)
        {
            World.GrowTree(Coords.WorldX + coords.WorldX, coords.WorldY, Coords.WorldZ + coords.WorldZ, treeType);
        }

        public void PlaceCactus(UniversalCoords coords)
        {
            World.GrowCactus(coords);
        }
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

        private void GrowCrops(UniversalCoords coords)
        {
            byte data = GetData(coords);

            if (data == 0x07)
                return;

            if (World.Server.Rand.Next(10) == 0) // Was 200
            {
                SetData(coords, ++data, true);
            }
        }

        private void GrowCobblestone(UniversalCoords coords)
        {
            if (!IsAdjacentTo(coords, (byte)BlockData.Blocks.Mossy_Cobblestone))
                return;

            if (World.Server.Rand.Next(60) == 0)
            {
                SetType(coords, BlockData.Blocks.Mossy_Cobblestone);
            }
        }

        private void GrowGrass(UniversalCoords coords)
        {
            UniversalCoords oneUp = UniversalCoords.FromWorld(coords.WorldX, coords.WorldY + 1, coords.WorldZ);
            if (coords.WorldY >= 127 || !IsAir(oneUp))
                return;

            if (IsAir(oneUp))
            {
                if (World.Time % 50 == 0)
                {
                    if (World.Server.Rand.Next(Settings.Default.AnimalSpawnInterval) == 0)
                        World.SpawnAnimal(oneUp);
                }
            }
            if (World.Server.Rand.Next(30) == 0)
            {
                SetType(coords, BlockData.Blocks.Grass);
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

        private void GrowReed(UniversalCoords coords)
        {
            if (GetType(coords) == BlockData.Blocks.Reed)
                return;

            if (GetType(UniversalCoords.FromWorld(coords.WorldX, coords.WorldY - 3, coords.WorldZ)) == BlockData.Blocks.Reed)
                return;

            if (World.Server.Rand.Next(60) == 0)
            {
                SetType(coords, BlockData.Blocks.Reed);
            }
        }

        private void SpawnMob(UniversalCoords coords)
        {
            UniversalCoords oneUp = UniversalCoords.FromWorld(coords.WorldX, coords.WorldY + 1, coords.WorldZ);
            if (GetType(coords) != BlockData.Blocks.Air)
                return;

            if (GetType(oneUp) != BlockData.Blocks.Air)
                return;

            if (World.Time % 100 == 0)
            {
                if (World.Server.Rand.Next(Settings.Default.AnimalSpawnInterval) == 0)
                    World.SpawnMob(oneUp);
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

                SendPacketToAllNearbyPlayers(new BlockChangePacket
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
                SendPacketToAllNearbyPlayers(new MultiBlockChangePacket { CoordsArray = blocks, Metadata = data, Types = types, ChunkCoords = Coords});
            }
            else
            {
                SendPacketToAllNearbyPlayers(new MapChunkPacket { Chunk = this });
            }

            BlocksUpdating.Clear();
            base.UpdateBlocksToNearbyPlayers(state);
        }

        private void SendPacketToAllNearbyPlayers(Packet packet)
        {
            Dictionary<int, Client> nearbyClients = new Dictionary<int, Client>();
            int radius = Settings.Default.SightRadius;

            int chunkX = Coords.ChunkX;
            int chunkZ = Coords.ChunkZ;

            for (int x = chunkX - radius; x <= chunkX + radius; ++x)
            {
                for (int z = chunkZ - radius; z <= chunkZ + radius; ++z)
                {
                    Chunk c = World.GetChunkFromChunk(x, z, false, false);

                    if (c != null)
                    {
                        foreach (Client client in c.GetClients())
                        {
                            if (!nearbyClients.ContainsKey(client.Owner.SessionID))
                            {
                                nearbyClients.Add(client.Owner.SessionID, client);
                                client.SendPacket(packet);
                            }
                        }
                    }
                }
            }
        }
    }
}
