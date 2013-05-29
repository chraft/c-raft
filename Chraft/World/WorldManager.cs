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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Chraft.Interfaces.Containers;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.PluginSystem.World;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Plugins.Events;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Collision;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utilities.Config;
using System.IO;
using Chraft.Entity;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;
using Chraft.World.Blocks.Physics;
using Chraft.World.Weather;
using System.Threading.Tasks;
using Chraft.Utils;
using System.Collections.Generic;
using Chraft.WorldGen;
using System.Collections.Concurrent;
using Timer = System.Threading.Timer;

namespace Chraft.World
{
    public partial class WorldManager : IDisposable, IWorldManager
    {
        private IChunkGenerator _generator;
        public object ChunkGenLock = new object();
        private ChunkProvider _chunkProvider;

        public sbyte Dimension { get { return 0; } }
        public long Seed { get; private set; }
        public UniversalCoords Spawn { get; set; }
        public bool Running { get; private set; }
        public Server Server { get; private set; }
        public Logger Logger { get { return Server.Logger; } }
        public string Name { get { return ChraftConfig.DefaultWorldName; } }
        public string Folder { get { return ChraftConfig.WorldsFolder + Path.DirectorySeparatorChar + Name; } }
        public string SignsFolder { get { return Folder + Path.DirectorySeparatorChar + "Signs"; } }

        public WeatherManager Weather { get; private set; }

        private readonly ChunkSet _chunks;
        private ChunkSet Chunks { get { return _chunks; } }

        public ConcurrentQueue<ChunkLightUpdate> InitialChunkLightToRecalculate;
        public ConcurrentQueue<ChunkLightUpdate> ChunkLightToRecalculate;

        internal ConcurrentDictionary<int, BaseFallingPhysics> PhysicsBlocks;
        private Task _physicsSimulationTask;
        private Task _entityUpdateTask;
        private Task _mobSpawnerTask;

        public ConcurrentQueue<Chunk> ChunksToSave;
        public ConcurrentQueue<Chunk> ChunksToSavePostponed;

        public ConcurrentDictionary<int, ChunkEntry> PendingChunks = new ConcurrentDictionary<int, ChunkEntry>();

        private CancellationTokenSource _saveToken;

        private Task _growStuffTask;
        private Task _collectTask;
        private Task _chunkSaveTask;

        private Task _profile;

        private int _time;
        private Chunk[] _chunksCache;
        /// <summary>
        /// In units of 0.05 seconds (between 0 and 23999)
        /// </summary>
        public int Time
        {
            get { 
                int time = _time;
                
                return time; }
            set {
                _time = value;             
                }
        }

        public int AgeOfWorld { get; set; }

        private int _worldTicks;
        
        /// <summary>
        /// The current World Tick independant of the world's current Time (1 tick = 0.05 secs with a max value of 4,294,967,295 gives approx. 6.9 years of ticks)
        /// </summary>
        public int WorldTicks
        {
            get
            {
                return _worldTicks;
            }
        }

        public IServer GetServer()
        {
            return Server;
        }

        public IChunk GetChunk(UniversalCoords coords, bool create = false, bool load = false)
        {
            Chunk chunk;
            if ((chunk = Chunks[coords]) != null)
                return chunk;

            return load ? LoadChunk(coords, create) : null;
        }

        public IChunk GetChunkFromChunkSync(int chunkX, int chunkZ, bool create = false, bool load = false)
        {
            Chunk chunk;
            if ((chunk = Chunks[chunkX, chunkZ]) != null)
                return chunk;

            return load ? LoadChunk(UniversalCoords.FromChunk(chunkX, chunkZ), create) : null;
        }

        public IChunk GetChunkFromChunkAsync(int chunkX, int chunkZ, IClient client, bool create = false, bool load = false)
        {
            Chunk chunk;
            if ((chunk = Chunks[chunkX, chunkZ]) != null)
                return chunk;

            return load ? LoadChunk(UniversalCoords.FromChunk(chunkX, chunkZ), create, client as Client, false) : null;
        }

        public IChunk GetChunkFromWorld(int worldX, int worldZ, bool create = false, bool load = false)
        {
            Chunk chunk;
            if ((chunk = Chunks[worldX >> 4, worldZ >> 4]) != null)
                return chunk;

            return load ? LoadChunk(UniversalCoords.FromWorld(worldX, 0, worldZ), create) : null;
        }

        public IChunk GetChunkFromAbs(double absX, double absZ, bool create = false, bool load = false)
        {
            int worldX = (int) Math.Floor(absX);
            int worldZ = (int)Math.Floor(absZ);

            Chunk chunk;
            if ((chunk = Chunks[worldX >> 4, worldZ >> 4]) != null)
                return chunk;

            return load ? LoadChunk(UniversalCoords.FromWorld(worldX, 0, worldZ), create) : null;
        }
        
        public IEnumerable<IEntityBase> GetEntitiesWithinBoundingBoxExcludingEntity(IEntityBase entity, BoundingBox boundingBox)
        {
            return (from e in Server.GetEntitiesWithinBoundingBox(boundingBox)
                   where e != entity
                   select e);
        }
        
