using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.bukkit;
using java.net;
using org.bukkit.entity;
using org.bukkit.util;
using org.bukkit.inventory;
using org.bukkit.command;
using org.bukkit.block;
using org.bukkit.material;

namespace Chraft.World
{
	public partial class WorldManager : org.bukkit.World
	{
		public Item dropItem(Location l, ItemStack @is)
		{
			// TODO: Implement
			throw new NotImplementedException();
		}

		public Item dropItemNaturally(Location l, ItemStack @is)
		{
			// TODO: Implement
			throw new NotImplementedException();
		}

		public bool generateTree(Location l, TreeType tt, BlockChangeDelegate bcd)
		{
			// TODO: Implement tree type and change delegate
			return GrowTree(l.getBlockX(), l.getBlockY(), l.getBlockZ());
		}

		public bool generateTree(Location l, TreeType tt)
		{
			// TODO: Implement tree type
			return GrowTree(l.getBlockX(), l.getBlockY(), l.getBlockZ());
		}

		public Block getBlockAt(int i1, int i2, int i3)
		{
			return new BukkitBlock
			{
				X = i1,
				Y = i2,
				Z = i3,
				World = this
			};
		}

		public Block getBlockAt(Location l)
		{
			return new BukkitBlock
			{
				X = l.getBlockX(),
				Y = l.getBlockY(),
				Z = l.getBlockZ(),
				World = this
			};
		}

		public int getBlockTypeIdAt(Location l)
		{
			return GetBlockId(l.getBlockX(), l.getBlockY(), l.getBlockZ());
		}

		public int getBlockTypeIdAt(int i1, int i2, int i3)
		{
			return GetBlockId(i1, i2, i3);
		}

		public org.bukkit.Chunk getChunkAt(Block b)
		{
			return b.getChunk();
		}

		public org.bukkit.Chunk getChunkAt(Location l)
		{
			return GetChunkFromPosition(l.getBlockX(), l.getBlockZ());
		}

		public org.bukkit.Chunk getChunkAt(int i1, int i2)
		{
			throw new NotImplementedException();
		}

		public java.util.List getEntities()
		{
			throw new NotImplementedException();
		}

		public org.bukkit.World.Environment getEnvironment()
		{
			throw new NotImplementedException();
		}

		public long getFullTime()
		{
			throw new NotImplementedException();
		}

		public int getHighestBlockYAt(Location l)
		{
			throw new NotImplementedException();
		}

		public int getHighestBlockYAt(int i1, int i2)
		{
			throw new NotImplementedException();
		}

		public long getId()
		{
			throw new NotImplementedException();
		}

		public java.util.List getLivingEntities()
		{
			throw new NotImplementedException();
		}

		public org.bukkit.Chunk[] getLoadedChunks()
		{
			throw new NotImplementedException();
		}

		public string getName()
		{
			throw new NotImplementedException();
		}

		public java.util.List getPlayers()
		{
			throw new NotImplementedException();
		}

		public Location getSpawnLocation()
		{
			throw new NotImplementedException();
		}

		public long getTime()
		{
			throw new NotImplementedException();
		}

		public bool isChunkLoaded(int i1, int i2)
		{
			throw new NotImplementedException();
		}

		public bool isChunkLoaded(org.bukkit.Chunk c)
		{
			throw new NotImplementedException();
		}

		public bool loadChunk(int i1, int i2, bool b)
		{
			throw new NotImplementedException();
		}

		public void loadChunk(int i1, int i2)
		{
			throw new NotImplementedException();
		}

		public void loadChunk(org.bukkit.Chunk c)
		{
			throw new NotImplementedException();
		}

		public bool refreshChunk(int i1, int i2)
		{
			throw new NotImplementedException();
		}

		public bool regenerateChunk(int i1, int i2)
		{
			throw new NotImplementedException();
		}

		public void save()
		{
			throw new NotImplementedException();
		}

		public void setFullTime(long l)
		{
			throw new NotImplementedException();
		}

		public bool setSpawnLocation(int i1, int i2, int i3)
		{
			throw new NotImplementedException();
		}

		public void setTime(long l)
		{
			throw new NotImplementedException();
		}

		public Arrow spawnArrow(Location l, Vector v, float f1, float f2)
		{
			throw new NotImplementedException();
		}

		public Boat spawnBoat(Location l)
		{
			throw new NotImplementedException();
		}

		public LivingEntity spawnCreature(Location l, CreatureType ct)
		{
			throw new NotImplementedException();
		}

		public Minecart spawnMinecart(Location l)
		{
			throw new NotImplementedException();
		}

		public PoweredMinecart spawnPoweredMinecart(Location l)
		{
			throw new NotImplementedException();
		}

		public StorageMinecart spawnStorageMinecart(Location l)
		{
			throw new NotImplementedException();
		}

		public bool unloadChunk(int i1, int i2, bool b1, bool b2)
		{
			throw new NotImplementedException();
		}

		public bool unloadChunk(int i1, int i2, bool b)
		{
			throw new NotImplementedException();
		}

		public bool unloadChunk(int i1, int i2)
		{
			throw new NotImplementedException();
		}

		public bool unloadChunkRequest(int i1, int i2, bool b)
		{
			throw new NotImplementedException();
		}

		public bool unloadChunkRequest(int i1, int i2)
		{
			throw new NotImplementedException();
		}
	}
}
