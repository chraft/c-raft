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

using System.Collections.Generic;

namespace Chraft.Utilities.Blocks
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
				if (World.Server.Rand.Next(ChraftConfig.AnimalSpawnInterval) == 0)
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
        public enum Blocks : byte
        {
            //Invalid = 255,

            //Block Tiles
            Air = 0,
            Stone = 1,
            Grass = 2,
            Dirt = 3,
            Cobblestone = 4,
            Wood_Planks = 5,
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
            Wood = 17,
            Leaves = 18,
            Sponge = 19,
            Glass = 20,
            Lapis_Lazuli_Ore = 21,
            Lapis_Lazuli_Block = 22,
            Dispenser = 23,
            Sandstone = 24,
            Note_Block = 25,
            Bed = 26,
            PoweredRail = 27,
            DetectorRail = 28,
            StickyPiston = 29,
            Cobweb = 30,
            TallGrass = 31,
            DeadBush = 32,
            Piston = 33,
            PistonHead = 34,
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
            Moss_Stone = 48,
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
            Redstone_Repeater_On = 94,
            LockedChest = 95,
            Trapdoor = 96,
            SilverfishStone = 97,
            StoneBrick = 98,
            BrownMushroomCap = 99,
            RedMushroomCap = 100,
            IronBars = 101,
            GlassPane = 102,
            Melon = 103,
            PumpkinStem = 104,
            MelonStem = 105,
            Vines = 106,
            FenceGate = 107,
            BrickStairs = 108,
            StoneBrickStairs = 109,
            Mycelium = 110,
            LilyPad = 111,
            NetherBrick = 112,
            NetherBrickFence = 113,
            NetherBrickStairs = 114,
            NetherWart = 115,
            Enchantment_Table = 116,
            Brewing_Stand = 117,
            Cauldron = 118,
            End_Portal = 119,
            End_Portal_Block = 120,
            End_Stone = 121,
            Dragon_Egg = 122,
            Redstone_Lamp = 123,
            Redstone_Lamp_On = 124,
            Wooden_Double_Slab = 125,
            Wooden_Slab = 126,
            Cocoa = 127,
            Sandstone_Stairs = 128,
            Emerald_Ore = 129,
            Ender_Chest = 130,
            Tripwire_Hook = 131,
            Tripwire = 132,
            Block_Of_Emerald = 133,
            Spruce_Wood_Stairs = 134,
            Birch_Wood_Stairs = 135,
            Jungle_Wood_Stairs = 136,
            Command_Block = 137,
            Beacon = 138,
            Cobblestone_Wall = 139,
            Flower_Pot = 140,
            Carrots = 141,
            Potatoes = 142,
            Wooden_Button = 143,
            Mob_Head = 144,
            Anvil = 145,
            Trapped_Chest = 146,
            Weighted_Pressure_Plate_Light = 147,
            Weighted_Pressure_Plate_Heavy = 148,
            Redstone_Comparator = 149,
            Redstone_Comparator_On = 150,
            Daylight_Sensor = 151,
            Block_Of_Redstone = 152,
            Nether_Quartz_Ore = 153,
            Hopper = 154,
            Block_Of_Quartz = 155,
            Quartz_Stairs = 156,
            Activator_Rail = 157,
            Dropper = 158,
            Hay_Block = 170,
            Carpet = 171,
            Hardened_Clay = 172,
            Block_Of_Coal = 173,
        }

        public enum Items : short
        {
            Iron_Shovel = 256,
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
            Wooden_Shovel = 269,
            Wooden_Pick = 270,
            Wooden_Pickaxe = 270,
            Wooden_Axe = 271,

            Stone_Sword = 272,
            Stone_Shovel = 273,
            Stone_Pick = 274,
            Stone_Pickaxe = 274,
            Stone_Axe = 275,

            Diamond_Sword = 276,
            Diamond_Shovel = 277,
            Diamond_Pick = 278,
            Diamond_Pickaxe = 278,
            Diamond_Axe = 279,

            Stick = 280,
            Bowl = 281,
            Mushroom_Soup = 282,

            Gold_Sword = 283,
            Gold_Shovel = 284,
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
            Raw_Porkchop = 319,
            Cooked_Porkchop = 320,
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

            Melon = 360,
            PumpkinSeeds = 361,
            MelonSeeds = 362,
            RawBeef = 363,
            Steak = 364,
            RawChicken = 365,
            CookedChicken = 366,
            RottenFlesh = 367,
            EnderPearl = 368,
            BlazeRod = 369,
            GhastTear = 370,
            GoldNugget = 371,
            NetherWartSeeds = 372,
            Potion = 373,
            GlassBottle = 374,
            SpiderEye = 375,
            FermentedSpiderEye = 376,
            BlazePowder = 377,
            MagmaCream = 378,

            Brewing_Stand = 379,
            Cauldron = 380,
            Eye_Of_Ender = 381,
            GlisteringMelon = 382, 
            Spawn_Egg = 383,
            Bottle_O_Enchanting = 384,
            Fire_Charge = 385,
            Book_And_Quill = 386,
            Written_Book = 387,
            Emerald = 388,
            Item_Frame = 389,
            Flower_Pot = 390,
            Carrot = 391,
            Potato = 392,
            Baked_Potato = 393,
            Poisonous_Potato = 394,
            Empty_Map = 395,

            Golden_Carrot = 396,
		    Mob_Head = 397,
		    Carrot_On_A_Stick = 398,
		    Nether_Star = 399,
		    Pumpkin_Pie = 400,
		    Firework_Rocket = 401,
		    Firework_Star = 402,
		    Enchanted_Book = 403,
		    Redstone_Comparator = 404,
		    Nether_Brick = 405,
		    Nether_Quartz = 406,
		    Minecart_with_TNT = 407,
		    Minecart_with_Hopper = 408,
		    Iron_Horse_Armor = 417,
		    Gold_Horse_Armor = 418,
		    Diamond_Horse_Armor = 419,
		    Lead = 420,
		    Name_Tag = 421,

            Disc13 = 2256,
            DiscCat = 2257,
            DiscBlocks = 2258,
            DiscChirp = 2259,
            DiscFar = 2260,
            DiscMall = 2261,
            DiscMellohi = 2262,
            DiscStal = 2263,
            DiscStrad = 2264,
            DiscWard = 2265,
            Disc11 = 2266
        }

        public static readonly Dictionary<Items, short> ToolDuarability = new Dictionary<Items, short>() {
            // TODO: Put in a csv for user control.
            {Items.Cake, 6},
            {Items.Chainmail_Boots, 196},
            {Items.Chainmail_Chestplate, 241},
            {Items.Chainmail_Helmet, 166},
            {Items.Chainmail_Pants, 226},
            {Items.Diamond_Axe, 1562},
            {Items.Diamond_Boots, 430},
            {Items.Diamond_Chestplate, 529},
            {Items.Diamond_Helmet, 364},
            {Items.Diamond_Hoe, 1562},
            {Items.Diamond_Pants, 496},
            {Items.Diamond_Pick, 1562},
            {Items.Diamond_Shovel, 1562},
            {Items.Diamond_Sword, 1562},
            {Items.Fishing_Rod, 33},
            {Items.Flint_And_Steel, 65},
            {Items.Gold_Axe, 33},
            {Items.Gold_Boots, 92},
            {Items.Gold_Chestplate, 113},
            {Items.Gold_Helmet, 78},
            {Items.Gold_Hoe, 33},
            {Items.Gold_Pants, 106},
            {Items.Gold_Pickaxe, 33},
            {Items.Gold_Shovel, 33},
            {Items.Gold_Sword, 33},
            {Items.Iron_Axe, 251},
            {Items.Iron_Boots, 196},
            {Items.Iron_Chestplate, 241},
            {Items.Iron_Helmet, 166},
            {Items.Iron_Hoe, 251},
            {Items.Iron_Pants, 226},
            {Items.Iron_Pick, 251},
            {Items.Iron_Shovel, 251},
            {Items.Iron_Sword, 251},
            {Items.Leather_Boots, 66},
            {Items.Leather_Chestplate, 81},
            {Items.Leather_Helmet, 56},
            {Items.Leather_Pants, 76},
            {Items.Shears, 239},
            {Items.Stone_Axe, 132},
            {Items.Stone_Hoe, 132},
            {Items.Stone_Pick, 132},
            {Items.Stone_Shovel, 132},
            {Items.Stone_Sword, 132},
            {Items.Wooden_Axe, 60},
            {Items.Wooden_Hoe, 60},
            {Items.Wooden_Pick, 60},
            {Items.Wooden_Shovel, 60},
            {Items.Wooden_Sword, 60}
        };
    }
}
