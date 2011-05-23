using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Chraft.Properties;
using System.IO;
using Chraft.Utils;
using Chraft.Entity;
using Chraft.World.Weather;

namespace Chraft.World
{
	public partial class WorldManager : IDisposable
	{
		private Timer GlobalTick;
		private ChunkGenerator Generator;
		private object ChunkGenLock = new object();
		private WorldChunkManager ChunkManager;

		public sbyte Dimension { get { return 0; } }
		public long Seed { get; private set; }
		public PointI Spawn { get; set; }
		public bool Running { get; private set; }
		public Server Server { get; private set; }
		public Logger Logger { get { return Server.Logger; } }
		public string Name { get { return Settings.Default.DefaultWorldName; } }
		public string Folder { get { return Settings.Default.WorldsFolder + "/" + Name; } }
		public WeatherManager Weather { get; private set; }

		private readonly ChunkSet _Chunks;
		public ChunkSet Chunks { get { return _Chunks; } }

		private volatile int _Time;
		private readonly object _TimeWriteLock = new object();
		public int Time
		{
			get { return _Time; }
			set { lock (_TimeWriteLock) _Time = value; }
		}

		public Chunk this[int x, int z]
		{
			get
			{
				if (Chunks.ContainsKey(new PointI(x, z)))
					return Chunks[x, z];
				return LoadChunk(x, z);
			}
		}

		public WorldManager(Server server)
		{
			EnsureDirectory();

			Generator = new ChunkGenerator(this, GetSeed());
			ChunkManager = new WorldChunkManager(this);
			_Chunks = new ChunkSet();
			Server = server;

			InitializeSpawn();
			InitializeThreads();
			InitializeWeather();
		}

		private void InitializeWeather()
		{
			Weather = new WeatherManager(this);
		}

		public int GetHeight(int x, int z)
		{
			if (Chunks.ContainsKey(new PointI(x, z).Chunk))
				return this[x >> 4, z >> 4].HeightMap[x & 0xf, z & 0xf];
			return 0;
		}

		private Chunk LoadChunk(int x, int z)
		{
			lock (ChunkGenLock)
			{
				Chunk chunk = new Chunk(this, x << 4, z << 4);
				if (chunk.Load())
					Chunks.Add(chunk);
				else
                    chunk = Generator.ProvideChunk(x, z);
				return chunk;
			}
		}

		private void InitializeThreads()
		{
			this.Running = true;
			GlobalTick = new Timer(GlobalTickProc, null, 50, 50);
			SaveStart();
			GrowStart();
			CollectStart();
            EntityMoverStart();
		}

		private void InitializeSpawn()
		{
			Spawn = new PointI(Settings.Default.SpawnX, Settings.Default.SpawnY, Settings.Default.SpawnZ);
			for (int i = 127; i > 0; i--)
			{
				if (GetBlockOrLoad(Spawn.X, i, Spawn.Z) != 0)
				{
					Spawn = new PointI(Spawn.X, i + 4, Spawn.Z);
					break;
				}
			}
		}

		private void CollectStart()
		{
			Thread trd = new Thread(SaveRun);
			trd.IsBackground = false;
			trd.Start();
		}

		private void CollectRun()
		{
			while (Running)
				CollectProc();
		}

		private void CollectProc()
		{
			Chunk[] chunks = GetChunks();
			foreach (Chunk c in chunks)
			{
				if (c.Persistent)
					continue;
				if (c.GetClients().Length > 0)
					continue;
				Chunks.Remove(c);
			}
		}

		private void SaveStart()
		{
			Thread trd = new Thread(SaveRun);
			trd.IsBackground = false;
			trd.Priority = ThreadPriority.Lowest;
			trd.Start();
		}

		private void SaveRun()
		{
			while (Running)
				SaveProc(true);
			SaveProc(false);
		}

		private void SaveProc(bool passive)
		{
			Chunk[] chunks = GetChunks();
			for (int i = 0; i < chunks.Length && Running; i++)
			{
				chunks[i].Save();
				if (passive)
					Thread.Sleep(5000);
			}
		}

		private void EnsureDirectory()
		{
			if (!Directory.Exists(Settings.Default.WorldsFolder))
				Directory.CreateDirectory(Settings.Default.WorldsFolder);
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);
		}

		private void GlobalTickProc(object state)
		{
			int time;
			lock (_TimeWriteLock)
			{	// Lock Time so that others cannot write to it
				time = ++_Time;
				if (time == 24000)
				{	// A day has passed.
					// MUST interface directly with _Time to bypass the write lock, which we hold.
					_Time = time = 0;
				}
			}

			if (time % 200 == 0)
			{	// Triggered once every ten seconds
				Thread thread = new Thread(Server.DoPulse);
				thread.IsBackground = true;
				thread.Start();
			}
		}

