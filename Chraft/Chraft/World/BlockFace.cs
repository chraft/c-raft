using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.World
{
	public enum BlockFace : sbyte
	{
		Self = -1,
		Held = -1,
		Down = 0,
		Up = 1,
		East = 2,
		West = 3,
		North = 4,
		South = 5,
		NorthEast = 6,
		NorthWest = 7,
		SouthEast = 8,
		SouthWest = 9
	}

	public static class BlockFaceExtension
	{
		public static BlockFace Convert(this org.bukkit.block.BlockFace bf)
		{
			switch (bf.ordinal())
			{
			case (int)org.bukkit.block.BlockFace.__Enum.SELF:
				return BlockFace.Self;

			case (int)org.bukkit.block.BlockFace.__Enum.UP:
				return BlockFace.Up;

			case (int)org.bukkit.block.BlockFace.__Enum.DOWN:
				return BlockFace.Down;

			case (int)org.bukkit.block.BlockFace.__Enum.NORTH:
				return BlockFace.North;

			case (int)org.bukkit.block.BlockFace.__Enum.SOUTH:
				return BlockFace.South;

			case (int)org.bukkit.block.BlockFace.__Enum.EAST:
				return BlockFace.East;

			case (int)org.bukkit.block.BlockFace.__Enum.WEST:
				return BlockFace.West;

			case (int)org.bukkit.block.BlockFace.__Enum.NORTH_EAST:
				return BlockFace.NorthEast;

			case (int)org.bukkit.block.BlockFace.__Enum.NORTH_WEST:
				return BlockFace.NorthWest;

			case (int)org.bukkit.block.BlockFace.__Enum.SOUTH_EAST:
				return BlockFace.SouthEast;

			case (int)org.bukkit.block.BlockFace.__Enum.SOUTH_WEST:
				return BlockFace.SouthWest;

			default:
				return BlockFace.Self;
			}
		}
	}
}
