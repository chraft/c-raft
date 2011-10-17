using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Chraft.Net;
using Chraft.Plugins.Events;
using Chraft.Properties;
using System.IO;
using Chraft.Entity;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Physics;
using Chraft.World.Weather;
using Chraft.Plugins.Events.Args;
using System.Threading.Tasks;
using Chraft.Utils;
using System.Collections.Generic;
using Chraft.WorldGen;
using System.Collections.Concurrent;

namespace Chraft.World
{
    public partial class WorldManager : IDisposable
    {
        private Timer GlobalTick;
        private IChunkGenerator Generator;
        public object ChunkGenLock = new object();
        private ChunkProvider _ChunkProvider;

        public sbyte Dimension { get { return 0; } }
        public long Seed { get; private set; }
        public UniversalCoords Spawn { get; set; }
        public bool Running { get; private set; }
        public Server Server { get; private set; }
        public Logger Logger { get { return Server.Logger; } }
        public string Name { get { return Settings.Default.DefaultWorldName; } }
        public string Folder { get { return Settings.Default.WorldsFolder + Path.DirectorySeparatorChar + Name; } }
        public string SignsFolder { get { return Folder + Path.DirectorySeparatorChar + "Signs"; } }

        public WeatherManager Weather { get; private set; }

        private readonly ChunkSet _Chunks;
        private ChunkSet Chunks { get { return _Chunks; } }

        public ConcurrentQueue<ChunkLightUpdate> ChunksToRecalculate;

        public ConcurrentDictionary<int, BlockBasePhysics> PhysicsBlocks;
        private Task _physicsSimulationTask;
        private Task _entityUpdateTask;

        public ConcurrentQueue<ChunkBase> ChunksToSave;

        private CancellationTokenSource _saveToken;
        public bool NeedsFullSave;
        public bool FullSaving;

        private Task _growStuffTask;
        private Task _collectTask;
        private Task _saveTask;
        private Task _profile;

        private int _Time;
        private Chunk[] _ChunksCache;
        /// <summary>
        /// In units of 0.05 seconds (between 0 and 23999)
        /// </summary>
        public int Time
        {
            get { 
                int time = _Time;
                
                return time; }
            set {
                _Time = value;             
                }
        }

        private int _WorldTicks = 0;
        
        /// <summary>
        /// The current World Tick independant of the world's current Time (1 tick = 0.05 secs with a max value of 4,294,967,295 gives approx. 6.9 years of ticks)
        /// </summary>
        public int WorldTicks
        {
            get
            {
                return _WorldTicks;
            }
        }

        public Chunk GetChunk(UniversalCoords coords, bool create, bool load, bool recalculate = true)
        {
            Chunk chunk;
            if ((chunk = Chunks[coords]) != null)
                return chunk;

            return load ? LoadChunk(coords, create, recalculate) : null;
        }

        public Chunk GetChunkFromChunk(int chunkX, int chunkZ, bool create, bool load, bool recalculate = true)
        {
            Chunk chunk;
            if ((chunk = Chunks[chunkX, chunkZ]) != null)
                return chunk;

            return load ? LoadChunk(UniversalCoords.FromChunk(chunkX, chunkZ), create, recalculate) : null;
        }

        public Chunk GetChunkFromWorld(int worldX, int worldZ, bool create, bool load, bool recalculate = true)
        {
            Chunk chunk;
            if ((chunk = Chunks[worldX >> 4, worldZ >> 4]) != null)
                return chunk;

            return load ? LoadChunk(UniversalCoords.FromWorld(worldX, 0, worldZ), create, recalculate) : null;
        }

        public Chunk GetChunkFromAbs(double absX, double absZ, bool create, bool load, bool recalculate = true)
        {
            int worldX = (int) Math.Floor(absX);
            int worldZ = (int)Math.Floor(absZ);

            Chunk chunk;
            if ((chunk = Chunks[worldX >> 4, worldZ >> 4]) != null)
                return chunk;

            return load ? LoadChunk(UniversalCoords.FromWorld(worldX, 0, worldZ), create, recalculate) : null;
        }
        
