using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Properties;

namespace Chraft.World
{
	public class BlockOld
	{
		/*
		public BlockData.Blocks Type;
		public byte Light;
		public byte Data;
		public byte Sky;
		public int X, Y, Z;

		public WorldManager World { get; private set; }
		public Chunk Chunk { get; private set; }
		public Block Up { get { return World.GetBlockOrNull(X, Y + 1, Z); } }
		public Block Down { get { return World.GetBlockOrNull(X, Y - 1, Z); } }
		public Block East { get { return World.GetBlockOrNull(X, Y, Z - 1); } }
		public Block West { get { return World.GetBlockOrNull(X, Y, Z + 1); } }
		public Block South { get { return World.GetBlockOrNull(X + 1, Y, Z); } }
		public Block North { get { return World.GetBlockOrNull(X - 1, Y, Z); } }
		public Block[] NSEW { get { return new Block[] { North, South, East, West }; } }
		public Block[] Adjacent { get { return new Block[] { Up, Down, North, South, East, West }; } }
		public byte Luminence { get { return BlockData.Luminence[(int)Type]; } }
		public byte Opacity { get { return BlockData.Opacity[(int)Type]; } }

		public Block[] AdjacentSameChunk
		{
			get
			{
				return new Block[]
				{
					Y < 127 ? Chunk.ChunkBlocks[X & 0xf, Y + 1, Z & 0xf] : null,
					Y > 0 ? Chunk.ChunkBlocks[X & 0xf, Y - 1, Z & 0xf] : null,
					(X & 0xf) > 0 ? Chunk.ChunkBlocks[(X & 0xf) - 1, Y, Z & 0xf] : null,
					(X & 0xf) < 0xf ? Chunk.ChunkBlocks[(X & 0xf) + 1, Y, Z & 0xf] : null,
					(Z & 0xf) > 0? Chunk.ChunkBlocks[X & 0xf, Y, (Z & 0xf) - 1] : null,
					(Z & 0xf) < 0xf? Chunk.ChunkBlocks[X & 0xf, Y, (Z & 0xf) + 1] : null,
				};
			}
		}

		public Block(WorldManager world, Chunk chunk, BlockData.Blocks type, int x, int y, int z)
		{
			World = world;
			Chunk = chunk;
			Type = type;
			Light = 0;
			Data = 0;
			Sky = 0;
			X = x;
			Y = y;
			Z = z;
		}

		public void Relight()
		{
		}

		public Block FromFace(BlockFace face)
		{
			switch (face)
			{
			case BlockFace.Up: return Up;
			case BlockFace.Down: return Down;
			case BlockFace.North: return North;
			case BlockFace.South: return South;
			case BlockFace.East: return East;
			case BlockFace.West: return West;
			default: return null;
			}
		}

		public void UpdateClients()
		{
			foreach (Client c in World.Server.GetNearbyPlayers(World, X, Y, Z))
				c.SendBlock(this);
		}

		public bool IsAdjacentTo(BlockData.Blocks type)
		{
			return Up.Type == type || Down.Type == type || North.Type == type || South.Type == type || East.Type == type || West.Type == type;
		}

		public void GrowTree()
		{
			if (Y > 123)
				return;

			if (Up == null
				|| Up.Up == null
				|| Up.Up.Up == null
				|| Up.Up.Up.Up == null
				|| North == null
				|| North.North == null
				|| South == null
				|| South.South == null
				|| East == null
				|| East.East == null
				|| West == null
				|| West.West == null)
			{	// We're on the edge of a loaded region of chunks, so don't grow, since the rest isn't loaded.
				Type = BlockData.Blocks.Sapling;
				return;
			}

			// Trunk
			Type = BlockData.Blocks.Log;
			Up.Type = BlockData.Blocks.Log;
			Up.Up.Type = BlockData.Blocks.Log;
			Up.Up.Up.Type = BlockData.Blocks.Log;

			//Leaves, top layer
			Up.Up.Up.Up.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.Up.North.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.Up.South.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.Up.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.Up.West.Type = BlockData.Blocks.Leaves;

			// Leaves, middle layer
			Up.Up.Up.North.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.West.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.North.North.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.South.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.West.West.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.North.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.North.West.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.West.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.North.North.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.North.North.West.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.South.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.South.West.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.North.North.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.North.North.West.West.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.South.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.South.West.West.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.North.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.North.West.West.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.Up.South.West.West.Type = BlockData.Blocks.Leaves;

			// Leaves, bottom layer
			Up.Up.North.Type = BlockData.Blocks.Leaves;
			Up.Up.South.Type = BlockData.Blocks.Leaves;
			Up.Up.East.Type = BlockData.Blocks.Leaves;
			Up.Up.West.Type = BlockData.Blocks.Leaves;
			Up.Up.North.North.Type = BlockData.Blocks.Leaves;
			Up.Up.South.South.Type = BlockData.Blocks.Leaves;
			Up.Up.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.West.West.Type = BlockData.Blocks.Leaves;
			Up.Up.North.East.Type = BlockData.Blocks.Leaves;
			Up.Up.North.West.Type = BlockData.Blocks.Leaves;
			Up.Up.South.East.Type = BlockData.Blocks.Leaves;
			Up.Up.South.West.Type = BlockData.Blocks.Leaves;
			Up.Up.North.North.East.Type = BlockData.Blocks.Leaves;
			Up.Up.North.North.West.Type = BlockData.Blocks.Leaves;
			Up.Up.South.South.East.Type = BlockData.Blocks.Leaves;
			Up.Up.South.South.West.Type = BlockData.Blocks.Leaves;
			Up.Up.North.North.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.North.North.West.West.Type = BlockData.Blocks.Leaves;
			Up.Up.South.South.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.South.South.West.West.Type = BlockData.Blocks.Leaves;
			Up.Up.North.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.North.West.West.Type = BlockData.Blocks.Leaves;
			Up.Up.South.East.East.Type = BlockData.Blocks.Leaves;
			Up.Up.South.West.West.Type = BlockData.Blocks.Leaves;
		}

		internal void Grow()
		{
			if (Light < 7 && Sky < 7)
				return;

			switch (Type)
			{
			case BlockData.Blocks.Dirt:
				GrowDirt();
				break;

			case BlockData.Blocks.Cobblestone:
				GrowCobblestone();
				break;

			case BlockData.Blocks.Grass:
				GrowGrass();
				break;

			case BlockData.Blocks.Sapling:
				GrowSapling();
				break;
			}
		}

		private void GrowSapling()
		{
			if (World.Server.Rand.Next(60) == 0)
			{
				GrowTree();
			}
		}

		private void GrowGrass()
		{
			if (Up == null)
				return;
			if (BlockData.Air.Contains(Up.Type))
			{
				if (World.Server.Rand.Next(Settings.Default.AnimalSpawnInterval) == 0)
					World.SpawnAnimal(Up.X, Up.Y, Up.Z);
			}
			else if (World.Server.Rand.Next(30) != 0)
			{
				Type = BlockData.Blocks.Dirt;
				UpdateClients();
			}
		}

		private void GrowDirt()
		{
			if (Up != null && !BlockData.Air.Contains(Up.Type))
				return;

			if (World.Server.Rand.Next(30) == 0)
			{
				Type = BlockData.Blocks.Grass;
				UpdateClients();
			}
		}

		private void GrowCobblestone()
		{
			if (!IsAdjacentTo(BlockData.Blocks.Mossy_Cobblestone))
				return;

			if (World.Server.Rand.Next(60) == 0)
			{
				Type = BlockData.Blocks.Mossy_Cobblestone;
				UpdateClients();
			}
		}

		internal void Update(bool updateClients = true)
		{
			if (updateClients)
				UpdateClients();
			UpdatePhysics();
			if (Up != null)
				Up.UpdatePhysics();
			if (Down != null)
				Down.UpdatePhysics();
			if (North != null)
				North.UpdatePhysics();
			if (South != null)
				South.UpdatePhysics();
			if (East != null)
				East.UpdatePhysics();
			if (West != null)
				West.UpdatePhysics();
		}

		internal void UpdatePhysics(bool updateClients = true)
		{
			if (Type == BlockData.Blocks.Sand && Y > 0 && Down.Type == BlockData.Blocks.Air)
			{
				Down.Type = BlockData.Blocks.Sand;
				Type = BlockData.Blocks.Air;
				Down.Update(updateClients);
				return;
			}

			if (Type == BlockData.Blocks.Gravel && Y > 0 && Down.Type == BlockData.Blocks.Air)
			{
				Down.Type = BlockData.Blocks.Gravel;
				Type = BlockData.Blocks.Air;
				Down.Update(updateClients);
				return;
			}

			if (Type == BlockData.Blocks.Water)
			{
				byte water = 8;
				foreach (Block b in NSEW)
				{
					if (b == null)
						continue;
					if (b.Type == BlockData.Blocks.Still_Water)
						water = 0;
					else if (b.Type == BlockData.Blocks.Water && b.Data < water)
						water = (byte)(b.Data + 1);
				}
				if (water != Data)
				{
					if (water == 8)
					{
						Type = BlockData.Blocks.Air;
						Data = 0;
					}
					else
					{
						Data = water;
					}
					Update(updateClients);
					return;
				}
			}

			if (Type == BlockData.Blocks.Air)
			{
				if (Y < 127 && (Up.Type == BlockData.Blocks.Water || Up.Type == BlockData.Blocks.Still_Water))
				{
					Type = BlockData.Blocks.Water;
					Data = 0;
					Update(updateClients);
					return;
				}

				if (Y < 127 && (Up.Type == BlockData.Blocks.Lava || Up.Type == BlockData.Blocks.Still_Lava))
				{
					Type = BlockData.Blocks.Lava;
					Data = 0;
					Update(updateClients);
					return;
				}

				byte water = 8;
				foreach (Block b in NSEW)
				{
					if (b == null)
						continue;
					if (b.Type == BlockData.Blocks.Still_Water)
						water = 0;
					else if (b.Type == BlockData.Blocks.Water && b.Data < water)
						water = (byte)(b.Data + 1);
				}
				if (water < 8)
				{
					Type = BlockData.Blocks.Water;
					Data = water;
					Update(updateClients);
					return;
				}

				byte lava = 4;
				foreach (Block b in NSEW)
				{
					if (b == null)
						continue;
					if (b.Type == BlockData.Blocks.Still_Lava)
						lava = 0;
					else if (b.Type == BlockData.Blocks.Lava && b.Data < lava)
						water = (byte)(b.Data + 1);
				}
				if (lava < 4)
				{
					Type = BlockData.Blocks.Lava;
					Data = water;
					Update(updateClients);
					return;
				}
			}
		}
		*/
	}