		public Chunk GetChunkFromPosition(int x, int z)
		{
			return Chunks[x, z];
		}

		public byte this[int x, int y, int z]
		{
			get
			{
				Chunk WorkChunk = GetChunkFromPosition(x, z);
				return (WorkChunk[x & 0xf, y, z & 0xf]);
			}
			set
			{
				Chunk WorkChunk = GetChunkFromPosition(x, z);
				WorkChunk[x & 0xf, y, z & 0xf] = value;
			}
		}

		public void Dispose()
		{
			this.Running = false;
			this.GlobalTick.Change(Timeout.Infinite, Timeout.Infinite);
		}

		public byte GetBlockOrLoad(int x, int y, int z)
		{
			return this[x >> 4, z >> 4][x & 0xf, y, z & 0xf];
		}

		public Chunk[] GetChunks()
		{
			return Chunks.Values.ToArray();
		}

		private void GrowStart()
		{
			Thread thread = new Thread(GrowThread);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}

		private void GrowThread()
		{
			while (Running)
			{
				Thread.Sleep(1000);
				GrowProc();
			}
		}

		private void GrowProc()
		{
			foreach (Chunk c in GetChunks())
			{
				Thread.Sleep(20);
				c.Grow();
			}
		}

        private void EntityMoverStart()
        {
            Thread thread = new Thread(MovementThread);
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        private void MovementThread()
        {
            while (Running)
            {
                Thread.Sleep(200);
                MovProc();
            }
        }

        private void MovProc()
        {
            foreach (EntityBase e in Server.GetEntities())
            {
                e.TimeInWorld++;

                if (e is Mob)
                {
                    Mob m = (Mob)e;

                    if (m.Hunter)
                        m.HuntMode();
                    else
                        m.PassiveMode();
                }
                else if (e is ItemEntity)
                {
                    byte? uBlock = GetBlockOrNull((int)e.X, (int)(e.Y - 0.4), (int)e.Z);

                    if (uBlock != null) // Ignore if item is in an unloaded chunk.
                    {
                        switch ((BlockData.Blocks)uBlock)
                        {
                            case BlockData.Blocks.Air:
                            case BlockData.Blocks.Brown_Mushroom:
                            case BlockData.Blocks.Crops:
                            case BlockData.Blocks.Ladder:
                            case BlockData.Blocks.Lever:
                            case BlockData.Blocks.Portal:
                            case BlockData.Blocks.Rails:
                            case BlockData.Blocks.Red_Mushroom:
                            case BlockData.Blocks.Red_Rose:
                            case BlockData.Blocks.Redstone_Torch:
                            case BlockData.Blocks.Redstone_Torch_On:
                            case BlockData.Blocks.Redstone_Wire:
                            case BlockData.Blocks.Reed:
                            case BlockData.Blocks.Sapling:
                            case BlockData.Blocks.Still_Water:
                            case BlockData.Blocks.Stone_Button:
                            case BlockData.Blocks.Torch:
                            case BlockData.Blocks.Water:
                            case BlockData.Blocks.Yellow_Flower:
                                e.Y -= 0.4;
                                break;

                            case BlockData.Blocks.Fire:
                            case BlockData.Blocks.Lava:
                            case BlockData.Blocks.Still_Lava:
                                Server.Entities.Remove(e);
                                break;
                        }
                    }

                    // TOOD: Water flow movement.
                }
            }
        }

		public void SpawnAnimal(int X, int Y, int Z)
		{
            MobType type = MobType.Giant;
			switch (Server.Rand.Next(4))
			{
			case 0: type = MobType.Cow; break;
			case 1: type = MobType.Hen; break;
			case 2: type = MobType.Pig; break;
			case 3: type = MobType.Sheep; break;
			}

			Mob mob = new Mob(Server, Server.AllocateEntity(), type);

            switch (type) // Assign type specific stats
            {
                case MobType.Cow: mob.Health = 10; break;
                case MobType.Hen: mob.Health = 5; break;
                case MobType.Pig: mob.Health = 10; break;
                case MobType.Sheep: mob.Health = 10; break;
                case MobType.PigZombie: mob.Health = 10; break;
                case MobType.Squid: mob.Health = 10; break;
                case MobType.Wolf: mob.Health = 10; break;
            }

            mob.X = X + 0.5;
            mob.Y = Y;
            mob.Z = Z + 0.5;
            mob.World = this;

            mob.Hunter = true;
            mob.Hunting = false;
            mob.AttackRange = 10;
            
            //mob.Data // Set accessor is inaccebile?

			lock (Server.Entities)
				Server.Entities.Add(mob);
		}

        public void SpawnMob(int X, int Y, int Z, MobType type = MobType.Pig)
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

            Mob mob = new Mob(Server, Server.AllocateEntity(), type);

            switch (type) // Assign type specific stats
            {
                case MobType.Zombie: mob.Health = 10; break;
                case MobType.Skeleton: mob.Health = 10;  break;
                case MobType.Creeper: mob.Health = 10; break;
                case MobType.Spider: mob.Health = 10; break;
                case MobType.Ghast: mob.Health = 10; break;
                case MobType.Giant: mob.Health = 10; break;
                case MobType.Slime: mob.Health = 10; break; 
            }
           
            mob.X = X + 0.5;
            mob.Y = Y;
            mob.Z = Z + 0.5;
            mob.World = this;

            mob.Hunter = true;
            mob.Hunting = false;
            mob.AttackRange = 10;

            //mob.Data // Set accessor is inaccebile?

            lock (Server.Entities)
                Server.Entities.Add(mob); // TODO: Limit this in some way.
        }