        public IEnumerable<EntityBase> GetEntitiesWithinBoundingBoxExcludingEntity(EntityBase entity, BoundingBox boundingBox)
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
                    for (int y = minimumBlockXYZ.WorldY - 1; y < maximumBlockXYZ.WorldY; y++)
                    {
                        StructBlock block = this.GetBlock(UniversalCoords.FromWorld(x, y, z));
                        
                        BlockBase blockInstance = BlockHelper.Instance(block.Type);
                        
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
                BoundingBox? collisionBox = entity.GetCollisionBox(e);
                if (collisionBox != null && collisionBox.Value != e.BoundingBox && collisionBox.Value.IntersectsWith(boundingBox))
                {
                    collidingBoundingBoxes.Add(collisionBox.Value);
                }
            }
            
            return collidingBoundingBoxes.ToArray();
        }

        public WorldManager(Server server)
        {          
            _Chunks = new ChunkSet();
            Server = server;
            ChunksToRecalculate = new ConcurrentQueue<ChunkLightUpdate>();
            ChunksToSave = new ConcurrentQueue<ChunkBase>();
            Load();
        }

        public bool Load()
        {
            EnsureDirectory();

            //Event
            WorldLoadEventArgs e = new WorldLoadEventArgs(this);
            Server.PluginManager.CallEvent(Event.WORLD_LOAD, e);
            if (e.EventCanceled) return false;
            //End Event

            _ChunkProvider = new ChunkProvider(this);
            Generator = _ChunkProvider.GetNewGenerator(GeneratorType.Custom, GetSeed());
            PhysicsBlocks = new ConcurrentDictionary<int, BlockBasePhysics>();

            InitializeSpawn();
            InitializeThreads();
            InitializeWeather();
            return true;
        }
        
        private void InitializeWeather()
        {
            Weather = new WeatherManager(this);
        }

        public int GetHeight(UniversalCoords coords)
        {
            return GetChunk(coords, false, true).HeightMap[coords.BlockX, coords.BlockZ];
        }

        public int GetHeight(int x, int z)
        {
            return GetChunkFromWorld(x, z, false, true).HeightMap[x & 0xf, z & 0xf];
        }

        public void AddChunk(Chunk c)
        {
            c.CreationDate = DateTime.Now;
            Chunks.Add(c);
        }

        private Chunk LoadChunk(UniversalCoords coords, bool create, bool recalculate)
        {
            lock (ChunkGenLock)
            {
                // We should check again since two threads can enter here, one after the other, with the same chunk to load 
                Chunk chunk;
                if ((chunk = Chunks[coords]) != null)
                    return chunk;

                chunk = new Chunk(this, coords);
                if (chunk.Load())
                    AddChunk(chunk);
                else if (create)
                    Generator.ProvideChunk(coords.ChunkX, coords.ChunkZ, chunk, recalculate);
                else
                    chunk = null;

                if(chunk != null)
                    chunk.InitGrowableCache();

                return chunk;
            }
        }

        private void InitializeThreads()
        {
            Running = true;
            GlobalTick = new Timer(GlobalTickProc, null, 50, 50);
        }

        private void InitializeSpawn()
        {
            Spawn = UniversalCoords.FromWorld(Settings.Default.SpawnX, Settings.Default.SpawnY, Settings.Default.SpawnZ);
            for (int i = 127; i > 0; i--)
            {
                if (GetBlockOrLoad(Spawn.WorldX, i, Spawn.WorldZ) != 0)
                {
                    Spawn = UniversalCoords.FromWorld(Spawn.WorldX, i + 4, Spawn.WorldZ);
                    break;
                }
            }
        }

        private void CollectProc()
        {
            CheckAliveClients();

            Chunk[] chunks = GetChunks();
            foreach (Chunk c in chunks)
            {
                if (c.Persistent)
                    continue;
                if (c.GetClients().Length > 0 || (DateTime.Now - c.CreationDate) < TimeSpan.FromSeconds(10.0))
                    continue;

                c.Save();
                Chunks.Remove(c);
            }
        }

        public void CheckAliveClients()
        {
            Parallel.ForEach(Server.GetAuthenticatedClients(), (c) => c.CheckAlive());
        }

        private void FullSave()
        {
            // Wait until the task has been canceled
            _saveTask.Wait();

            int count = ChunksToSave.Count;
            _saveTask = Task.Factory.StartNew(() => SaveProc(count, CancellationToken.None));
            NeedsFullSave = false;
            FullSaving = false;
        }