    public class BlockData
    {
        public static readonly byte[] Opacity = new byte[]
		{
			//0    1    2    3    4    5    6    7    8    9    a    b    c    d    e    f
			0x0, 0xf, 0xf, 0xf, 0xf, 0xf, 0x0, 0xf, 0x2, 0x2, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0x1, 0x0, 0x0, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0x0, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0x0, 0x0, 0x0, 0x0, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0x0, 0x0, 0x0, 0xf, 0xf, 0x0, 0xf, 0xf, 0xf, 0x0, 0xf, 0xf, 0xf, 0x0, 
			0x0, 0x0, 0x0, 0xf, 0x0, 0x0, 0x0, 0x0, 0x0, 0xf, 0xf, 0x0, 0x0, 0x0, 0x0, 0x2, 
			0xf, 0x0, 0x0, 0x0, 0xf, 0x0, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0x0, 0x0, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
			0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf
		};

        public static readonly byte[] Luminance = new byte[]
		{
			//0    1    2    3    4    5    6    7    8    9    a    b    c    d    e    f
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xf, 0xf, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0xe, 0xf, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x9, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xf, 0x0, 0xf, 0x0, 0x0, 0x7, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0
		};

        public static readonly Blocks[] Air = new Blocks[]
		{
			Blocks.Air,
			Blocks.Brown_Mushroom,
            Blocks.Crops,
            Blocks.Fence,
            Blocks.Ladder,
            Blocks.Lever,
            Blocks.Rails,
            Blocks.Reed,
			Blocks.Red_Mushroom,
			Blocks.Red_Rose,
            Blocks.Redstone_Wire,
            Blocks.Redstone_Torch,
            Blocks.Redstone_Torch_On,
			Blocks.Sapling,
            Blocks.Sign_Post,
            Blocks.Stone_Button,
            Blocks.Stone_Pressure_Plate,
            Blocks.Torch,
			Blocks.Yellow_Flower,
            Blocks.Wall_Sign,
            Blocks.Wooden_Pressure_Plate
		};

