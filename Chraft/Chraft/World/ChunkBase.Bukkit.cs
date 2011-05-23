using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.bukkit.block;
using org.bukkit;
using org.bukkit.entity;
using org.bukkit.inventory;
using org.bukkit.material;

namespace Chraft.World
{
	public partial class ChunkBase : org.bukkit.Chunk
	{
		public Block getBlock(int i1, int i2, int i3)
		{
			return new BukkitBlock
			{
				World = World,
				X = X + i1,
				Y = i2,
				Z = Z + i3
			};
		}

		public org.bukkit.entity.Entity[] getEntities()
		{
			return GetEntities();
		}

		public BlockState[] getTileEntities()
		{
			return GetTileEntities();
		}

		public org.bukkit.World getWorld()
		{
			return World;
		}

		public int getX()
		{
			return X;
		}

		public int getZ()
		{
			return Z;
		}
	}
}