        public BoundingBox[] GetCollidingBoundingBoxes(EntityBase entity, BoundingBox boundingBox)
        {
            List<BoundingBox > collidingBoundingBoxes = new List<BoundingBox>();

            UniversalCoords minimumBlockXYZ = UniversalCoords.FromAbsWorld(boundingBox.Minimum.X, boundingBox.Minimum.Y, boundingBox.Minimum.Z);
            UniversalCoords maximumBlockXYZ = UniversalCoords.FromAbsWorld(boundingBox.Maximum.X + 1.0D, boundingBox.Maximum.Y + 1.0D, boundingBox.Maximum.Z + 1.0D);

            for (int x = minimumBlockXYZ.WorldX; x < maximumBlockXYZ.WorldX; x++)
            {
                for (int z = minimumBlockXYZ.WorldZ; z < maximumBlockXYZ.WorldZ; z++)
                {
                    Chunk chunk = GetChunkFromWorld(x, z) as Chunk;

                    if (chunk == null)
                        continue;

                    for (int y = minimumBlockXYZ.WorldY - 1; y < maximumBlockXYZ.WorldY; y++)
                    {
                        StructBlock block = (StructBlock)chunk.GetBlock(UniversalCoords.FromWorld(x, y, z));

                        BlockBase blockInstance = BlockHelper.Instance.CreateBlockInstance(block.Type);
                        
                        if (blockInstance != null && blockInstance.IsCollidable)
                        {
                            BoundingBox blockBox = blockInstance.GetCollisionBoundingBox(block);
                            if (blockBox.IntersectsWith(boundingBox))
                            {
                                collidingBoundingBoxes.Add(blockBox);
                            }
                        }
                    }
                }
            }
   
            foreach (var e in GetEntitiesWithinBoundingBoxExcludingEntity(entity, boundingBox.Expand(new Vector3(0.25, 0.25, 0.25))))
            {
                collidingBoundingBoxes.Add(e.BoundingBox);
                
                // TODO: determine if overridable collision boxes between two entities is necessary
                BoundingBox? collisionBox = entity.GetCollisionBox(e as EntityBase);
                if (collisionBox != null && collisionBox.Value != e.BoundingBox && collisionBox.Value.IntersectsWith(boundingBox))
                {
                    collidingBoundingBoxes.Add(collisionBox.Value);
                }
            }
            
            return collidingBoundingBoxes.ToArray();
        }
        double[] _lightBrightnessTable = new double[16];
        public WorldManager(Server server)
        {          
            _chunks = new ChunkSet();
            Server = server;
            InitialChunkLightToRecalculate = new ConcurrentQueue<ChunkLightUpdate>();
            ChunkLightToRecalculate = new ConcurrentQueue<ChunkLightUpdate>();
            ChunksToSave = new ConcurrentQueue<Chunk>();
            ChunksToSavePostponed = new ConcurrentQueue<Chunk>();
            Load();

            // Create the light brightness table
            double d = 0.0;
            for(int i = 0; i < 16; i++)
            {
                double d1 = 1.0 - i / 15.0;
                _lightBrightnessTable[i] = ((1.0 - d1) / (d1 * 3.0 + 1.0)) * (1.0 - d) + d;
            }
        }

        internal bool Load()
        {
            EnsureDirectory();

            //Event
            WorldLoadEventArgs e = new WorldLoadEventArgs(this);
            Server.PluginManager.CallEvent(Event.WorldLoad, e);
            if (e.EventCanceled) return false;
            //End Event

            _chunkProvider = new ChunkProvider(this);
            _generator = _chunkProvider.GetNewGenerator("Default", GetSeed());
            PhysicsBlocks = new ConcurrentDictionary<int, BaseFallingPhysics>();

            if (_generator == null)
            {
                Logger.Log(LogLevel.Error,
                           "No ChunkGenerator found in the Plugins folder! Add the default one from CustomGenerator project and then restart the server.");
                return false;
            }

            InitializeSpawn();
            InitializeWeather();
            Running = true;
            return true;
        }
        
        private void InitializeWeather()
        {
            Weather = new WeatherManager(this);
        }

        public int GetHeight(UniversalCoords coords)
        {
            Chunk chunk = GetChunk(coords) as Chunk;
            if (chunk == null)
                return -1;

            return chunk.HeightMap[coords.BlockX, coords.BlockZ];
        }

        public int GetHeight(int x, int z)
        {
            Chunk chunk = GetChunkFromWorld(x, z) as Chunk;

            if(chunk == null)
                return -1;
            return chunk.HeightMap[x & 0xf, z & 0xf];
        }

        public void AddChunk(IChunk iChunk)
        {
            Chunk c = (Chunk) iChunk;
            c.CreationDate = DateTime.Now;
            Chunks.Add(c);
        }

        public IChunk CreateChunk(UniversalCoords coords)
        {
            return new Chunk(this, coords);
        }

        public ConcurrentDictionary<ChunkEntry, ChunkEntry> TempPendingChunks = new ConcurrentDictionary<ChunkEntry, ChunkEntry>(); 
      