        public static readonly Blocks[] SingleHit = new Blocks[]
		{
			Blocks.Crops,
			Blocks.Redstone_Repeater,
			Blocks.Redstone_Repeater_On,
			Blocks.Redstone_Torch,
			Blocks.Redstone_Torch_On,
			Blocks.Redstone_Wire,
			Blocks.Reed,
			Blocks.Sapling,
			Blocks.Torch,
            Blocks.TNT
		};

        public static bool IsOpaque(int p)
        {
            return !Air.Contains((Blocks)p) && p != (int)Blocks.Glass && p != (int)Blocks.Ice;
        }

        public static bool IsSolid(Blocks block)
        {
            return !Air.Contains((Blocks)block) && !IsLiquid(block);
        }

        public static bool IsLiquid(Blocks material)
        {
            return material == Blocks.Still_Water || material == Blocks.Still_Lava || material == Blocks.Water || material == Blocks.Lava;
        }

        public enum Blocks : byte
        {
            //Invalid = 255,

            //Block Tiles
            Air = 0,
            Stone = 1,
            Grass = 2,
            Dirt = 3,
            Cobblestone = 4,
            Wood = 5,
            Sapling = 6,
            Bedrock = 7,
            Adminium = 7,
            Water = 8,
            Stationary_Water = 9,
            Still_Water = 9,
            Lava = 10,
            Stationary_Lava = 11,
            Still_Lava = 11,
            Sand = 12,
            Gravel = 13,
            Gold_Ore = 14,
            Iron_Ore = 15,
            Coal_Ore = 16,
            Log = 17,
            Leaves = 18,
            Glass = 20,
            Lapis_Lazuli_Ore = 21,
            Lapis_Lazuli_Block = 22,
            Dispenser = 23,
            Sandstone = 24,
            Note_Block = 25,
            Bed = 26,
            TallGrass = 31,
            // TODO: add pistons, 29 and 33
            Cloth = 35,
            Wool = 35,
            Yellow_Flower = 37,
            Flower = 37,
            Red_Rose = 38,
            Rose = 38,
            Brown_Mushroom = 39,
            Red_Mushroom = 40,
            Gold_Block = 41,
            Iron_Block = 42,
            Double_Stair = 43,
            Double_Stone_Slab = 43,
            Stair = 44,
            Slab = 44,
            Brick = 45,
            TNT = 46,
            Bookcase = 47,
            Bookshelf = 47,
            Mossy_Cobblestone = 48,
            Obsidian = 49,
            Torch = 50,
            Fire = 51,
            Mob_Spawner = 52,
            Wooden_Stairs = 53,
            Chest = 54,
            Redstone_Wire = 55,
            Diamond_Ore = 56,
            Diamond_Block = 57,
            Workbench = 58,
            Crops = 59,
            Soil = 60,
            Furnace = 61,
            Burning_Furnace = 62,
            Sign_Post = 63,
            Wooden_Door = 64,
            Ladder = 65,
            Minecart_Rail = 66,
            Rails = 66,
            Track = 66,
            Tracks = 66,
            Cobblestone_Stairs = 67,
            Stone_Stairs = 67,
            Wall_Sign = 68,
            Lever = 69,
            Stone_Pressure_Plate = 70,
            Iron_Door = 71,
            Wooden_Pressure_Plate = 72,
            Redstone_Ore = 73,
            Redstone_Ore_Glowing = 74,
            Redstone_Torch = 75,
            Redstone_Torch_On = 76,
            Stone_Button = 77,
            Snow = 78,
            Ice = 79,
            Snow_Block = 80,
            Cactus = 81,
            Clay = 82,
            Reed = 83,
            Jukebox = 84,
            Fence = 85,
            Pumpkin = 86,
            Bloodstone = 87,
            Netherrack = 87,
            Slow_Sand = 88,
            Soul_Sand = 88,
            Lightstone = 89,
            Glowstone = 89,
            Portal = 90,
            Jack_O_Lantern = 91,
            Pumpkin_Lantern = 91,
            Cake = 92,
            Redstone_Repeater = 93,
            Redstone_Repeater_On = 94
            // TODO: add TrapDoor

        }

