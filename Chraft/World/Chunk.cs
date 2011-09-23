using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.GZip;
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
        public string DataFile { get { return World.Folder + "/x" + X + "_z" + Z + ".gz"; } }
        public bool Persistent { get; set; }
        public DateTime CreationDate;

        
        private short[] BlocksUpdating = new short[20];
        

        internal Chunk(WorldManager world, int x, int z)
            : base(world, x, z)
        {
            
                /*using(StreamWriter sw  = new StreamWriter("chunkStack.log", true))
                {
                    sw.WriteLine("Instance: {0}, {1}, Thread: {2}", X, Z, Thread.CurrentThread.ManagedThreadId);

                    StackTrace stackTrace = new StackTrace();           // get call stack
                    StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

                    // write call stack method names
                    foreach (StackFrame stackFrame in stackFrames)
                    {
                        sw.WriteLine(stackFrame.GetMethod().ReflectedType.FullName + "." + stackFrame.GetMethod().Name + " line: {0}", stackFrame.GetFileLineNumber());   // write method name
                    }
                    sw.WriteLine("\r\n");
                }*/
            
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

            Console.WriteLine("Chunk ({0},{1}): {2}", X, Z, sw.ElapsedMilliseconds);
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

        private void RecalculateHeight()
        {
            MaxHeight = 127;
            HeightMap = new byte[16, 16];
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                    RecalculateHeight(x, z);
            }
        }

        private void RecalculateSky()
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    RecalculateSky(x, z);
                }
            }
        }

        private void RecalculateSky(int x, int z)
        {
            byte sky = 15;
            int y = 127;
            do
            {
                sky -= GetOpacity(x, y, z);
                SkyLight.setNibble(x, y, z, sky);
            }
            while (--y > 0 && sky > 0);
        }

        public int StackSize;

        public void SpreadSkyLightFromBlock(byte x, byte y, byte z)
        {
            /*if (StackSize > 200)
            {
                World.ScheduleSkyLightUpdate(new ChunkLightUpdate(this, x, y, z));
                Console.WriteLine("Rescheduling chunk");
                return;
            }*/
            BitArray directionChunkExist = new BitArray(4);
            directionChunkExist.SetAll(false);

            byte[] skylights = new byte[7]{0,0,0,0,0,0,0};

            skylights[0] = (byte)SkyLight.getNibble(x,y,z);

            int newSkylight = skylights[0];
            
            // Take the skylight value of our neighbor blocks
            if (x > 0)
                skylights[1] = (byte)SkyLight.getNibble((x - 1), y, z);
            else if (World.ChunkExists(X - 1, Z))
            {
                skylights[1] = (byte)World[X - 1, Z, false, true].SkyLight.getNibble((x - 1) & 0xf, y, z);
                directionChunkExist[0] = true;
            }

            if (x < 15)
                skylights[2] = (byte)SkyLight.getNibble(x + 1, y, z);
            else if (World.ChunkExists(X + 1, Z))
            {
                skylights[2] = (byte)World[X + 1, Z, false, true].SkyLight.getNibble((x + 1) & 0xf, y, z);
                directionChunkExist[1] = true;
            }

            if (z > 0)
                skylights[3] = (byte)SkyLight.getNibble(x, y, z - 1);
            else if (World.ChunkExists(X, Z - 1))
            {
                skylights[3] = (byte)World[X, Z - 1, false, true].SkyLight.getNibble(x, y, (z - 1) & 0xf);
                directionChunkExist[2] = true;
            }

            if (z < 15)
                skylights[4] = (byte)SkyLight.getNibble(x, y, z + 1);
            else if (World.ChunkExists(X, Z + 1))
            {
                skylights[4] = (byte)World[X, Z + 1, false, true].SkyLight.getNibble(x, y, (z + 1) & 0xf);
                directionChunkExist[3] = true;
            }

            skylights[5] = (byte)SkyLight.getNibble(x, y + 1, z);

            if (y > 0)
                skylights[6] = (byte)SkyLight.getNibble(x, y - 1, z);


            if (HeightMap == null)
                Console.WriteLine("null: {0}, {1} {2}", X, Z,Thread.CurrentThread.ManagedThreadId);
                
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
                    newSkylight = skylights[5];
                    
                if (skylights[6] > newSkylight)
                    newSkylight = skylights[6]; 
            }

            if (HeightMap[x, z] <= y)
                newSkylight = 15;
            else
            {
                byte toSubtract = (byte)(1 + BlockData.Opacity[Types[x << 11 | z << 7 | y]]);
                newSkylight -= toSubtract;

                if (newSkylight < 0)
                    newSkylight = 0;
            }

            if (skylights[0] != newSkylight)
                SetSkyLight(x, y, z, (byte)newSkylight);

            --newSkylight;

            if (newSkylight < 0)
                newSkylight = 0;

            if (y > 0 && skylights[6] < newSkylight)
            {
                ++StackSize;
                SpreadSkyLightFromBlock(x, (byte)(y - 1), z);
                --StackSize;
            }

            byte neighborCoord;
            // Then spread the light to our neighbor if the has lower skylight value
            neighborCoord = (byte)(x - 1);

            if (x > 0)
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
            else if (directionChunkExist[0])
            {
                // Reread Skylight value since it can be changed in the meanwhile
                skylights[0] = (byte)World[X - 1, Z, false, true].SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight)
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(World[X - 1, Z, false, true], neighborCoord & 0xf, y, z));
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
                skylights[0] = (byte)World[X + 1, Z, false, true].SkyLight.getNibble(neighborCoord & 0xf, y, z);

                if (skylights[0] < newSkylight)
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(World[X + 1, Z, false, true], neighborCoord & 0xf, y, z));
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
                skylights[0] = (byte)World[X, Z - 1, false, true].SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight)
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(World[X, Z - 1, false, true], x, y, neighborCoord & 0xf));
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
                skylights[0] = (byte)World[X, Z + 1, false, true].SkyLight.getNibble(x, y, neighborCoord & 0xf);
                if (skylights[0] < newSkylight)
                    World.ChunksToRecalculate.Enqueue(new ChunkLightUpdate(World[X, Z + 1, false, true], x, y, neighborCoord & 0xf));
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

        private void RecalculateHeight(int x, int z)
        {
            int height;
            for (height = 127; height > 0 && GetOpacity(x, height - 1, z) == 0; height--) ;
            HeightMap[x, z] = (byte)height;
            if (height < MaxHeight)
                MaxHeight = height;
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
                for (int x = 0; x < 16; ++x)
                {
                    for (int z = 0; z < 16; ++z)
                    {
                        HeightMap = new byte[16,16];
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
                Entities.Add(client);
        }

        internal void RemoveClient(Client client)
        {
            lock (Clients)
                Clients.Remove(client);
            lock (Entities)
                Entities.Remove(client);

            if (Clients.Count == 0 && !Persistent)
            {
                Save();
                World.RemoveChunk(this);
            }
        }

        internal void Grow()
        {
            //ForEach((x, y, z) => Grow(x, y, z));
        }

        private void Grow(int x, int y, int z)
        {
            BlockData.Blocks type = GetType(x, y, z);
            byte light = GetBlockLight(x, y + 1, z);
            byte sky = GetSkyLight(x, y + 1, z);

            switch (type)
            {
                case BlockData.Blocks.Grass:
                    GrowDirt(x, y, z);
                    break;
            }

            if (light < 7 && sky < 7)
            {
                SpawnMob(x, y + 1, z);
                return;
            }

            switch (type)
            {
                case BlockData.Blocks.Cactus:
                    GrowCactus(x, y + 1, z);
                    break;

                case BlockData.Blocks.Crops:
                    GrowCrops(x, y, z);
                    break;

                case BlockData.Blocks.Dirt:
                    GrowGrass(x, y, z);
                    break;

                case BlockData.Blocks.Cobblestone:
                    GrowCobblestone(x, y, z);
                    break;

                case BlockData.Blocks.Reed:
                    GrowReed(x, y + 1, z);
                    break;

                case BlockData.Blocks.Sapling:
                    GrowSapling(x, y, z);
                    break;
            }
        }

        private void UpdateClients(int x, int y, int z)
        {
            World.UpdateClients((X << 4) + x, y, (Z << 4) + z);
        }

        private void GrowSapling(int x, int y, int z)
        {
            GrowTree(x, y, z);
            foreach (Client c in World.Server.GetNearbyPlayers(World, (X << 4) + x, y, (Z << 4) + z))
                c.SendBlockRegion((X << 4) + x - 3, y, (Z << 4) + z - 3, 7, 7, 7);
        }

        public void GrowTree(int x, int y, int z, byte treeType = 0)
        {
            World.GrowTree((X << 4) + x, y, (Z << 4) + z, treeType);
        }

        public void PlaceCactus(int x, int y, int z)
        {
            World.GrowCactus(x, y, z);
        }
        public void ForAdjacent(int x, int y, int z, ForEachBlock predicate)
        {
            int chunkWorldX = (X << 4);
            int chunkWorldZ = (Z << 4);
            predicate(chunkWorldX + x - 1, y, chunkWorldZ + z);
            predicate(chunkWorldX + x + 1, y, chunkWorldZ + z);
            predicate(chunkWorldX + x, y, chunkWorldZ + z - 1);
            predicate(chunkWorldX + x, y, chunkWorldZ + z + 1);
            if (y > 0)
                predicate(chunkWorldX + x, y - 1, chunkWorldZ + z);
            if (y < 127)
                predicate(chunkWorldX + x, y + 1, chunkWorldZ + z);
        }

        public void ForNSEW(int x, int y, int z, ForEachBlock predicate)
        {
            int chunkWorldX = (X << 4);
            int chunkWorldZ = (Z << 4);
            predicate(chunkWorldX + x - 1, y, chunkWorldZ + z);
            predicate(chunkWorldX + x + 1, y, chunkWorldZ + z);
            predicate(chunkWorldX + x, y, chunkWorldZ + z - 1);
            predicate(chunkWorldX + x, y, chunkWorldZ + z + 1);
        }

        public bool IsAdjacentTo(int x, int y, int z, byte block)
        {
            bool retval = false;
            ForAdjacent(x, y, z, delegate(int bx, int by, int bz)
            {
                retval = retval || World.GetBlockId(bx, by, bz) == block;
            });
            return retval;
        }

        public bool IsNSEWTo(int x, int y, int z, byte block)
        {
            bool retval = false;
            ForNSEW(x, y, z, delegate(int bx, int by, int bz)
            {
                if (World.GetBlockId(bx, by, bz) == block)
                    retval = true;
            });
            return retval;
        }

        public void GrowCactus(int x, int y, int z)
        {
            if ((BlockData.Blocks)GetType(x, y, z) == BlockData.Blocks.Cactus)
                return;

            if ((BlockData.Blocks)GetType(x, y - 3, z) == BlockData.Blocks.Cactus)
                return;

            if (!IsNSEWTo(x, y, z, (byte)BlockData.Blocks.Air))
                return;

            if (World.Server.Rand.Next(60) == 0)
            {
                SetType(x, y, z, BlockData.Blocks.Cactus);
            }
        }

        private void GrowCrops(int x, int y, int z)
        {
            byte data = GetData(x, y, z);

            if (data == 0x07)
                return;

            if (World.Server.Rand.Next(10) == 0) // Was 200
            {
                SetData(x, y, z, ++data, true);
            }
        }

        private void GrowCobblestone(int x, int y, int z)
        {
            if (!IsAdjacentTo(x, y, z, (byte)BlockData.Blocks.Mossy_Cobblestone))
                return;

            if (World.Server.Rand.Next(60) == 0)
            {
                SetType(x, y, z, BlockData.Blocks.Mossy_Cobblestone);
            }
        }

        private void GrowGrass(int x, int y, int z)
        {
            if (y >= 127 || !IsAir(x, y + 1, z))
                return;

            if (IsAir(x, y + 1, z))
            {
                if (World.Time % 50 == 0)
                {
                    if (World.Server.Rand.Next(Settings.Default.AnimalSpawnInterval) == 0)
                        World.SpawnAnimal((X << 4) + x, y + 1, (Z << 4) + z);
                }
            }
            if (World.Server.Rand.Next(30) == 0)
            {
                SetType(x, y, z, BlockData.Blocks.Grass);
            }
        }

        private void GrowDirt(int x, int y, int z)
        {
            if (y >= 127 || IsAir(x, y + 1, z))
                return;

            if (World.Server.Rand.Next(30) != 0)
            {
                SetType(x, y, z, BlockData.Blocks.Dirt);
            }
        }

        private void GrowReed(int x, int y, int z)
        {
            if ((BlockData.Blocks)GetType(x, y, z) == BlockData.Blocks.Reed)
                return;

            if ((BlockData.Blocks)GetType(x, y - 3, z) == BlockData.Blocks.Reed)
                return;

            if (World.Server.Rand.Next(60) == 0)
            {
                SetType(x, y, z, BlockData.Blocks.Reed);
            }
        }

        private void SpawnMob(int x, int y, int z)
        {
            if ((BlockData.Blocks)GetType(x, y, z) != BlockData.Blocks.Air)
                return;

            if ((BlockData.Blocks)GetType(x, y + 1, z) != BlockData.Blocks.Air)
                return;

            if (World.Time % 100 == 0)
            {
                if (World.Server.Rand.Next(Settings.Default.AnimalSpawnInterval) == 0)
                    World.SpawnMob((X << 4) + x, y, (Z << 4) + z);
            }
        }

        internal void SetWeather(WeatherState weather)
        {
            foreach (Client c in GetClients())
            {
                c.SendWeather(weather, (X << 4), (Z << 4));
            }
        }

        protected override void UpdateBlocksToNearbyPlayers(object state)
        {
            BlocksUpdateLock.EnterWriteLock();
            int num = Interlocked.Exchange(ref NumBlocksToUpdate, 0);
            short[] temp = BlocksToBeUpdated;
            BlocksToBeUpdated = BlocksUpdating;
            BlocksUpdateLock.ExitWriteLock();

            BlocksUpdating = temp;

            if (num == 1)
            {
                int index = BlocksUpdating[0];
                int worldX = (X << 4) + (index >> 12 & 0xf);
                int worldY = (index & 0xff);
                int worldZ = (Z << 4) + (index >> 8 & 0xf);
                byte blockId = World.GetBlockId(worldX, worldY, worldZ);
                byte data = World.GetBlockData(worldX, worldY, worldZ);

                SendPacketToAllNearbyPlayers(new BlockChangePacket { X = worldX, Y = (sbyte)worldY, Z = worldZ, Data = data, Type = blockId });
            }
            else if (num < 20)
            {
                sbyte[] data = new sbyte[num];
                sbyte[] types = new sbyte[num];

                for (int i = 0; i < num; ++i)
                {
                    int index = BlocksUpdating[i];
                    int worldX = (X << 4) + (index >> 12 & 0xf);
                    int worldY = (index & 0xff);
                    int worldZ = (Z << 4) + (index >> 8 & 0xf);

                    
                }
                SendPacketToAllNearbyPlayers(new MultiBlockChangePacket { Coords = BlocksUpdating, Metadata = data, Types = types, X = this.X, Z = this.Z});
            }
            else
            {
                SendPacketToAllNearbyPlayers(new MapChunkPacket { Chunk = this });
            }
            base.UpdateBlocksToNearbyPlayers(state);
        }

        private void SendPacketToAllNearbyPlayers(Packet packet)
        {
            Dictionary<int, Client> nearbyClients = new Dictionary<int, Client>();
            int radius = Settings.Default.SightRadius;
            for (int x = X - radius; x <= X + radius; ++x)
            {
                for (int z = Z - radius; z <= Z + radius; ++z)
                {
                    Chunk c = World[x, z, false, false];

                    if (c != null)
                    {
                        foreach (Client client in c.GetClients())
                        {
                            if (!nearbyClients.ContainsKey(client.SessionID))
                                nearbyClients.Add(client.SessionID, client);
                        }
                    }
                }
            }

            foreach (Client c in nearbyClients.Values)
                c.PacketHandler.SendPacket(packet);
        }
    }
}