		public byte GetBlockId(int x, int y, int z)
		{
			if (!ChunkExists(x >> 4, z >> 4))
				return 0;
			return Chunks[x >> 4, z >> 4][x & 0xf, y, z & 0xf];
		}

		public byte GetBlockData(int x, int y, int z)
		{
			if (!ChunkExists(x >> 4, z >> 4))
				return 0;
			return Chunks[x >> 4, z >> 4].GetData(x & 0xf, y, z & 0xf);
		}

		public byte GetBlockLight(int x, int y, int z)
		{
			if (!ChunkExists(x >> 4, z >> 4))
				return 0;
			return Chunks[x >> 4, z >> 4].GetBlockLight(x & 0xf, y, z & 0xf);
		}

		public byte GetSkyLight(int x, int y, int z)
		{
			if (!ChunkExists(x >> 4, z >> 4))
				return 0;
			return Chunks[x >> 4, z >> 4].GetSkyLight(x & 0xf, y, z & 0xf);
		}

		public byte? GetBlockOrNull(int x, int y, int z)
		{
			if (y < 0 || y > 127)
				return null;
			if (!ChunkExists(x >> 4, z >> 4))
				return null;
			return Chunks[x >> 4, z >> 4][x & 0xf, y, z & 0xf];
		}

		public long GetSeed()
		{
			return Settings.Default.WorldSeed.GetHashCode();
		}

		public void UpdateClients(int x, int y, int z)
		{
			byte type = GetBlockId(x, y, z);
			byte data = GetBlockData(x, y, z);
			foreach (Client c in Server.GetNearbyPlayers(this, x, y, z))
				c.SendBlock(x, y, z, type, data);
		}

		public void SetBlockAndData(int x, int y, int z, byte type, byte data)
		{
			Chunk chunk = this[x >> 4, z >> 4];
			int bx = x & 0xf;
			int bz = z & 0xf;
			chunk[bx, y, bz] = type;
			chunk.SetData(bx, y, bz, data);
			UpdateClients(x, y, z);
		}

		public void SetBlockData(int x, int y, int z, byte data)
		{
			this[x >> 4, z >> 4].SetData(x & 0xf, y, z & 0xf, data);
			UpdateClients(x, y, z);
		}

		internal WorldChunkManager GetWorldChunkManager()
		{
			return ChunkManager;
		}

		public bool ChunkExists(int x, int z)
		{
			return Chunks.ContainsKey(new PointI(x, z));
		}

		internal void Update(int x, int y, int z, bool updateClients = true)
		{
			if (updateClients)
				UpdateClients(x, y, z);
			UpdatePhysics(x, y, z);
            this[x >> 4, z >> 4].ForAdjacent(x & 0xf, y, z & 0xf, delegate(int bx, int by, int bz)
			{
				UpdatePhysics(bx, by, bz);
			});
		}

