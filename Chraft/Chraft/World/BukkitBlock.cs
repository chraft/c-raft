using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.bukkit.block;
using org.bukkit;

namespace Chraft.World
{
	public class BukkitBlock : Block
	{
		public int X;
		public int Y;
		public int Z;
		public WorldManager World;

		public org.bukkit.block.Biome getBiome()
		{
			// TODO: Implement biome detection
			return org.bukkit.block.Biome.SEASONAL_FOREST;
		}

		public int getBlockPower()
		{
			// TODO: Implement redstone
			return 0;
		}

		public int getBlockPower(org.bukkit.block.BlockFace bf)
		{
			// TODO: Implement redstone
			return 0;
		}

		public org.bukkit.Chunk getChunk()
		{
			return World.getChunkAt(X, Z);
		}

		public byte getData()
		{
			return World.GetBlockData(X, Y, Z);
		}

		public org.bukkit.block.BlockFace getFace(Block b)
		{
			// TODO: Figure out what this does and implement it
			throw new NotImplementedException();
		}

		public Block getFace(org.bukkit.block.BlockFace bf, int i)
		{
			// TODO: Figure out what parameter "i" does and implement it
			return getFace(bf);
		}

		public Block getFace(org.bukkit.block.BlockFace bf)
		{
			// TODO: Isn't this the same as getRelative?
			int x, y, z;
			World.FromFace(X, Y, Z, bf.Convert(), out x, out y, out z);
			return new BukkitBlock
			{
				X = x,
				Y = y,
				Z = z,
				World = World
			};
		}

		public byte getLightLevel()
		{
			// HACK: Is this right?
			byte sky = World.GetSkyLight(X, Y, Z);
			byte light = World.GetBlockLight(X, Y, Z);
			return sky > light ? sky : light;
		}

		public org.bukkit.Location getLocation()
		{
			return new Location(World, X, Y, Z);
		}

		public Block getRelative(org.bukkit.block.BlockFace bf)
		{
			int x, y, z;
			World.FromFace(X, Y, Z, bf.Convert(), out x, out y, out z);
			return new BukkitBlock
			{
				X = x,
				Y = y,
				Z = z,
				World = World
			};
		}

		public Block getRelative(int i1, int i2, int i3)
		{
			return new BukkitBlock
			{
				X = X + i1,
				Y = Y + i2,
				Z = Z + i3,
				World = World
			};
		}

		public BlockState getState()
		{
			// TODO: Implement
			throw new NotImplementedException();
		}

		public Material getType()
		{
			return Material.getMaterial(getTypeId());
		}

		public int getTypeId()
		{
			return World.GetBlockId(X, Y, Z);
		}

		public org.bukkit.World getWorld()
		{
			return World;
		}

		public int getX()
		{
			return X;
		}

		public int getY()
		{
			return Y;
		}

		public int getZ()
		{
			return Z;
		}

		public bool isBlockFaceIndirectlyPowered(org.bukkit.block.BlockFace bf)
		{
			// TODO: Implement redstone
			throw new NotImplementedException();
		}

		public bool isBlockFacePowered(org.bukkit.block.BlockFace bf)
		{
			// TODO: Implement redstone
			throw new NotImplementedException();
		}

		public bool isBlockIndirectlyPowered()
		{
			// TODO: Implement redstone
			throw new NotImplementedException();
		}

		public bool isBlockPowered()
		{
			// TODO: Implement redstone
			throw new NotImplementedException();
		}

		public void setData(byte b1, bool b2)
		{
			// TODO: Figure out what b2 is for
			throw new NotImplementedException();
		}

		public void setData(byte b)
		{
			World.SetBlockData(X, Y, Z, b);
		}

		public void setType(org.bukkit.Material m)
		{
			World.SetBlockAndData(X, Y, Z, (byte)m.getId(), 0);
		}

		public bool setTypeId(int i, bool b)
		{
			// TODO: Figure out what parameter b is for
			throw new NotImplementedException();
		}

		public bool setTypeId(int i)
		{
			if (World.GetBlockId(X, Y, Z) == i)
				return false;
			World.SetBlockAndData(X, Y, Z, (byte)i, 0);
			return true;
		}

		public bool setTypeIdAndData(int i, byte b1, bool b2)
		{
			// TODO: Figure out what b2 is for
			if (World.GetBlockId(X, Y, Z) == i && World.GetBlockData(X, Y, Z) == b1)
				return false;
			World.SetBlockAndData(X, Y, Z, (byte)i, b1);
			return true;
		}
	}
}