        private void SaveProc(int chunkToSave, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            int count = ChunksToSave.Count;

            if (count > chunkToSave)
                count = chunkToSave;

            for (int i = 0; i < count && Running && !token.IsCancellationRequested; ++i)
            {
                ChunkBase chunk;
                ChunksToSave.TryDequeue(out chunk);

                if (chunk == null)
                    continue;

                /* Better to "signal" that the chunk can be queued again before saving, 
                 * we don't know which signaled changes will be saved during the save */
                Interlocked.Exchange(ref chunk.ChangesToSave, 0);

                chunk.Save();
                
            }
        }

        private void EnsureDirectory()
        {
            if (!Directory.Exists(Settings.Default.WorldsFolder))
                Directory.CreateDirectory(Settings.Default.WorldsFolder);
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);
        }

        public static int ligthUpdateCounter = 0;

        private void GlobalTickProc(object state)
        {
            // Increment the world tick count
            Interlocked.Increment(ref _WorldTicks);

            int time;
            time = Interlocked.Increment(ref _Time);

            if (time == 24000)
            {	// A day has passed.
                // MUST interface directly with _Time to bypass the write lock, which we hold.
                _Time = time = 0;
            }

            // Using this.WorldTick here as it is independant of this.Time. "this.Time" can be changed outside of the WorldManager.
            if (WorldTicks % 10 == 0)
            {
                // Triggered once every half second
                Task.Factory.StartNew(Server.DoPulse);
            }

            if (NeedsFullSave)
            {
                FullSaving = true;
                if(_saveTask != null && !_saveTask.IsCompleted)
                    _saveToken.Cancel();

                Task.Factory.StartNew(FullSave);
            }
            else if(WorldTicks % 20 == 0 && !FullSaving && ChunksToSave.Count > 0)
            {
                if(_saveTask == null || _saveTask.IsCompleted)
                {
                    _saveToken = new CancellationTokenSource();
                    var token = _saveToken.Token;
                    _saveTask = Task.Factory.StartNew(() => SaveProc(20, token), token);
                }
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
   
            // Every Tick (50ms)
            if (_physicsSimulationTask == null || _physicsSimulationTask.IsCompleted)
            {
                _physicsSimulationTask = Task.Factory.StartNew(PhysicsProc);
            }

            if (_entityUpdateTask == null || _entityUpdateTask.IsCompleted)
            {
                _entityUpdateTask = Task.Factory.StartNew(EntityProc);
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
            this.GlobalTick.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public byte GetBlockOrLoad(int x, int y, int z)
        {
            return GetChunkFromWorld(x, z, true, true)[x & 0xf, y, z & 0xf];
        }

        public Chunk[] GetChunks()
        {
            int changes = Interlocked.Exchange(ref Chunks.Changes, 0);
            if(_ChunksCache == null || changes > 0)
                _ChunksCache = Chunks.Values.ToArray();

            return _ChunksCache;
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
            foreach (var physicsBlock in PhysicsBlocks)
            {
                physicsBlock.Value.Simulate();
            }
        }
  
        private void EntityProc()
        {
            Parallel.ForEach(Server.GetEntities().Where((entity) => entity.World == this), (e) =>
            {
                e.Update();
            });
        }

        public void SpawnAnimal(UniversalCoords coords)
        {
            MobType type = MobType.Giant;
            switch (Server.Rand.Next(4))
            {
                case 0: type = MobType.Cow; break;
                case 1: type = MobType.Hen; break;
                case 2: type = MobType.Pig; break;
                case 3: type = MobType.Sheep; break;
            }

            Mob mob = MobFactory.CreateMob(this, this.Server.AllocateEntity(), type);

            mob.Position = new AbsWorldCoords(new Vector3(coords.WorldX + 0.5, coords.WorldY, coords.WorldZ + 0.5));
            mob.World = this;

            mob.Hunter = true;
            mob.Hunting = false;

            //Event
            EntitySpawnEventArgs e = new EntitySpawnEventArgs(mob, mob.Position);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_SPAWN, e);
            if (e.EventCanceled) return;
            mob.Position = e.Location;
            //End Event
            
            //mob.Data // Set accessor is inaccebile?
            Server.AddEntity(mob);
        }

        public void SpawnMob(UniversalCoords coords, MobType type = MobType.Pig)
        {
            if (type == MobType.Pig) // Type has not been forced.
            {
                switch (Server.Rand.Next(4))
                {
                    case 0: type = MobType.Zombie; break;
                    case 1: type = MobType.Skeleton; break;
                    case 2: type = MobType.Creeper; break;
                    case 3: type = MobType.Spider; break; // TODO: Check space is larger than 1x2
                }
            }

            Mob mob = MobFactory.CreateMob(this, this.Server.AllocateEntity(), type);

            mob.Position = new AbsWorldCoords(new Vector3(coords.WorldX + 0.5, coords.WorldY, coords.WorldZ + 0.5));
            mob.World = this;

            mob.Hunter = true;
            mob.Hunting = false;

            //Event
            EntitySpawnEventArgs e = new EntitySpawnEventArgs(mob, mob.Position);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_SPAWN, e);
            if (e.EventCanceled) return;
            mob.Position = e.Location;
            //End Event
            
            //mob.Data // Set accessor is inaccebile?
            Server.AddEntity(mob); // TODO: Limit this in some way.
        }

        public Chunk GetBlockChunk(UniversalCoords coords)
        {
            return GetChunk(coords, false, true);
        }

        public Chunk GetBlockChunk(int worldX, int worldY, int worldZ)
        {
            return GetChunkFromWorld(worldX, worldZ, false, true);
        }
  
        public StructBlock GetBlock(UniversalCoords coords)
        {
            return new StructBlock(coords, GetBlockId(coords), GetBlockData(coords), this);
        }

        public StructBlock GetBlock(int worldX, int worldY, int worldZ)
        {
            UniversalCoords coords = UniversalCoords.FromWorld(worldX, worldY, worldZ);
            return new StructBlock(coords, GetBlockId(coords), GetBlockData(coords), this);
        }
                                                            
        public byte GetBlockId(UniversalCoords coords)
        {
            if (!ChunkExists(coords))
                return 0;
            return Chunks[coords][coords];
        }

        public byte GetBlockId(int worldX, int worldY, int worldZ)
        {
            if (!ChunkExists(worldX >> 4, worldZ >> 4))
                return 0;
            return (byte)Chunks[worldX >> 4, worldZ >> 4].GetType(worldX & 0xF, worldY, worldZ & 0xF);
        }

        public byte GetBlockData(UniversalCoords coords)
        {
            if (!ChunkExists(coords))
                return 0;
            return Chunks[coords].GetData(coords);
        }

        public byte GetBlockData(int worldX, int worldY, int worldZ)
        {
            if (!ChunkExists(worldX >> 4, worldZ >> 4))
                return 0;
            return Chunks[worldX >> 4, worldZ >> 4].GetData(worldX & 0xF, worldY, worldZ & 0xF);
        }

        public byte GetBlockLight(UniversalCoords coords)
        {
            if (!ChunkExists(coords))
                return 0;
            return Chunks[coords].GetBlockLight(coords);
        }

        public byte GetBlockLight(int worldX, int worldY, int worldZ)
        {
            if (!ChunkExists(worldX >> 4, worldZ >> 4))
                return 0;
            return Chunks[worldX >> 4, worldZ >> 4].GetBlockLight(worldX & 0xF, worldY, worldZ & 0xF);
        }

        public byte GetSkyLight(UniversalCoords coords)
        {
            if (!ChunkExists(coords))
                return 0;
            return Chunks[coords].GetSkyLight(coords);
        }

        public byte GetSkyLight(int worldX, int worldY, int worldZ)
        {
            if (!ChunkExists(worldX >> 4, worldZ >> 4))
                return 0;
            return Chunks[worldX >> 4, worldZ >> 4].GetSkyLight(worldX & 0xF, worldY, worldZ & 0xF);
        }

        public byte? GetBlockOrNull(UniversalCoords coords)
        {
            if (coords.WorldY < 0 || coords.WorldY > 127)
                return null;
            if (!ChunkExists(coords))
                return null;
            return Chunks[coords][coords];
        }

        public byte? GetBlockOrNull(int worldX, int worldY, int worldZ)
        {
            if (worldY < 0 || worldY > 127)
                return null;
            if (!ChunkExists(worldX >> 4, worldZ >> 4))
                return null;
            return Chunks[worldX >> 4, worldZ >> 4][worldX & 0xF, worldY, worldZ & 0xF];
        }
  
        public RayTraceHitBlock RayTraceBlocks(AbsWorldCoords rayStart, AbsWorldCoords rayEnd)
        {
            UniversalCoords startCoord = UniversalCoords.FromAbsWorld(rayStart);
            UniversalCoords endCoord = UniversalCoords.FromAbsWorld(rayEnd);
            
            // Interpolate along the ray looking for block collisions
            UniversalCoords previousPoint = UniversalCoords.Empty;
            UniversalCoords currentPoint = startCoord;
            
            Vector3 rayStartVec = rayStart.ToVector();
            Vector3 rayEndVec = rayEnd.ToVector();
            double step = 1.0 / rayStartVec.Distance(rayEndVec); // This makes step the correct size to check the coord of each block along the ray once - none missed
            for (double t = 0.0; t <= 1.0; t += step)
            {
                Vector3 interpolatedVec = rayStartVec.Interpolate(rayEndVec, t, false);
                
                currentPoint = UniversalCoords.FromAbsWorld(interpolatedVec.X, interpolatedVec.Y, interpolatedVec.Z);
                
                // Note: This check is not required - tests show that the step size created above will mean each block is only checked once.
                //if (currentPoint == previousPoint)
                //    continue;
                
                byte blockType = this.GetBlockId(currentPoint); // only get the block type first to save time
                if (blockType > 0)
                {
                    StructBlock block = new StructBlock(currentPoint, blockType, this.GetBlockData(currentPoint), this);
                    BlockBase blockClass = BlockHelper.Instance(block.Type);
                    RayTraceHitBlock blockRayTrace = blockClass.RayTraceIntersection(block, rayStartVec, rayEndVec);
                    if (blockRayTrace != null)
                    {
                        return blockRayTrace;
                    }
                }
                previousPoint = currentPoint;
            }
            return null;
        }
                    
        public long GetSeed()
        {
            return Settings.Default.WorldSeed.GetHashCode();
        }

        public void SetBlockAndData(UniversalCoords coords, byte type, byte data)
        {
            Chunk chunk = GetChunk(coords, false, true);
            chunk.SetType(coords, (BlockData.Blocks)type);
            chunk.SetData(coords, data, true);
        }

        public void SetBlockAndData(int worldX, int worldY, int worldZ, byte type, byte data)
        {
            Chunk chunk = GetChunkFromWorld(worldX, worldZ, false, true);
            chunk.SetType(worldX & 0xF, worldY, worldZ & 0xF, (BlockData.Blocks)type);
            chunk.SetData(worldX & 0xF, worldY, worldZ & 0xF, data, true);
        }

        public void SetBlockData(UniversalCoords coords, byte data)
        {
            GetChunk(coords, false, true).SetData(coords, data, true);
        }

        public void SetBlockData(int worldX, int worldY, int worldZ, byte data)
        {
            GetChunkFromWorld(worldX, worldZ, false, true).SetData(worldX & 0xF, worldY, worldZ & 0xF, data, true);
        }

        public bool ChunkExists(UniversalCoords coords)
        {
            return (Chunks[coords] != null);
        }

        public bool ChunkExists(int chunkX, int chunkZ)
        {
            return (Chunks[chunkX, chunkZ] != null);
        }

        public void RemoveChunk(Chunk c)
        {
            Chunks.Remove(c);
        }

        internal void Update(UniversalCoords coords, bool updateClients = true)
        {
            Chunk chunk = GetChunk(coords, false, true);
            if (updateClients)
                chunk.BlockNeedsUpdate(coords.BlockX, coords.BlockY, coords.BlockZ);

            UpdatePhysics(coords);
            chunk.ForAdjacent(coords, delegate(UniversalCoords uc)
            {
                UpdatePhysics(uc);
            });
        }

        private void UpdatePhysics(UniversalCoords coords, bool updateClients = true)
        {
            BlockData.Blocks type = (BlockData.Blocks)GetBlockId(coords);
            UniversalCoords oneDown = UniversalCoords.FromWorld(coords.WorldX, coords.WorldY - 1, coords.WorldZ);

            if (type == BlockData.Blocks.Water)
            {
                byte water = 8;
                GetChunk(coords, false, true).ForNSEW(coords, delegate(UniversalCoords uc)
                {
                    if (GetBlockId(uc) == (byte)BlockData.Blocks.Still_Water)
                        water = 0;
                    else if (GetBlockId(uc) == (byte)BlockData.Blocks.Water && GetBlockData(uc) < water)
                        water = (byte)(GetBlockData(uc) + 1);
                });
                if (water != GetBlockData(coords))
                {
                    if (water == 8)
                        SetBlockAndData(coords, 0, 0);
                    else
                        SetBlockAndData(coords, (byte)BlockData.Blocks.Water, water);
                    //Update(x, y, z, updateClients);
                    return;
                }
            }

            if (type == BlockData.Blocks.Air)
            {
                UniversalCoords oneUp = UniversalCoords.FromWorld(coords.WorldX, coords.WorldY + 1, coords.WorldZ);
                if (coords.WorldY < 127 && (GetBlockId(oneUp) == (byte)BlockData.Blocks.Water || GetBlockId(oneUp) == (byte)BlockData.Blocks.Still_Water))
                {
                    SetBlockAndData(coords, (byte)BlockData.Blocks.Water, 0);
                    //Update(x, y, z, updateClients);
                    return;
                }

                if (coords.WorldY < 127 && (GetBlockId(oneUp) == (byte)BlockData.Blocks.Lava || GetBlockId(oneUp) == (byte)BlockData.Blocks.Still_Lava))
                {
                    SetBlockAndData(coords, (byte)BlockData.Blocks.Lava, 0);
                    //Update(x, y, z, updateClients);
                    return;
                }

                byte water = 8;
                Chunk chunk = GetChunk(coords, false, true);
                chunk.ForNSEW(coords, delegate(UniversalCoords uc)
                {
                    if (GetBlockId(uc) == (byte)BlockData.Blocks.Still_Water)
                        water = 0;
                    else if (GetBlockId(uc) == (byte)BlockData.Blocks.Water && GetBlockData(uc) < water)
                        water = (byte)(GetBlockData(uc) + 1);
                });
                if (water < 8)
                {
                    SetBlockAndData(coords, (byte)BlockData.Blocks.Water, water);
                    //Update(x, y, z, updateClients);
                    return;
                }

                byte lava = 8;
                chunk.ForNSEW(coords, delegate(UniversalCoords uc)
                {
                    if (GetBlockId(uc) == (byte)BlockData.Blocks.Still_Lava)
                        lava = 0;
                    else if (GetBlockId(uc) == (byte)BlockData.Blocks.Lava && GetBlockData(uc) < lava)
                        lava = (byte)(GetBlockData(uc) + 1);
                });
                if (water < 4)
                {
                    SetBlockAndData(coords, (byte)BlockData.Blocks.Lava, lava);
                    //Update(x, y, z, updateClients);
                    return;
                }
            }
        }

        internal UniversalCoords FromFace(UniversalCoords coords, BlockFace blockFace)
        {
            int bx = coords.WorldX;
            int by = coords.WorldY;
            int bz = coords.WorldZ;

            switch (blockFace)
            {
                case BlockFace.Self:
                    break;

                case BlockFace.Up:
                    by++;
                    break;

                case BlockFace.Down:
                    by--;
                    break;

                case BlockFace.North:
                    bx--;
                    break;

                case BlockFace.South:
                    bx++;
                    break;

                case BlockFace.East:
                    bz--;
                    break;

                case BlockFace.West:
                    bz++;
                    break;

                case BlockFace.NorthEast:
                    bx--;
                    bz--;
                    break;

                case BlockFace.NorthWest:
                    bx--;
                    bz++;
                    break;

                case BlockFace.SouthEast:
                    bx++;
                    bz--;
                    break;

                case BlockFace.SouthWest:
                    bx++;
                    bz++;
                    break;
            }

            return UniversalCoords.FromWorld(bx, by, bz);
        }
    }
}