        private Chunk LoadChunk(UniversalCoords coords, bool create, Client client = null, bool sync = true)
        {
            ChunkEntry newEntry = new ChunkEntry();
            ChunkEntry entry;
            ChunkEntry removedEntry;

            entry = PendingChunks.GetOrAdd(coords.ChunkPackedCoords, newEntry);
            int state = Interlocked.CompareExchange(ref entry.State, ChunkEntry.InProgress, ChunkEntry.NotInitialized);
            Chunk chunk;
            
            if (state == ChunkEntry.NotInitialized)
            {
                int threads;
                
                Interlocked.Increment(ref entry.ThreadsWaiting);
                // The entry could have been just readded but the chunk is already initialized
                if ((chunk = Chunks[coords]) != null)
                {
                    threads = Interlocked.Decrement(ref entry.ThreadsWaiting);
                    if (threads == 0)
                        PendingChunks.TryRemove(coords.ChunkPackedCoords, out removedEntry);
                    
                    entry.ChunkRequested = chunk;
                    entry.State = ChunkEntry.Initialized;

                    NotifyChunkToClients(entry);

                    entry.ChunkLock.Set();

                    return chunk;

                }


                if ((chunk = Chunk.Load(coords, this)) != null)
                    AddChunk(chunk);
                else if (create)
                {
                    chunk = new Chunk(this, coords);
                    _generator.GenerateChunk(chunk, coords.ChunkX, coords.ChunkZ, false);
                }

                if(chunk == null)
                {
                    threads = Interlocked.Decrement(ref entry.ThreadsWaiting);

                    if (threads == 0)
                        PendingChunks.TryRemove(coords.ChunkPackedCoords, out removedEntry);

                    entry.ChunkLock.Set();

                    return chunk;
                }

                entry.ChunkRequested = chunk;
                entry.State = ChunkEntry.Initialized;           

                threads = Interlocked.Decrement(ref entry.ThreadsWaiting);

                if (threads == 0)
                    PendingChunks.TryRemove(coords.ChunkPackedCoords, out removedEntry);

                NotifyChunkToClients(entry);
                entry.ChunkLock.Set();

                if (chunk != null)
                {
                    chunk.InitGrowableCache();
                    ContainerFactory.LoadContainersFromDisk(chunk);
                }

                return chunk;
            }
           
            if (state == ChunkEntry.InProgress)
            {
                Interlocked.Increment(ref entry.ThreadsWaiting);

                if (!sync)
                    entry.Requests.Enqueue(new ClientRequest { ClientRequesting = client });
                else
                    entry.ChunkLock.WaitOne();
                

                int threads = Interlocked.Decrement(ref entry.ThreadsWaiting);
                if (threads == 0)
                    PendingChunks.TryRemove(coords.ChunkPackedCoords, out removedEntry);

                if(!sync)
                {
                    if (entry.State == ChunkEntry.Initialized)
                        NotifyChunkToClients(entry);

                    return null;
                }

                if (entry.State == ChunkEntry.Initialized)
                    chunk = Chunks[coords];
                else
                    return null;

                Debug.Assert(chunk != null, "RETURNING NULL CHUNK");
                return chunk;
            }

            chunk = Chunks[coords];

            Debug.Assert(chunk != null, "RETURNING NULL CHUNK");
            Debug.Assert(!PendingChunks.TryGetValue(coords.ChunkPackedCoords, out entry), "ENTRY INITIALIZED AND STILL LISTED");
            
            return chunk;            
        }

        private void NotifyChunkToClients(ChunkEntry entry)
        {
            int notifyState = Interlocked.CompareExchange(ref entry.NotifyStatus, ChunkEntry.Notified,
                                                          ChunkEntry.NotNotified);

            if (notifyState == ChunkEntry.Notified || entry.Requests.IsEmpty)
                return;

            Task.Factory.StartNew(() =>
            {
                Parallel.For(0, entry.Requests.Count, (i) =>
                {
                    ClientRequest req;

                    if (!entry.Requests.TryDequeue(out req))
                        return;

                    if (entry.ChunkRequested.LightToRecalculate)
                        entry.ChunkRequested.RecalculateSky();

                    req.ClientRequesting.Owner.LoadedChunks.TryUpdate(entry.ChunkRequested.Coords.ChunkPackedCoords,
                                                                      entry.ChunkRequested, null);
                    entry.ChunkRequested.AddClient(req.ClientRequesting);
                    req.ClientRequesting.SendChunk(entry.ChunkRequested);
                });
            });
        }

        private void InitializeSpawn()
        {
            Logger.LogOnOneLine(LogLevel.Info, "Initializing spawn area...", true);
            Spawn = UniversalCoords.FromWorld(ChraftConfig.SpawnX, ChraftConfig.SpawnY, ChraftConfig.SpawnZ);

            Queue<Chunk> toRecalculate = new Queue<Chunk>();           
            Chunk chunk = GetChunkFromWorld(Spawn.WorldX, Spawn.WorldZ, true, true) as Chunk;
            chunk.Persistent = true;
            toRecalculate.Enqueue(chunk);

            Spawn = UniversalCoords.FromWorld(Spawn.WorldX, chunk.HeightMap[Spawn.WorldX, Spawn.WorldZ] + 4, Spawn.WorldZ);

            int chunkX = Spawn.ChunkX;
            int chunkZ = Spawn.ChunkZ;

            
            for(int x = chunkX - 4; x < chunkX + 4; ++x)
            {
                for(int z = chunkZ - 4; z < chunkZ + 4; ++z)
                {
                    if(x == chunkX && z == chunkZ)
                        continue;

                    chunk = GetChunkFromChunkSync(x, z, true, true) as Chunk;
                    chunk.Persistent = true;
                    toRecalculate.Enqueue(chunk);
                }
            }
#if PROFILE
            Stopwatch watch = new Stopwatch();
#endif
            while(toRecalculate.Count > 0)
            {
                chunk = toRecalculate.Dequeue();

                if (chunk.LightToRecalculate)
                {
#if PROFILE              
                    watch.Reset();
                    watch.Start();
#endif
                    chunk.RecalculateHeight();
                    chunk.RecalculateSky();
#if PROFILE
                    watch.Stop();
                    Console.WriteLine("Chunk {0} - {1} skylight recalc: {2} ms", chunk.Coords.ChunkX, chunk.Coords.ChunkZ, watch.ElapsedMilliseconds);
                }
#else
                }
#endif

                AddChunk(chunk);
            }