		private void UpdatePhysics(int x, int y, int z, bool updateClients = true)
		{
			BlockData.Blocks type = (BlockData.Blocks)GetBlockId(x, y, z);

			if (type == BlockData.Blocks.Sand && y > 0 && GetBlockId(x, y - 1, z) == 0)
			{
				SetBlockAndData(x, y, z, 0, 0);
				SetBlockAndData(x, y - 1, z, (byte)BlockData.Blocks.Sand, 0);
				Update(x, y - 1, z, updateClients);
				return;
			}

			if (type == BlockData.Blocks.Gravel && y > 0 && GetBlockId(x, y - 1, z) == 0)
			{
				SetBlockAndData(x, y, z, 0, 0);
				SetBlockAndData(x, y - 1, z, (byte)BlockData.Blocks.Gravel, 0);
				Update(x, y - 1, z, updateClients);
				return;
			}

			if (type == BlockData.Blocks.Water)
			{
				byte water = 8;
                this[x >> 4, z >> 4].ForNSEW(x & 0xf, y, z & 0xf, delegate(int bx, int by, int bz)
				{
					if (GetBlockId(bx, by, bz) == (byte)BlockData.Blocks.Still_Water)
						water = 0;
					else if (GetBlockId(bx, by, bz) == (byte)BlockData.Blocks.Water && GetBlockData(bx, by, bz) < water)
						water = (byte)(GetBlockData(bx, by, bz) + 1);
				});
				if (water != GetBlockData(x, y, z))
				{
					if (water == 8)
						SetBlockAndData(x, y, z, 0, 0);
					else
						SetBlockAndData(x, y, z, (byte)BlockData.Blocks.Water, water);
					//Update(x, y, z, updateClients);
					return;
				}
			}

			if (type == BlockData.Blocks.Air)
			{
				if (y < 127 && (GetBlockId(x, y + 1, z) == (byte)BlockData.Blocks.Water || GetBlockId(x, y + 1, z) == (byte)BlockData.Blocks.Still_Water))
				{
					SetBlockAndData(x, y, z, (byte)BlockData.Blocks.Water, 0);
					//Update(x, y, z, updateClients);
					return;
				}

				if (y < 127 && (GetBlockId(x, y + 1, z) == (byte)BlockData.Blocks.Lava || GetBlockId(x, y + 1, z) == (byte)BlockData.Blocks.Still_Lava))
				{
					SetBlockAndData(x, y, z, (byte)BlockData.Blocks.Lava, 0);
					//Update(x, y, z, updateClients);
					return;
				}

				byte water = 8;
                this[x >> 4, z >> 4].ForNSEW(x & 0xf, y, z & 0xf, delegate(int bx, int by, int bz)
				{
					if (GetBlockId(bx, by, bz) == (byte)BlockData.Blocks.Still_Water)
						water = 0;
					else if (GetBlockId(bx, by, bz) == (byte)BlockData.Blocks.Water && GetBlockData(bx, by, bz) < water)
						water = (byte)(GetBlockData(bx, by, bz) + 1);
				});
				if (water < 8)
				{
					SetBlockAndData(x, y, z, (byte)BlockData.Blocks.Water, water);
					//Update(x, y, z, updateClients);
					return;
				}

				byte lava = 8;
                this[x >> 4, z >> 4].ForNSEW(x & 0xf, y, z & 0xf, delegate(int bx, int by, int bz)
				{
					if (GetBlockId(bx, by, bz) == (byte)BlockData.Blocks.Still_Lava)
						lava = 0;
					else if (GetBlockId(bx, by, bz) == (byte)BlockData.Blocks.Lava && GetBlockData(bx, by, bz) < lava)
						lava = (byte)(GetBlockData(bx, by, bz) + 1);
				});
				if (water < 4)
				{
					SetBlockAndData(x, y, z, (byte)BlockData.Blocks.Lava, lava);
					//Update(x, y, z, updateClients);
					return;
				}
			}
		}

		internal bool GrowTree(int x, int y, int z, byte treeType = 0)
		{
            // TODO: Expand this futher to build redwood.
			if (y > 120)
				return false;

			for (int by = y; by < y + 5; by++)
                SetBlockAndData(x, by, z, (byte)BlockData.Blocks.Log, treeType);

			for (int by = y + 2; by < y + 5; by++)
				for (int bx = x - 2; bx <= x + 2; bx++)
					for (int bz = z - 2; bz <= z + 2; bz++)
						SetLeaves(bx, by, bz);

			for (int bx = x - 1; bx <= x + 1; bx++)
				for (int bz = z - 1; bz <= z + 1; bz++)
					SetLeaves(bx, y + 5, bz);
			return true;
		}

        private void SetLeaves(int x, int y, int z, byte treeType = 0)
		{
			if (!ChunkExists(x >> 4, z >> 4) || GetBlockId(x, y, z) != 0)
				return;
            SetBlockAndData(x, y, z, (byte)BlockData.Blocks.Leaves, treeType);
		}

        internal void GrowCactus(int x, int y, int z)
        {
            if (y > 120)
				return;

            //World.Logger.Log(Logger.LogLevel.Info, "Checking Cactus at: " + (X + x) + " " + (Y + y) + " " + (Z + z));
            // TODO: Fixing this, NSEW isn't working as it's supposed to.
            for (int by = y; by < y + 3; by++)
            {
                if (!this[x >> 4, z >> 4].IsNSEWTo(x, by, z, (byte)BlockData.Blocks.Air))
                    return;
            }

            for (int by = y; by < y + 3; by++)
            {
                SetBlockAndData(x, by, z, (byte)BlockData.Blocks.Cactus, 0);
            }
        }

		internal void FromFace(int x, int y, int z, BlockFace blockFace, out int bx, out int by, out int bz)
		{
			bx = x;
			by = y;
			bz = z;

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
		}
	}
}