        public enum Items : short
        {
            Iron_Spade = 256,
            Iron_Pick = 257,
            Iron_Pickaxe = 257,
            Iron_Axe = 258,

            Flint_And_Steel = 259,
            Lighter = 259,
            Apple = 260,
            Bow = 261,
            Arrow = 262,
            Coal = 263,
            Diamond = 264,
            Iron_Ingot = 265,
            Gold_Ingot = 266,
            Iron_Sword = 267,

            Wooden_Sword = 268,
            Wooden_Spade = 269,
            Wooden_Pick = 270,
            Wooden_Pickaxe = 270,
            Wooden_Axe = 271,

            Stone_Sword = 272,
            Stone_Spade = 273,
            Stone_Pick = 274,
            Stone_Pickaxe = 274,
            Stone_Axe = 275,

            Diamond_Sword = 276,
            Diamond_Spade = 277,
            Diamond_Pick = 278,
            Diamond_Pickaxe = 278,
            Diamond_Axe = 279,

            Stick = 280,
            Bowl = 281,
            Mushroom_Soup = 282,

            Gold_Sword = 283,
            Gold_Spade = 284,
            Gold_Pick = 285,
            Gold_Pickaxe = 285,
            Gold_Axe = 286,

            Bow_String = 287,
            Feather = 288,
            Gunpowder = 289,