            Logger.LogOnOneLine(LogLevel.Info, " Done\n", false);
        }

        private void CollectProc()
        {
            CheckAliveClients();
            IChunk[] chunks = GetChunks();
            foreach (Chunk c in chunks)
            {
                if (c.Persistent)
                    continue;
                if (c.GetClients().Length > 0 || (DateTime.Now - c.CreationDate) < TimeSpan.FromSeconds(20.0))
                    continue;

                c.Save();
                Chunks.Remove(c);
            }
        }

        private void CheckAliveClients()
        {
            Client[] clients = Server.GetClients() as Client[];
            Parallel.ForEach(clients, c => c.CheckAlive());
        }

        private void SaveProc(int chunkToSave, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            int count = ChunksToSave.Count;

            if (count > chunkToSave)
                count = chunkToSave;

            chunkToSave -= count;
            for (int i = 0; i < count && Running && !token.IsCancellationRequested; ++i)
            {
                Chunk chunk;
                ChunksToSave.TryDequeue(out chunk);

                if (chunk == null)
                    continue;

                /* Better to "signal" that the chunk can be queued again before saving, 
                 * we don't know which signaled changes will be saved during the save */
                Interlocked.Exchange(ref chunk.ChangesToSave, 0);

                chunk.Save();               
            }

            count = ChunksToSavePostponed.Count;

            if (count > chunkToSave)
                count = chunkToSave;

            for(int i = 0; i < count && Running && !token.IsCancellationRequested; ++i)
            {
                Chunk chunk;
                ChunksToSavePostponed.TryDequeue(out chunk);

                if (chunk == null)
                    continue;


                if (chunk.ChangesToSave > 0 && chunk.EnqueuedForSaving > chunk.LastSaveTime)
                {
                    if ((DateTime.Now - chunk.LastSaveTime) > Chunk.SaveSpan)
                    {
                        ChunksToSavePostponed.Enqueue(chunk);
                        continue;
                    }
                    /* Better to "signal" that the chunk can be queued again before saving, 
                     * we don't know which signaled changes will be saved during the save */
                    Interlocked.Exchange(ref chunk.ChangesToSave, 0);

                    chunk.Save();
                }
            }
        }

        private void EnsureDirectory()
        {
            if (!Directory.Exists(ChraftConfig.WorldsFolder))
                Directory.CreateDirectory(ChraftConfig.WorldsFolder);
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);
        }

        internal void StartSaveProc(int chunksToSave)
        {
            if (WorldTicks % 20 == 0 && !Server.NeedsFullSave && !Server.FullSaving && (!ChunksToSave.IsEmpty || !ChunksToSavePostponed.IsEmpty))
            {
                if (_chunkSaveTask == null || _chunkSaveTask.IsCompleted)
                {
                    _saveToken = new CancellationTokenSource();
                    var token = _saveToken.Token;
                    _chunkSaveTask = Task.Factory.StartNew(() => SaveProc(chunksToSave, token), token);
                }
            }
        }

        public bool IsSaving()
        {
            return _chunkSaveTask != null && _chunkSaveTask.IsCompleted;
        }

        internal void StopSave()
        {
            _saveToken.Cancel();
            if (!_chunkSaveTask.IsCompleted)
                _chunkSaveTask.Wait();
        }

        internal void WorldTick()
        {
            // Increment the world tick count
            Interlocked.Increment(ref _worldTicks);

            int time = Interlocked.Increment(ref _time);

            if (time == 24000)
            {	// A day has passed.
                // MUST interface directly with _Time to bypass the write lock, which we hold.
                _time = 0;
            }

            // Using this.WorldTick here as it is independant of this.Time. "this.Time" can be changed outside of the WorldManager.
            if (WorldTicks % 10 == 0)
            {
                // Triggered once every half second
                Task.Factory.StartNew(Server.DoPulse);
            }
           
            // Every 5 seconds
            if(WorldTicks % 100 == 0)
            {
                if(_collectTask == null || _collectTask.IsCompleted)
                {
                    _collectTask = Task.Factory.StartNew(CollectProc);
                }
            }

            // Every 10 seconds
            if (WorldTicks % 200 == 0)
            {
                if (_growStuffTask == null || _growStuffTask.IsCompleted)
                {
                    _growStuffTask = Task.Factory.StartNew(GrowProc);
                }
            }

            
            PhysicsProc();

            EntityProc();
            

            // Every 2 Ticks (100ms)
            if (WorldTicks % 2 == 0)
            {
                if (_mobSpawnerTask == null || _mobSpawnerTask.IsCompleted)
                {
                    _mobSpawnerTask = Task.Factory.StartNew(MobSpawnerProc);
                }
            }

#if PROFILE
            // Must wait at least one second between calls to perf counter
            if (WorldTicks % 20 == 0)
            {
                if(_profile == null || _profile.IsCompleted)
                {
                    _profile = Task.Factory.StartNew(Profile);
                }
            }
#endif
        }
        
