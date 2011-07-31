using System;
using System.Linq;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.Entity;
using System.Text.RegularExpressions;
using Chraft.Utils;
using Chraft.Interfaces;

namespace Chraft
{
    public partial class Client
    {
        public bool OnGround { get; set; }
        public double Stance { get; set; }

        private void InitializeRecv()
        {
            InitializeRecvBasic();
            InitializeRecvPlayer();
            InitializeRecvInterface();
            InitializeRecvUse();
        }

        private void InitializeRecvUse()
        {
            PacketHandler.UseEntity += new PacketEventHandler<UseEntityPacket>(PacketHandler_UseEntity);
            PacketHandler.UseBed += new PacketEventHandler<UseBedPacket>(PacketHandler_UseBed);
        }

        private void InitializeRecvInterface()
        {
            PacketHandler.CloseWindow += PacketHandler_CloseWindow;
            PacketHandler.WindowClick += PacketHandler_WindowClick;
        }

        private void InitializeRecvBasic()
        {
            PacketHandler.ChatMessage += PacketHandler_ChatMessage;
            PacketHandler.LoginRequest += PacketHandler_LoginRequest;
            PacketHandler.Handshake += PacketHandler_Handshake;
            PacketHandler.Disconnect += PacketHandler_Disconnect;
        }

        private void InitializeRecvPlayer()
        {
            PacketHandler.Animation += new PacketEventHandler<AnimationPacket>(PacketHandler_Animation);
            PacketHandler.PlayerPosition += PacketHandler_PlayerPosition;
            PacketHandler.PlayerPositionRotation += PacketHandler_PlayerPositionRotation;
            PacketHandler.PlayerRotation += PacketHandler_PlayerRotation;
            PacketHandler.PlayerDigging += PacketHandler_PlayerDigging;
            PacketHandler.Player += PacketHandler_Player;
            PacketHandler.PlayerBlockPlacement += PacketHandler_PlayerBlockPlacement;
            PacketHandler.HoldingChange += PacketHandler_HoldingChange;
            PacketHandler.Respawn += PacketHander_Respawn; // Does this need a new handler?
        }

        private void PacketHandler_Animation(object sender, PacketEventArgs<AnimationPacket> e)
        {
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (c == this)
                    continue;
                c.PacketHandler.SendPacket(new AnimationPacket
                {
                    Animation = e.Packet.Animation,
                    PlayerId = this.EntityId
                });
            }
        }

        private void PacketHander_Respawn(object sender, PacketEventArgs<RespawnPacket> e)
        {
            HandleRespawn();
        }

        private void PacketHandler_ChatMessage(object sender, PacketEventArgs<ChatMessagePacket> e)
        {
            string clean = Chat.CleanMessage(e.Packet.Message);

            if (clean.StartsWith("/"))
                ExecuteCommand(clean.Substring(1));
            else
                ExecuteChat(clean);
        }


        #region Use

        private void PacketHandler_UseBed(object sender, PacketEventArgs<UseBedPacket> e)
        {
            throw new NotImplementedException();
        }