            Wooden_Hoe = 290,
            Stone_Hoe = 291,
            Iron_Hoe = 292,
            Diamond_Hoe = 293,
            Gold_Hoe = 294,

            Seeds = 295,
            Wheat = 296,
            Bread = 297,

            Leather_Helmet = 298,
            Leather_Chestplate = 299,
            Leather_Pants = 300,
            Leather_Boots = 301,

            Chainmail_Helmet = 302,
            Chainmail_Chestplate = 303,
            Chainmail_Pants = 304,
            Chainmail_Boots = 305,

            Iron_Helmet = 306,
            Iron_Chestplate = 307,
            Iron_Pants = 308,
            Iron_Boots = 309,

            Diamond_Helmet = 310,
            Diamond_Chestplate = 311,
            Diamond_Pants = 312,
            Diamond_Boots = 313,

            Gold_Helmet = 314,
            Gold_Chestplate = 315,
            Gold_Pants = 316,
            Gold_Boots = 317,

            Flint = 318,
            Pork = 319,
            Grilled_Pork = 320,
            Paintings = 321,
            Golden_Apple = 322,
            Sign = 323,
            Wooden_Door = 324,
            Bucket = 325,
            Water_Bucket = 326,
            Lava_Bucket = 327,
            Mine_Cart = 328,
            Minecart = 328,
            Saddle = 329,
            Iron_Door = 330,
            Redstone = 331,
            Snowball = 332,
            Boat = 333,
            Leather = 334,
            Milk_Bucket = 335,
            Clay_Brick = 336,
            Clay_Balls = 337,
            Reeds = 338,
            Paper = 339,
            Book = 340,
            Slime_Ball = 341,
            Storage_Minecart = 342,
            Powered_Minecart = 343,
            Egg = 344,
            Compass = 345,
            Fishing_Rod = 346,
            Watch = 347,
            Lightstone_Dust = 348,
            Raw_Fish = 349,
            Cooked_Fish = 350,
            Ink_Sack = 351,
            Bone = 352,
            Sugar = 353,
            Cake = 354,
            Bed = 355,
            Redstone_Repeater = 356,
            Cookie = 357,
            Map = 358,
            Shears = 359,

