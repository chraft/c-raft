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

namespace Chraft.World
{
	public class Chunk : ChunkBase
	{
		private static object _SavingLock = new object();
		private static volatile bool Saving = false;
		private int MaxHeight;

		public byte[,] HeightMap { get; private set; }
		public string DataFile { get { return World.Folder + "/x" + X + "_z" + Z + ".gz"; } }
		public bool Persistent { get; set; }

		internal Chunk(WorldManager world, int x, int z)
			: base(world, x, z)
		{
		}

		public void Recalculate()
		{
			RecalculateHeight();
			RecalculateSky();
			RecalculateLight();
		}

		private void RecalculateLight()
		{
			ForEach(RecalculateLight);
		}

		private void RecalculateLight(int x, int y, int z)
		{
			byte light = GetLuminence(x, y, z);
			byte sky = GetSkyLight(x, y, z);
			ForAdjacentSameChunk(x, y, z, delegate(int bx, int by, int bz)
			{
				byte opacity = GetOpacity(bx, by, bz);

				int l = GetBlockLight(bx, by, bz) - 1 - opacity;
				if (l > light)
					light = (byte)l;

				int s = GetSkyLight(bx, by, bz) - 1 - opacity;
				if (s > sky)
					sky = (byte)s;
			});

			if (sky != GetSkyLight(x, y, z) || light != GetBlockLight(x, y, z))
			{
				SetSkyLight(x, y, z, sky);
				SetBlockLight(x, y, z, light);
				ForAdjacentSameChunk(x, y, z, delegate(int bx, int by, int bz)
				{
					RecalculateLight(bx, by, bz);
				});
			}
			else
			{
				SetSkyLight(x, y, z, sky);
				SetBlockLight(x, y, z, light);
			}
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
				SetSkyLight(x, y, z, sky);
				byte opacity = GetOpacity(x, y, z);
				sky = (byte)(sky <= opacity ? 0 : sky - opacity);
			}
			while (--y > 0 && sky > 0);
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
			SetData(x, y, z, data);
			SetDualLight(x, y, z, ls);
		}

		private bool EnterSave()
		{
			lock (_SavingLock)
			{
				if (Saving)
					return false;
				Saving = true;
				return true;
			}
		}

		private void ExitSave()
		{
			Saving = false;
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
				World.Chunks.Remove(this);
			}
		}

		internal void Grow()
		{
			ForEach((x, y, z) => Grow(x, y, z));
		}

		private void Grow(int x, int y, int z)
		{
			BlockData.Blocks type = GetType(x, y, z);
			byte light = GetBlockLight(x, y, z);
			byte sky = GetSkyLight(x, y, z);

			switch (type)
			{
			case BlockData.Blocks.Grass:
				GrowGrass(x, y, z);
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
				GrowDirt(x, y, z);
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
			World.UpdateClients(X + x, y, Z + z);
		}

		private void GrowSapling(int x, int y, int z)
		{
			GrowTree(x, y, z);
			foreach (Client c in World.Server.GetNearbyPlayers(World, X + x, y, Z + z))
				c.SendBlockRegion(X + x - 3, y, Z + z - 3, 7, 7, 7);
		}

		public void GrowTree(int x, int y, int z, byte treeType = 0)
		{
			World.GrowTree(X + x, y, Z + z, treeType);
		}

        public void PlaceCactus(int x, int y, int z)
        {
            World.GrowCactus(x, y, z);
        }
		public void ForAdjacent(int x, int y, int z, ForEachBlock predicate)
		{
			predicate(X + x - 1, y, Z + z);
			predicate(X + x + 1, y, Z + z);
			predicate(X + x, y, Z + z - 1);
			predicate(X + x, y, Z + z + 1);
			if (y > 0)
				predicate(X + x, y - 1, Z + z);
			if (y < 127)
				predicate(X + x, y + 1, Z + z);
		}

		public void ForNSEW(int x, int y, int z, ForEachBlock predicate)
		{
			predicate(X + x - 1, y, Z + z);
			predicate(X + x + 1, y, Z + z);
			predicate(X + x, y, Z + z - 1);
			predicate(X + x, y, Z + z + 1);
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
            bool retval = true;
            ForNSEW(x, y, z, delegate(int bx, int by, int bz)
            {
                if (World.GetBlockId(bx, by, bz) != block)
                    retval = false;
            });
            return retval;
        }

        private void GrowCactus(int x, int y, int z)
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
                UpdateClients(x, y, z);
            }
        }

        private void GrowCrops(int x, int y, int z)
        {
            byte data = GetData(x, y, z);

            if (data == 0x07)
                return;

			if (World.Server.Rand.Next(10) == 0) // Was 200
			{
                SetData(x, y, z, ++data);
                UpdateClients(x, y, z);
            }	
        }

		private void GrowCobblestone(int x, int y, int z)
		{
			if (!IsAdjacentTo(x, y, z, (byte)BlockData.Blocks.Mossy_Cobblestone))
				return;

			if (World.Server.Rand.Next(60) == 0)
			{
				SetType(x, y, z, BlockData.Blocks.Mossy_Cobblestone);
                UpdateClients(x, y, z);
			}
		}

		private void GrowDirt(int x, int y, int z)
		{
			if (y < 127 && !IsAir(x, y + 1, z))
				return;

			if (World.Server.Rand.Next(30) == 0)
			{
				SetType(x, y, z, BlockData.Blocks.Grass);
				UpdateClients(x, y, z);
			}
		}

		private void GrowGrass(int x, int y, int z)
		{
			if (y >= 127)
				return;

			if (IsAir(x, y + 1, z))
			{
                if (World.Time % 50 == 0)
                {
                    if (World.Server.Rand.Next(Settings.Default.AnimalSpawnInterval) == 0)
                        World.SpawnAnimal(X + x, y + 1, Z + z);
                }
			}
			else if (World.Server.Rand.Next(30) != 0)
			{
				SetType(x, y, z, BlockData.Blocks.Dirt);
				UpdateClients(x, y, z);
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
                UpdateClients(x, y, z);
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
                    World.SpawnMob(X + x, y, Z + z);
            }
        }

		internal void SetWeather(WeatherState weather)
		{
			foreach (Client c in GetClients())
			{
				c.SendWeather(weather, X, Z);
			}
		}
	}
}