        private void PacketHandler_UseEntity(object sender, PacketEventArgs<UseEntityPacket> e)
        {
            //Console.WriteLine(e.Packet.Target);
            //this.SendMessage("You are interacting with " + e.Packet.Target + " " + e.Packet.LeftClick);

            foreach (EntityBase eb in Server.GetNearbyEntities(World, Position.X, Position.Y, Position.Z))
            {
                if (eb.EntityId != e.Packet.Target)
                    continue;

                if (eb is Client)
                {
                    Client c = (Client)eb;

                    if (e.Packet.LeftClick)
                    {
                        if (c.Health > 0)
                            c.DamageClient(DamageCause.EntityAttack, this);
                    }
                    else
                    {
                        // TODO: Store the object being ridden, so we can update player movement.
                        // This will ride the entity, sends -1 to dismount.
                        foreach (Client cl in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
                        {
                            cl.PacketHandler.SendPacket(new AttachEntityPacket
                            {
                                EntityId = this.EntityId,
                                VehicleId = c.EntityId
                            });
                        }
                    }
                }
                else if (eb is Mob)
                {
                    Mob m = (Mob)eb;

                    if (e.Packet.LeftClick)
                    {
                        if (m.Health > 0)
                            m.DamageMob(this);
                    }
                    else
                    {
                        // TODO: Check Entity has saddle set.
                        // This will ride the entity, sends -1 to dismount.
                        foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
                        {
                            c.PacketHandler.SendPacket(new AttachEntityPacket
                            {
                                EntityId = this.EntityId,
                                VehicleId = c.EntityId
                            });
                        }
                    }
                }
                /*else
                {
                    this.SendMessage(e.Packet.Target + " has no interaction handler!");
                }*/
            }
        }

        #endregion


        #region Interfaces

        private void PacketHandler_WindowClick(object sender, PacketEventArgs<WindowClickPacket> e)
        {
            Interface iface = CurrentInterface ?? Inventory;
            iface.OnClicked(e.Packet);
        }

        private void PacketHandler_CloseWindow(object sender, PacketEventArgs<CloseWindowPacket> e)
        {
            if (CurrentInterface != null)
            {
                CurrentInterface.Close(false);
            }
            else if (this.Inventory != null && e.Packet.WindowId == this.Inventory.Handle)
            {
                this.Inventory.Close(false);
            }
            CurrentInterface = null;
        }

        private void PacketHandler_HoldingChange(object sender, PacketEventArgs<HoldingChangePacket> e)
        {
            Inventory.OnActiveChanged((short)(e.Packet.Slot += 36));

            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z).Where(c => c != this))
            {
                c.SendHoldingEquipment(this);
            }
        }

        #endregion


        #region Block Con/Destruction

        private void PacketHandler_PlayerItemPlacement(object sender, PacketEventArgs<PlayerBlockPlacementPacket> e)
        {
            // if(!Permissions.CanPlayerBuild(Username)) return;
            if (Inventory.Slots[Inventory.ActiveSlot].Type <= 255)
                return;

            int x = e.Packet.X;
            int y = e.Packet.Y;
            int z = e.Packet.Z;

            BlockData.Blocks adjacentBlockType = (BlockData.Blocks)World.GetBlockId(x, y, z); // Get block being built against.

            // Placed Item Info
            int px, py, pz;
            short pType = Inventory.Slots[Inventory.ActiveSlot].Type;
            short pMetaData = Inventory.Slots[Inventory.ActiveSlot].Durability;

            World.FromFace(x, y, z, e.Packet.Face, out px, out py, out pz);

            switch (e.Packet.Face)
            {
                case BlockFace.Held:
                    return; // TODO: Process buckets, food, etc.
            }

            switch (adjacentBlockType)
            {
                case BlockData.Blocks.Air:
                case BlockData.Blocks.Water:
                case BlockData.Blocks.Lava:
                case BlockData.Blocks.Still_Water:
                case BlockData.Blocks.Still_Lava:
                    return;
            }

            switch ((BlockData.Items)pType)
            {
                case BlockData.Items.Diamond_Hoe:
                case BlockData.Items.Gold_Hoe:
                case BlockData.Items.Iron_Hoe:
                case BlockData.Items.Stone_Hoe:
                case BlockData.Items.Wooden_Hoe:
                    if (adjacentBlockType == BlockData.Blocks.Dirt || adjacentBlockType == BlockData.Blocks.Grass)
                    {
                        // Think the client has a Notch bug where hoe's durability is not updated properly.
                        px = x; py = y; pz = z;
                        World.SetBlockAndData(px, py, pz, (byte)BlockData.Blocks.Soil, 0x00);
                    }
                    break;

                case BlockData.Items.Sign:

                    if (e.Packet.Face == BlockFace.Up) // Floor Sign
                    {
                        // Get the direction the player is facing.
                        switch (FacingDirection(8))
                        {
                            case "N":
                                pMetaData = (byte)MetaData.SignPost.North;
                                break;
                            case "NE":
                                pMetaData = (byte)MetaData.SignPost.Northeast;
                                break;
                            case "E":
                                pMetaData = (byte)MetaData.SignPost.East;
                                break;
                            case "SE":
                                pMetaData = (byte)MetaData.SignPost.Southeast;
                                break;
                            case "S":
                                pMetaData = (byte)MetaData.SignPost.South;
                                break;
                            case "SW":
                                pMetaData = (byte)MetaData.SignPost.Southwest;
                                break;
                            case "W":
                                pMetaData = (byte)MetaData.SignPost.West;
                                break;
                            case "NW":
                                pMetaData = (byte)MetaData.SignPost.Northwest;
                                break;
                            default:
                                return;
                        }

                        World.SetBlockAndData(px, py, pz, (byte)BlockData.Blocks.Sign_Post, (byte)pMetaData);
                    }
                    else // Wall Sign
                    {
                        switch (e.Packet.Face)
                        {
                            case BlockFace.East: pMetaData = (byte)MetaData.SignWall.East;
                                break;
                            case BlockFace.West: pMetaData = (byte)MetaData.SignWall.West;
                                break;
                            case BlockFace.North: pMetaData = (byte)MetaData.SignWall.North;
                                break;
                            case BlockFace.South: pMetaData = (byte)MetaData.SignWall.South;
                                break;
                            case BlockFace.Down:
                                return;
                        }

                        World.SetBlockAndData(px, py, pz, (byte)BlockData.Blocks.Wall_Sign, (byte)pMetaData);
                    }
                    break;

                case BlockData.Items.Seeds:
                    if (adjacentBlockType == BlockData.Blocks.Soil && e.Packet.Face == BlockFace.Down)
                    {
                        World.SetBlockAndData(px, py, pz, (byte)BlockData.Blocks.Crops, 0x00);
                    }
                    break;

                case BlockData.Items.Redstone:
                    World.SetBlockAndData(px, py, pz, (byte)BlockData.Blocks.Redstone_Wire, 0x00);
                    break;

                case BlockData.Items.Minecart:
                case BlockData.Items.Boat:
                case BlockData.Items.Storage_Minecart:
                case BlockData.Items.Powered_Minecart:
                    // TODO: Create new object
                    break;

                case BlockData.Items.Iron_Door:
                case BlockData.Items.Wooden_Door:
                    {
                        if (!BlockData.Air.Contains((BlockData.Blocks)World.GetBlockId(px, py + 1, pz)))
                            return;

                        switch (FacingDirection(4)) // Built on floor, set by facing dir
                        {
                            case "N":
                                pMetaData = (byte)MetaData.Door.Northwest;
                                break;
                            case "W":
                                pMetaData = (byte)MetaData.Door.Southwest;
                                break;
                            case "S":
                                pMetaData = (byte)MetaData.Door.Southeast;
                                break;
                            case "E":
                                pMetaData = (byte)MetaData.Door.Northeast;
                                break;
                            default:
                                return;
                        }

                        if ((BlockData.Items)pType == BlockData.Items.Iron_Door)
                        {
                            World.SetBlockAndData(px, py + 1, pz, (byte)BlockData.Blocks.Iron_Door, (byte)MetaData.Door.IsTopHalf);
                            World.SetBlockAndData(px, py, pz, (byte)BlockData.Blocks.Iron_Door, (byte)pMetaData);
                        }
                        else
                        {
                            World.SetBlockAndData(px, py + 1, pz, (byte)BlockData.Blocks.Wooden_Door, (byte)MetaData.Door.IsTopHalf);
                            World.SetBlockAndData(px, py, pz, (byte)BlockData.Blocks.Wooden_Door, (byte)pMetaData);
                        }

                        World.Update(px, py + 1, pz);
                    }
                    break;
            }

            if (!Inventory.DamageItem(Inventory.ActiveSlot)) // If item isn't durable, remove it.
                Inventory.RemoveItem(Inventory.ActiveSlot);

            World.Update(px, py, pz);
        }

        private void PacketHandler_PlayerBlockPlacement(object sender, PacketEventArgs<PlayerBlockPlacementPacket> e)
        {
            /*
             * Scenarios:
             * 
             * 1) using an item against a block (e.g. stone and flint)
             * 2) placing a new block
             * 3) using a block: e.g. open/close door, open chest, open workbench, open furnace
             * 
             * */

            //  if (!Permissions.CanPlayerBuild(Username)) return;
            // Using activeslot provides current item info wtihout having to maintain ActiveItem

            int x = e.Packet.X;
            int y = e.Packet.Y;
            int z = e.Packet.Z;

            BlockData.Blocks type = (BlockData.Blocks)World.GetBlockId(x, y, z); // Get block being built against.

            int bx, by, bz;
            World.FromFace(x, y, z, e.Packet.Face, out bx, out by, out bz);

            if (type == BlockData.Blocks.Chest)
            {
                if (!BlockData.Air.Contains((BlockData.Blocks)World.GetBlockId(bx, by, bz)))
                {
                    // Cannot open a chest if no space is above it
                    return;
                }

                Chunk chunk = World.GetBlockChunk(x, y, z);

                // Double chest?
                // TODO: simplify chunk API so that no bit shifting is required
                if (chunk.IsNSEWTo(x & 0xf, y, z & 0xf, (byte)type))
                {
                    // Is this chest the "North or East", or the "South or West"
                    BlockData.Blocks[] nsewBlocks = new BlockData.Blocks[4];
                    PointI[] nsewBlockPositions = new PointI[4];
                    int nsewCount = 0;
                    chunk.ForNSEW(x & 0xf, y, z & 0xf, (x1, y1, z1) =>
                    {
                        nsewBlocks[nsewCount] = (BlockData.Blocks)World.GetBlockId(x1, y1, z1);
                        nsewBlockPositions[nsewCount] = new PointI(x1, y1, z1);
                        nsewCount++;
                    });

                    if (nsewBlocks[0] == type) // North
                    {
                        CurrentInterface = new LargeChestInterface(World, nsewBlockPositions[0], new PointI(x, y, z));
                    }
                    else if (nsewBlocks[2] == type) // East
                    {
                        CurrentInterface = new LargeChestInterface(World, nsewBlockPositions[2], new PointI(x, y, z));
                    }
                    else if (nsewBlocks[1] == type) // South
                    {
                        CurrentInterface = new LargeChestInterface(World, new PointI(x, y, z), nsewBlockPositions[1]);
                    }
                    else if (nsewBlocks[3] == type) // West
                    {
                        CurrentInterface = new LargeChestInterface(World, new PointI(x, y, z), nsewBlockPositions[3]);
                    }
                }
                else
                {
                    CurrentInterface = new SmallChestInterface(World, x, y, z);
                }

                if (CurrentInterface != null)
                {
                    CurrentInterface.Associate(this);
                    CurrentInterface.Open();
                }
                return;
            }
            else if (type == BlockData.Blocks.Workbench)
            {
                CurrentInterface = new WorkbenchInterface();
                CurrentInterface.Associate(this);
                CurrentInterface.Open();
                return;
            }


            if (Inventory.Slots[Inventory.ActiveSlot].Type <= 0 || Inventory.Slots[Inventory.ActiveSlot].Count < 1)
                return;

            // TODO: Neaten this out, or address via handler?
            if (Inventory.Slots[Inventory.ActiveSlot].Type > 255 || e.Packet.Face == BlockFace.Held) // Client is using an Item.
            {
                PacketHandler_PlayerItemPlacement(sender, e);
                return;
            }

            // Built Block Info

            byte bType = (byte)Inventory.Slots[Inventory.ActiveSlot].Type;
            byte bMetaData = (byte)Inventory.Slots[Inventory.ActiveSlot].Durability;

            switch (type) // Can't build against these blocks.
            {
                case BlockData.Blocks.Air:
                case BlockData.Blocks.Water:
                case BlockData.Blocks.Lava:
                case BlockData.Blocks.Still_Water:
                case BlockData.Blocks.Still_Lava:
                    return;
            }

            switch ((BlockData.Blocks)bType)
            {

                case BlockData.Blocks.Cactus:
                    {
                        BlockData.Blocks uType = (BlockData.Blocks)World.GetBlockId(bx, by - 1, bz);
                        if (uType != BlockData.Blocks.Sand && uType != BlockData.Blocks.Cactus)
                            return;
                    }
                    break;

                case BlockData.Blocks.Crops:
                    {
                        BlockData.Blocks uType = (BlockData.Blocks)World.GetBlockId(bx, by - 1, bz);
                        if (uType != BlockData.Blocks.Soil)
                            return;
                    }
                    break;

                case BlockData.Blocks.Chest:
                    // Load the blocks surrounding the position (NSEW) not diagonals
                    Chunk chunk = World.GetBlockChunk(x, y, z);
                    BlockData.Blocks[] nsewBlocks = new BlockData.Blocks[4];
                    PointI[] nsewBlockPositions = new PointI[4];
                    int nsewCount = 0;
                    chunk.ForNSEW(bx & 0xf, by, bz & 0xf, (x1, y1, z1) =>
                    {
                        nsewBlocks[nsewCount] = (BlockData.Blocks)World.GetBlockId(x1, y1, z1);
                        nsewBlockPositions[nsewCount] = new PointI(x1, y1, z1);
                        nsewCount++;
                    });

                    // Count chests in list
                    if (nsewBlocks.Where((b) => b == BlockData.Blocks.Chest).Count() > 1)
                    {
                        // Cannot place next to two chests
                        return;
                    }

                    // A chest cannot be surrounded by two blocks on the same axis when placed
                    //if (BlockData.IsSolid(nsewBlocks[0]) && BlockData.IsSolid(nsewBlocks[1]))
                    //{
                    //    return;
                    //}
                    //if (BlockData.IsSolid(nsewBlocks[2]) && BlockData.IsSolid(nsewBlocks[3]))
                    //{
                    //    return;
                    //}

                    for (int i = 0; i < 4; i++)
                    {
                        PointI p = nsewBlockPositions[i];
                        if (nsewBlocks[i] == BlockData.Blocks.Chest && chunk.IsNSEWTo(p.X & 0xf, p.Y, p.Z & 0xf, (byte)BlockData.Blocks.Chest))
                        {
                            // Cannot place next to a double chest
                            return;
                        }
                    }

                    break;
                case BlockData.Blocks.Furnace:
                case BlockData.Blocks.Dispenser:
                    switch (e.Packet.Face) //Bugged, as the client has a mind of its own for facing
                    {
                        case BlockFace.East: bMetaData = (byte)MetaData.Furnace.East;
                            break;
                        case BlockFace.West: bMetaData = (byte)MetaData.Furnace.West;
                            break;
                        case BlockFace.North: bMetaData = (byte)MetaData.Furnace.North;
                            break;
                        case BlockFace.South: bMetaData = (byte)MetaData.Furnace.South;
                            break;
                        default:
                            switch (FacingDirection(4)) // Built on floor, set by facing dir
                            {
                                case "N":
                                    bMetaData = (byte)MetaData.Furnace.North;
                                    break;
                                case "W":
                                    bMetaData = (byte)MetaData.Furnace.West;
                                    break;
                                case "S":
                                    bMetaData = (byte)MetaData.Furnace.South;
                                    break;
                                case "E":
                                    bMetaData = (byte)MetaData.Furnace.East;
                                    break;
                                default:
                                    return;

                            }
                            break;
                    }
                    break;

                case BlockData.Blocks.Rails:
                    // TODO: Rail Logic                    
                    break;

                case BlockData.Blocks.Reed:
                    // TODO: Check there is water nearby before placing.
                    break;

                case BlockData.Blocks.Stair:
                    // TODO : If (Block  Y - 1 = Stair && Block Y = Air) Then DoubleStair
                    // Else if (Buildblock = Stair) Then DoubleStair
                    break;

                case BlockData.Blocks.Wooden_Stairs:
                case BlockData.Blocks.Cobblestone_Stairs:
                    switch (FacingDirection(4))
                    {
                        case "N":
                            bMetaData = (byte)MetaData.Stairs.South;
                            break;
                        case "E":
                            bMetaData = (byte)MetaData.Stairs.West;
                            break;
                        case "S":
                            bMetaData = (byte)MetaData.Stairs.North;
                            break;
                        case "W":
                            bMetaData = (byte)MetaData.Stairs.East;
                            break;
                        default:
                            return;
                    }
                    break;

                case BlockData.Blocks.Torch:
                    switch (e.Packet.Face)
                    {
                        case BlockFace.Down: return;
                        case BlockFace.Up: bMetaData = (byte)MetaData.Torch.Standing;
                            break;
                        case BlockFace.West: bMetaData = (byte)MetaData.Torch.West;
                            break;
                        case BlockFace.East: bMetaData = (byte)MetaData.Torch.East;
                            break;
                        case BlockFace.North: bMetaData = (byte)MetaData.Torch.North;
                            break;
                        case BlockFace.South: bMetaData = (byte)MetaData.Torch.South;
                            break;
                    }
                    break;
            }

            World.SetBlockAndData(bx, by, bz, bType, bMetaData);
            World.Update(bx, by, bz);

            Inventory.RemoveItem(Inventory.ActiveSlot);
        }

        private void PacketHandler_PlayerDigging(object sender, PacketEventArgs<PlayerDiggingPacket> e)
        {
            int x = e.Packet.X;
            int y = e.Packet.Y;
            int z = e.Packet.Z;

            byte type = World.GetBlockId(x, y, z);
            byte data = World.GetBlockData(x, y, z);

            switch (e.Packet.Action)
            {
                case PlayerDiggingPacket.DigAction.StartDigging:
                    if (BlockData.SingleHit.Contains((BlockData.Blocks)type))
                        goto case PlayerDiggingPacket.DigAction.FinishDigging;
                    break;

                case PlayerDiggingPacket.DigAction.FinishDigging:
                    short give = type;
                    sbyte count = 1;
                    short durability = data;

                    switch ((BlockData.Blocks)type)
                    {
                        case BlockData.Blocks.Adminium:
                            return;

                        case BlockData.Blocks.Air:
                            return;

                        case BlockData.Blocks.Bed:
                            give = (short)BlockData.Items.Bed;
                            break;

                        case BlockData.Blocks.Burning_Furnace:
                            give = (short)BlockData.Blocks.Furnace;
                            break;

                        case BlockData.Blocks.Cake:
                            give = (short)BlockData.Items.Cake;
                            break;

                        case BlockData.Blocks.Clay:
                            give = (short)BlockData.Items.Clay_Balls;
                            break;

                        case BlockData.Blocks.Coal_Ore:
                            give = (short)BlockData.Items.Coal;
                            break;

                        case BlockData.Blocks.Crops:
                            // TODO: Check crops are mature enough before giving items.
                            give = (short)BlockData.Items.Seeds;
                            count = 2;
                            break;

                        case BlockData.Blocks.Diamond_Ore:
                            give = (short)BlockData.Items.Diamond;
                            break;

                        case BlockData.Blocks.Double_Stone_Slab:
                            give = (short)BlockData.Blocks.Stair;
                            break;

                        case BlockData.Blocks.Fire:
                            return;

                        case BlockData.Blocks.Glass:
                            give = -1;
                            break;

                        case BlockData.Blocks.Glowstone:
                            give = (short)BlockData.Items.Lightstone_Dust;
                            break;

                        case BlockData.Blocks.Grass:
                        case BlockData.Blocks.Soil:
                            give = (short)BlockData.Blocks.Dirt;
                            break;

                        case BlockData.Blocks.Gravel:
                            if (Server.Rand.Next(10) == 0)
                                Server.DropItem(World, x, y, z, new ItemStack((short)BlockData.Items.Flint));
                            break;

                        case BlockData.Blocks.Ice:
                            if (BlockData.Air.Contains((BlockData.Blocks)World.GetBlockId(x, y - 1, z)))
                            {
                                World.SetBlockAndData(x, y, z, 0, 0);
                                return;
                            }
                            World.SetBlockAndData(x, y, z, (byte)BlockData.Blocks.Still_Water, 0);
                            return;

                        case BlockData.Blocks.Lapis_Lazuli_Ore:
                            give = (short)BlockData.Items.Ink_Sack;
                            durability = 4;
                            count = (sbyte)(3 + Server.Rand.Next(17));
                            break;

                        case BlockData.Blocks.Lava:
                            return;

                        case BlockData.Blocks.Leaves:
                            give = Server.Rand.Next(5) == 0 ? (short)BlockData.Blocks.Sapling : (short)-1;
                            break;

                        case BlockData.Blocks.Portal:
                        case BlockData.Blocks.Mob_Spawner:
                            give = -1;
                            break;

                        case BlockData.Blocks.Redstone_Ore:
                            give = (short)BlockData.Items.Redstone;
                            count = (sbyte)(2 + Server.Rand.Next(4));
                            break;

                        case BlockData.Blocks.Redstone_Repeater:
                        case BlockData.Blocks.Redstone_Repeater_On:
                            give = (short)BlockData.Items.Redstone_Repeater;
                            break;

                        case BlockData.Blocks.Redstone_Torch_On:
                            give = (short)BlockData.Blocks.Redstone_Torch;
                            break;

                        case BlockData.Blocks.Redstone_Wire:
                            give = (short)BlockData.Items.Redstone;
                            break;

                        case BlockData.Blocks.Sign_Post:
                            give = (short)BlockData.Items.Sign;
                            break;

                        case BlockData.Blocks.Snow:
                            give = (short)BlockData.Items.Snowball;
                            break;

                        case BlockData.Blocks.Snow_Block:
                            give = (short)BlockData.Items.Snowball;
                            count = 3;
                            break;

                        case BlockData.Blocks.Stationary_Lava:
                        case BlockData.Blocks.Stationary_Water:
                            return;

                        case BlockData.Blocks.Stone:
                            give = (short)BlockData.Blocks.Cobblestone;
                            break;

                        case BlockData.Blocks.TNT:
                            // TODO: Spawn TNT Object and start explosion timer.
                            return;

                        case BlockData.Blocks.Wall_Sign:
                            give = (short)BlockData.Items.Sign;
                            break;

                        case BlockData.Blocks.Water:
                            return;
                    }

                    World.SetBlockAndData(x, y, z, 0, 0);
                    World.Update(x, y, z);

                    Inventory.DamageItem(Inventory.ActiveSlot);

                    if (give > 0)
                        Server.DropItem(World, x, y, z, new ItemStack(give, count, durability));
                    break;
            }
        }

        #endregion


        #region Movement and Updates

        private void PacketHandler_Player(object sender, PacketEventArgs<PlayerPacket> e)
        {
            this.OnGround = e.Packet.OnGround;
            this.UpdateEntities();
        }

        private void PacketHandler_PlayerRotation(object sender, PacketEventArgs<PlayerRotationPacket> e)
        {
            this.OnGround = e.Packet.OnGround;
            this.RotateTo(e.Packet.Yaw, e.Packet.Pitch);
            this.UpdateEntities();
        }

        private void PacketHandler_PlayerPositionRotation(object sender, PacketEventArgs<PlayerPositionRotationPacket> e)
        {
            this.OnGround = e.Packet.OnGround;
            this.Stance = e.Packet.Stance;
            this.MoveTo(e.Packet.X, e.Packet.Y, e.Packet.Z, e.Packet.Yaw, e.Packet.Pitch);
        }

        private void PacketHandler_PlayerPosition(object sender, PacketEventArgs<PlayerPositionPacket> e)
        {
            this.OnGround = e.Packet.OnGround;
            this.Stance = e.Packet.Stance;
            this.MoveTo(e.Packet.X, e.Packet.Y, e.Packet.Z);
        }

        #endregion


        #region Login

        private void PacketHandler_Disconnect(object sender, PacketEventArgs<DisconnectPacket> e)
        {
            Logger.Log(Logger.LogLevel.Info, DisplayName + " disconnected: " + e.Packet.Reason);
            Running = false;
        }

        private void PacketHandler_Handshake(object sender, PacketEventArgs<HandshakePacket> e)
        {
            Username = Regex.Replace(e.Packet.UsernameOrHash, Chat.DISALLOWED, "");
            DisplayName = Username;
            SendHandshake();
        }

        private void PacketHandler_LoginRequest(object sender, PacketEventArgs<LoginRequestPacket> e)
        {
            if (!CheckUsername(e.Packet.Username))
                Kick("Inconsistent username");
            else if (e.Packet.ProtocolOrEntityId < ProtocolVersion)
                Kick("Outdated client");
            else
            {
                if (this.Server.UseOfficalAuthentication)
                {
                    try
                    {
                        string authenticated = Http.GetHttpResponse(new Uri(String.Format("http://www.minecraft.net/game/checkserver.jsp?user={0}&serverId={1}", e.Packet.Username, this.Server.ServerHash)));
                        if (authenticated != "YES")
                        {
                            Kick("Authentication failed");
                            return;
                        }
                    }
                    catch (Exception exc)
                    {
                        Kick("Error while authenticating...");
                        this.Logger.Log(exc);
                        return;
                    }
                }

                SendLoginSequence();
            }
        }

        #endregion
    }
}