            Gold_Record = 2256,
            Green_Record = 2257
        }

        /// <summary>
        /// Block Flammability/Burn Efficiency measured in world ticks (x0.05secs). Value / 20 => number of seconds burn time. 10secs = 1 item smelted
        /// </summary>
        public static readonly Dictionary<Blocks, short> BlockBurnEfficiency = new Dictionary<Blocks, short>()
        {
            {Blocks.Bookshelf, 300},
            {Blocks.Chest, 300},
            {Blocks.Workbench, 300},
            {Blocks.Fence, 300},
            {Blocks.Jukebox, 300},
            {Blocks.Note_Block, 300},
            {Blocks.Log, 300},
            {Blocks.Wood, 300},
            {Blocks.Wooden_Stairs, 300},

            {Blocks.Sapling, 100},
        };

        /// <summary>
        /// Item Flammability/Burn Efficiency measured in world ticks (x0.05secs). Value / 20 => number of seconds burn time. 10secs = 1 item smelted
        /// </summary>
        public static readonly Dictionary<Items, short> ItemBurnEfficiency = new Dictionary<Items, short>()
        {
            {Items.Lava_Bucket, 20000},
            {Items.Coal, 1600},
            {Items.Stick, 100},
        };

        public static readonly Dictionary<Items, short> ToolDuarability = new Dictionary<Items, short>() {
            // TODO: Put in a csv for user control.
            {Items.Cake, 6},
            {Items.Chainmail_Boots, 132},
            {Items.Chainmail_Chestplate, 132},
            {Items.Chainmail_Helmet, 132},
            {Items.Chainmail_Pants, 132},
            {Items.Diamond_Axe, 1562},
            {Items.Diamond_Boots, 1562},
            {Items.Diamond_Chestplate, 1562},
            {Items.Diamond_Helmet, 1562},
            {Items.Diamond_Hoe, 1562},
            {Items.Diamond_Pants, 1562},
            {Items.Diamond_Pick, 1562},
            {Items.Diamond_Spade, 1562},
            {Items.Diamond_Sword, 1562},
            {Items.Fishing_Rod, 90},
            {Items.Flint_And_Steel, 64},
            {Items.Gold_Axe, 33},
            {Items.Gold_Boots, 33},
            {Items.Gold_Chestplate, 33},
            {Items.Gold_Helmet, 33},
            {Items.Gold_Hoe, 33},
            {Items.Gold_Pants, 33},
            {Items.Gold_Pickaxe, 33},
            {Items.Gold_Spade, 33},
            {Items.Gold_Sword, 33},
            {Items.Iron_Axe, 251},
            {Items.Iron_Boots, 251},
            {Items.Iron_Chestplate, 251},
            {Items.Iron_Helmet, 251},
            {Items.Iron_Hoe, 251},
            {Items.Iron_Pants, 251},
            {Items.Iron_Pick, 251},
            {Items.Iron_Spade, 251},
            {Items.Iron_Sword, 251},
            {Items.Leather_Boots, 60},
            {Items.Leather_Chestplate, 60},
            {Items.Leather_Helmet, 60},
            {Items.Leather_Pants, 60},
            {Items.Shears, 238},
            {Items.Stone_Axe, 132},
            {Items.Stone_Hoe, 132},
            {Items.Stone_Pick, 132},
            {Items.Stone_Spade, 132},
            {Items.Stone_Sword, 132},
            {Items.Wooden_Axe, 60},
            {Items.Wooden_Hoe, 60},
            {Items.Wooden_Pick, 60},
            {Items.Wooden_Spade, 60},
            {Items.Wooden_Sword, 60}
        };
    }
}