#if PROFILE
        private void Profile()
        {
            if (Server.ProfileStartTime == DateTime.MinValue)
                Server.ProfileStartTime = DateTime.Now;

            using (StreamWriter writer = new StreamWriter("cpu.csv", true))
            {
                writer.WriteLine("{0};{1}", (DateTime.Now - Server.ProfileStartTime).TotalSeconds, Server.CpuPerfCounter.NextValue() / Environment.ProcessorCount);
            }
        }
#endif

        public void Dispose()
        {
            this.Running = false;
        }

        /*public byte GetBlockOrLoad(int x, int y, int z)
        {
            return GetChunkFromWorld(x, z, true, true)[x & 0xf, y, z & 0xf];
        }*/

        public IChunk[] GetChunks()
        {
            int changes = Interlocked.Exchange(ref Chunks.Changes, 0);
            if(_chunksCache == null || changes > 0)
                _chunksCache = Chunks.Values.ToArray();

            return _chunksCache;
        }

        private void GrowProc()
        {
            foreach (Chunk c in GetChunks())
            {
                c.Grow();
            }
        }

        private void PhysicsProc()
        {
            if (PhysicsBlocks.Count > 10)
            {
                Parallel.ForEach(PhysicsBlocks, (physicsBlock) =>
                {
                    physicsBlock.Value.Simulate();
                });
            }
            else
            {
                foreach (var physicsBlock in PhysicsBlocks)
                {
                    physicsBlock.Value.Simulate();
                }
            }
        }
  
        private void EntityProc()
        {
            EntityBase[] entities = Server.GetEntities() as EntityBase[];

            if (entities.Length > 20)
            {
                Parallel.ForEach(entities.Where((entity) => entity.World == this), (e) =>
                {
                    e.Update();
                });
            }
            else
            {
                foreach (EntityBase entity in entities)
                {
                    entity.Update();
                }
            }      
        }
        
        private void MobSpawnerProc()
        {
            WorldMobSpawner.SpawnMobs(this, true, this.WorldTicks % 400 == 0);
        }
  
        /// <summary>
        /// Yields each <see cref="StructBlock"/> found within the bounds of a <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public IEnumerable<IStructBlock> GetBlocksInBoundingBox(BoundingBox boundingBox)
        {
            UniversalCoords minimum =
                UniversalCoords.FromAbsWorld(
                    boundingBox.Minimum.X < 0.0 ? boundingBox.Minimum.X - 1.0 : boundingBox.Minimum.X,
                    boundingBox.Minimum.Y < 0.0 ? boundingBox.Minimum.Y - 1.0 : boundingBox.Minimum.Y,
                    boundingBox.Minimum.Z < 0.0 ? boundingBox.Minimum.Z - 1.0 : boundingBox.Minimum.Z);
            UniversalCoords maximum = UniversalCoords.FromAbsWorld(boundingBox.Maximum.X + 1.0,
                                                                   boundingBox.Maximum.Y + 1.0,
                                                                   boundingBox.Maximum.Z + 1.0);

            return GetBlocksBetweenCoords(minimum, maximum);
        }

        /// <summary>
        /// Yields each <see cref="StructBlock"/> found between two coordinates
        /// </summary>
        /// <param name="minimum">Start here</param>
        /// <param name="maximum">Stop here</param>
        /// <returns>Yields a <see cref="StructBlock"/> for each coordinate between minimum and maximum</returns>
        public IEnumerable<IStructBlock> GetBlocksBetweenCoords(UniversalCoords minimum, UniversalCoords maximum)
        {
            if (minimum.WorldX >= maximum.WorldX || minimum.WorldY >= maximum.WorldY || minimum.WorldZ >= maximum.WorldZ)
                throw new ArgumentOutOfRangeException("minimum", "minimum X, Y and Z must be less than maximum X, Y and Z");

            for (int x = minimum.WorldX; x < maximum.WorldX; x++)
            {
                for (int y = minimum.WorldY; y < maximum.WorldY; y++)
                {
                    for (int z = minimum.WorldZ; z < maximum.WorldZ; z++)
                    {
                        yield return this.GetBlock(x, y, z);
                    }
                }
            }
        }

        public IStructBlock GetBlock(UniversalCoords coords)
        {
            Chunk chunk = GetChunk(coords) as Chunk;

            if(chunk == null)
                return StructBlock.Empty;

            byte blockId = (byte)chunk.GetType(coords);
            byte blockData = chunk.GetData(coords);

            return new StructBlock(coords, blockId, blockData, this);
        }

        public IStructBlock GetBlock(int worldX, int worldY, int worldZ)
        {
            Chunk chunk = GetChunkFromWorld(worldX, worldZ) as Chunk;

            if (chunk == null)
                return StructBlock.Empty;

            byte blockId = (byte)chunk.GetType(worldX & 0xF, worldY, worldZ & 0xF);
            byte blockData = chunk.GetData(worldX & 0xF, worldY, worldZ & 0xF);

            return new StructBlock(worldX, worldY, worldZ, blockId, blockData, this);
        }
                                                            
        public byte? GetBlockId(UniversalCoords coords)
        {
            Chunk chunk = Chunks[coords];

            if (chunk != null)
                return (byte?)chunk.GetType(coords);

            return null;
        }

        public byte? GetBlockId(int worldX, int worldY, int worldZ)
        {
            Chunk chunk = Chunks[worldX >> 4, worldZ >> 4];

            if (chunk != null)
                return (byte?)chunk.GetType(worldX & 0xF, worldY, worldZ & 0xF);

            return null;
        }

        public byte? GetBlockData(UniversalCoords coords)
        {
            Chunk chunk = Chunks[coords];
            if (chunk != null)
                return chunk.GetData(coords);

            return null;
        }

        public byte? GetBlockData(int worldX, int worldY, int worldZ)
        {
            Chunk chunk = Chunks[worldX >> 4, worldZ >> 4];
            if (chunk != null)
                return chunk.GetData(worldX & 0xF, worldY, worldZ & 0xF);
            
            return null;
        }

        public double GetBlockLightBrightness(UniversalCoords coords)
        {
            byte? effectiveLight = GetEffectiveLight(coords);
            if(effectiveLight == null)
                return 0.0;

            return _lightBrightnessTable[(byte)effectiveLight];        
        }       
 
        public byte? GetFullBlockLight(UniversalCoords coords)
        {
            Chunk chunk = GetChunk(coords, false, false) as Chunk;

            if (chunk == null)
                return null;
                      
            return Math.Max(chunk.GetSkyLight(coords), chunk.GetBlockLight(coords));        
        }

        /// <summary>
        /// Get the Effective light at a coordinate
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public byte? GetEffectiveLight(UniversalCoords coords)        
        {            
            // TODO: this needs to return the effective light for a block, taking into consideration the time of day etc... see calculateSkylightSubtracted and getBlockLightValue_do                        
            return GetFullBlockLight(coords); // THIS HAS TO BE REMOVED AND CHANGED TO PROPER TIME CALCULATION TOO        
        }        
        
        public byte? GetBlockLight(UniversalCoords coords)        
        {
            Chunk chunk = Chunks[coords];
            if (chunk != null)
                return chunk.GetBlockLight(coords);

            return null;
        }

        public byte? GetBlockLight(int worldX, int worldY, int worldZ)
        {
            Chunk chunk = Chunks[worldX >> 4, worldZ >> 4];
            if (chunk != null)
                return chunk.GetBlockLight(worldX & 0xF, worldY, worldZ & 0xF);

            return null;
        }

        public byte? GetSkyLight(UniversalCoords coords)
        {
            Chunk chunk = Chunks[coords];
            if (chunk != null)
                return chunk.GetSkyLight(coords);

            return null;
        }

        public byte? GetSkyLight(int worldX, int worldY, int worldZ)
        {
            Chunk chunk = Chunks[worldX >> 4, worldZ >> 4];
            if (chunk != null)
                return chunk.GetSkyLight(worldX & 0xF, worldY, worldZ & 0xF);

            return null;
        }
  
        public IEnumerable<IEntityBase> GetEntities()
        {
            EntityBase[] entities = Server.GetEntities() as EntityBase[];
            return entities.Where((e) => e != null && e.World == this);
        }

        public IPlayer GetClosestPlayer(AbsWorldCoords coords, double radius)
        {
            Client[] clients = Server.GetAuthenticatedClients() as Client[];
            var radiusSqrd = radius * radius;
            Vector3 coordVector = coords.ToVector();
            return (from c in clients.Where(client => client.Owner.World == this)
                    let distance = coordVector.DistanceSquared(c.Owner.Position.ToVector())
                    where distance <= radiusSqrd
                    orderby distance
                    select c.Owner).FirstOrDefault();
        }

        private RayTraceHitBlock DoRayTraceBlock(UniversalCoords coords, Vector3 rayStart, Vector3 rayEnd)
        {
            Chunk chunk = GetChunk(coords) as Chunk;

            if (chunk == null)
                return null;

            byte blockType = (byte)chunk.GetType(coords); // only get the block type first to save time
            if (blockType > 0)
            {
                byte blockData = chunk.GetData(coords);

                StructBlock block = new StructBlock(coords, (byte)blockType, (byte)blockData, this);
                BlockBase blockClass = BlockHelper.Instance.CreateBlockInstance(block.Type);
                RayTraceHitBlock blockRayTrace = blockClass.RayTraceIntersection(block, rayStart, rayEnd);
                if (blockRayTrace != null)
                {
                    return blockRayTrace;
                }
            }
            return null;
        }
  
        /// <summary>
        /// Ray traces the blocks along the ray. This method takes approx 0.1ms per 50-60 metres.
        /// </summary>
        /// <returns>
        /// The first block hit
        /// </returns>
        /// <param name='rayStart'>
        /// Ray start.
        /// </param>
        /// <param name='rayEnd'>
        /// Ray end.
        /// </param>
        internal RayTraceHitBlock RayTraceBlocks(AbsWorldCoords rayStart, AbsWorldCoords rayEnd)
        {
            UniversalCoords startCoord = UniversalCoords.FromAbsWorld(rayStart);
            UniversalCoords endCoord = UniversalCoords.FromAbsWorld(rayEnd);
            
            UniversalCoords previousPoint;
            UniversalCoords currentPoint = startCoord;
            
            Vector3 rayStartVec = rayStart.ToVector();
            Vector3 rayEndVec = rayEnd.ToVector();
            
            Vector3 stepVector = (rayEndVec - rayStartVec).Normalize();
            
            bool xDirectionPositive = stepVector.X > 0;
            bool yDirectionPositive = stepVector.Y > 0;
            bool zDirectionPositive = stepVector.Z > 0;
            
            Vector3 currentVec = rayStartVec;
            previousPoint = currentPoint;
            
            RayTraceHitBlock blockTrace = null;
            int blockCheckCount = 0;
            try
            {
                // Step along the ray looking for block collisions
                while (true)
                {
                    #region Check adjacent blocks if necessary (to prevent skipping over the corner of one)
                    bool xChanged = currentPoint.WorldX - previousPoint.WorldX != 0;
                    bool yChanged = currentPoint.WorldY - previousPoint.WorldY != 0;
                    bool zChanged = currentPoint.WorldZ - previousPoint.WorldZ != 0;
                    
                    // When we change a coord, need to check which adjacent block also needs to be checked (to prevent missing blocks when jumping over their corners)
                    if (xChanged && yChanged && zChanged)
                    {
                        // -X,Y,Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(previousPoint.WorldX, currentPoint.WorldY, currentPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                        // X,-Y,Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(currentPoint.WorldX, previousPoint.WorldY, currentPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                        // X,Y,-Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(currentPoint.WorldX, currentPoint.WorldY, previousPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                        
                        // -X,Y,-Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(previousPoint.WorldX, currentPoint.WorldY, previousPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                        // -X,-Y,Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(previousPoint.WorldX, previousPoint.WorldY, currentPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                        // X,-Y,-Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(currentPoint.WorldX, previousPoint.WorldY, previousPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                    }
                    else if (xChanged && zChanged)
                    {
                        // -X,Y,Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(previousPoint.WorldX, currentPoint.WorldY, currentPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                        // X,Y,-Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(currentPoint.WorldX, currentPoint.WorldY, previousPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                    }
                    else if (xChanged && yChanged)
                    {
                        // -X,Y,Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(previousPoint.WorldX, currentPoint.WorldY, currentPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                        // X,-Y,Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(currentPoint.WorldX, previousPoint.WorldY, currentPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                    }
                    else if (zChanged && yChanged)
                    {
                        // X,Y,-Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(currentPoint.WorldX, currentPoint.WorldY, previousPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                        // X,-Y,Z
                        blockCheckCount++;
                        blockTrace = DoRayTraceBlock(UniversalCoords.FromWorld(currentPoint.WorldX, previousPoint.WorldY, currentPoint.WorldZ), rayStartVec, rayEndVec);
                        if (blockTrace != null)
                            return blockTrace;
                    }
                    #endregion
                    
                    // Check the currentPoint
                    blockCheckCount++;
                    blockTrace = DoRayTraceBlock(currentPoint, rayStartVec, rayEndVec);
                    if (blockTrace != null)
                        return blockTrace;
                    
                    if (currentPoint == endCoord)
                    {
                        //Console.WriteLine("Reach endCoord with no hits");
                        break;
                    }
                    
                    #region Get the next coordinate
                    previousPoint = currentPoint;
                    do
                    {
                        currentVec += stepVector;
                        currentPoint = UniversalCoords.FromAbsWorld(currentVec.X, currentVec.Y, currentVec.Z);
                    } while(previousPoint == currentPoint);
                    
                    // check we haven't gone past the endCoord
                    if ((xDirectionPositive && currentPoint.WorldX > endCoord.WorldX) || (!xDirectionPositive && currentPoint.WorldX < endCoord.WorldX) ||
                        (yDirectionPositive && currentPoint.WorldY > endCoord.WorldY) || (!yDirectionPositive && currentPoint.WorldY < endCoord.WorldY) ||
                        (zDirectionPositive && currentPoint.WorldZ > endCoord.WorldZ) || (!zDirectionPositive && currentPoint.WorldZ < endCoord.WorldZ))
                    {
                        //Console.WriteLine("Went past endCoord: {0}, {1}", startCoord, endCoord);
                        break;
                    }
                    #endregion
                }
            }
            finally
            {
                //Console.WriteLine("Block check count {0}", blockCheckCount);
            }
            return null;
        }
                    
        public long GetSeed()
        {
            if (ChraftConfig.WorldSeed == string.Empty)
            {
                return DateTime.Now.ToString().GetHashCode();
            }
                return ChraftConfig.WorldSeed.GetHashCode();
        }

        public void SetBlockAndData(UniversalCoords coords, byte type, byte data, bool needsUpdate = true)
        {
            Chunk chunk = GetChunk(coords) as Chunk;

            if(chunk == null)
                return;

            chunk.SetType(coords, (BlockData.Blocks)type, false);
            chunk.SetData(coords, data, false);

            if(needsUpdate)
                chunk.BlockNeedsUpdate(coords.BlockX, coords.BlockY, coords.BlockZ);
        }

        public void SetBlockAndData(int worldX, int worldY, int worldZ, byte type, byte data, bool needsUpdate = true)
        {
            Chunk chunk = GetChunkFromWorld(worldX, worldZ) as Chunk;

            if (chunk == null)
                return;

            chunk.SetType(worldX & 0xF, worldY, worldZ & 0xF, (BlockData.Blocks)type, false);
            chunk.SetData(worldX & 0xF, worldY, worldZ & 0xF, data, false);

            if (needsUpdate)
                chunk.BlockNeedsUpdate(worldX & 0xF, worldY, worldZ & 0xF);
        }

        public void SetBlockData(UniversalCoords coords, byte data, bool needsUpdate = true)
        {
            Chunk chunk = GetChunk(coords) as Chunk;

            if(chunk != null)
                chunk.SetData(coords, data, needsUpdate);
        }

        public void SetBlockData(int worldX, int worldY, int worldZ, byte data, bool needsUpdate = true)
        {
            Chunk chunk = GetChunkFromWorld(worldX, worldZ) as Chunk;

            if(chunk != null)
                chunk.SetData(worldX & 0xF, worldY, worldZ & 0xF, data, needsUpdate);
        }

        public bool ChunkExists(UniversalCoords coords)
        {
            return (Chunks[coords] != null);
        }

        public bool ChunkExists(int chunkX, int chunkZ)
        {
            return (Chunks[chunkX, chunkZ] != null);
        }

        public void RemoveChunk(IChunk c)
        {
            Chunks.Remove(c as Chunk);
        }

        internal void Update(UniversalCoords coords, bool updateClients = true)
        {
            Chunk chunk = GetChunk(coords) as Chunk;

            if (chunk == null)
                return;

            if (updateClients)
                chunk.BlockNeedsUpdate(coords.BlockX, coords.BlockY, coords.BlockZ);

            UpdatePhysics(coords);
            chunk.ForAdjacent(coords, uc => UpdatePhysics(uc));
        }

        private void UpdatePhysics(UniversalCoords coords, bool updateClients = true)
        {
            Chunk chunk = GetChunk(coords) as Chunk;

            if (chunk == null)
                return;

            BlockData.Blocks type = chunk.GetType(coords);
            UniversalCoords oneDown = UniversalCoords.FromWorld(coords.WorldX, coords.WorldY - 1, coords.WorldZ);        

            if (type == BlockData.Blocks.Water)
            {
                byte water = 8;
                chunk.ForNSEW(coords, delegate(UniversalCoords uc)
                {
                    Chunk nearbyChunk = GetChunk(uc) as Chunk;

                    if (nearbyChunk == null)
                        return;

                    BlockData.Blocks blockId = nearbyChunk.GetType(uc);
                    byte blockData;
                    if (blockId == BlockData.Blocks.Still_Water)
                        water = 0;
                    else if (blockId == BlockData.Blocks.Water && (blockData = nearbyChunk.GetData(uc)) < water)
                        water = (byte)(blockData + 1);
                });

                if (water != chunk.GetData(coords))
                {
                    if (water == 8)
                        chunk.SetBlockAndData(coords, 0, 0);
                    else
                        chunk.SetBlockAndData(coords, (byte)BlockData.Blocks.Water, water);
                    //Update(x, y, z, updateClients);
                    return;
                }
            }

            if (type == BlockData.Blocks.Air)
            {
                UniversalCoords oneUp = UniversalCoords.FromWorld(coords.WorldX, coords.WorldY + 1, coords.WorldZ);
                BlockData.Blocks blockIdOneUp = chunk.GetType(oneUp);
                if (coords.WorldY < 127 && (blockIdOneUp == BlockData.Blocks.Water || blockIdOneUp == BlockData.Blocks.Still_Water))
                {
                    chunk.SetBlockAndData(coords, (byte)BlockData.Blocks.Water, 0);
                    //Update(x, y, z, updateClients);
                    return;
                }

                if (coords.WorldY < 127 && (blockIdOneUp == BlockData.Blocks.Lava || blockIdOneUp == BlockData.Blocks.Still_Lava))
                {
                    chunk.SetBlockAndData(coords, (byte)BlockData.Blocks.Lava, 0);
                    //Update(x, y, z, updateClients);
                    return;
                }

                byte water = 8;

                chunk.ForNSEW(coords, delegate(UniversalCoords uc)
                {
                    Chunk nearbyChunk = GetChunk(uc) as Chunk;

                    if (nearbyChunk == null)
                        return;

                    BlockData.Blocks blockId = nearbyChunk.GetType(uc);
                    byte blockData;

                    if (blockId == BlockData.Blocks.Still_Water)
                        water = 0;
                    else if (blockId == BlockData.Blocks.Water && (blockData = nearbyChunk.GetData(uc)) < water)
                        water = (byte)(blockData + 1);
                });
                if (water < 8)
                {
                    chunk.SetBlockAndData(coords, (byte)BlockData.Blocks.Water, water);
                    //Update(x, y, z, updateClients);
                    return;
                }

                byte lava = 8;
                chunk.ForNSEW(coords, delegate(UniversalCoords uc)
                {
                    Chunk nearbyChunk = GetChunk(uc) as Chunk;

                    if (nearbyChunk == null)
                        return;

                    BlockData.Blocks blockId = nearbyChunk.GetType(uc);
                    byte blockData;

                    if (blockId == BlockData.Blocks.Still_Lava)
                        lava = 0;
                    else if (blockId == BlockData.Blocks.Lava && (blockData = nearbyChunk.GetData(uc)) < lava)
                        lava = (byte)(blockData + 1);
                });
                if (water < 4)
                {
                    chunk.SetBlockAndData(coords, (byte)BlockData.Blocks.Lava, lava);
                    //Update(x, y, z, updateClients);
                    return;
                }
            }
        }
    }
}

