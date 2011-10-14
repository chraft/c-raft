using System;
using System.Linq;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.Plugins.Events;
using Chraft.Plugins.Events.Args;
using Chraft.World;
using Chraft.Entity;
using System.Text.RegularExpressions;
using Chraft.Utils;
using Chraft.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Chraft.Properties;
using System.Net.Sockets;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.Net
{
    public partial class Client
    {
        DateTime? _inAirStartTime = null;
        /// <summary>
        /// Returns the amount of time since the client was set as in the air
        /// </summary>
        public TimeSpan AirTime
        {
            get
            {
                if (_inAirStartTime == null)
                {
                    return new TimeSpan(0);
                }
                else
                {
                    return DateTime.Now - _inAirStartTime.Value;
                }
            }
        }

        double _beginInAirY = -1;
        double _lastGroundY = -1;
        bool _onGround = false;
        public bool OnGround
        {
            get
            {
                return _onGround;
            }
            set
            {
                if (_onGround != value)
                {
                    _onGround = value;

                    // TODO: For some reason the GetBlockId using an integer will sometime get the block adjacent to where the character is standing therefore falling down near a wall could cause issues (or falling into a 1x1 water might not pick up the water block)
                    BlockData.Blocks currentBlock = (BlockData.Blocks)_Player.World.GetBlockId(UniversalCoords.FromAbsWorld(_Player.Position.X, _Player.Position.Y, _Player.Position.Z));

                    if (!_onGround)
                    {
                        _beginInAirY = _Player.Position.Y;
                        _inAirStartTime = DateTime.Now;
#if DEBUG
                        this.SendMessage("In air");
#endif
                    }
                    else
                    {
#if DEBUG
                        this.SendMessage("On ground");
#endif

                        double blockCount = 0;

                        if (_lastGroundY < _Player.Position.Y)
                        {
                            // We have climbed (using _lastGroundY gives us a more accurate value than using _beginInAirY when climbing)
                            blockCount = (_lastGroundY - _Player.Position.Y);
                        }
                        else
                        {
                            // We have fallen
                            double startY = Math.Max(_lastGroundY, _beginInAirY);
                            blockCount = (startY - _Player.Position.Y);
                        }
                        _lastGroundY = _Player.Position.Y;

                        if (blockCount != 0)
                        {
                            if (blockCount > 0.5)
                            {
#if DEBUG
                                this.SendMessage(String.Format("Fell {0} blocks", blockCount));
#endif
                                double fallDamage = (blockCount - 3);// (we don't devide by two because DamageClient uses whole numbers i.e. 20 = 10 health)

                                #region Adjust based on falling into water
                                // For each sixteen blocks of altitude the water must be one block deep, if the jump altitude is higher as sixteen blocks and the water is only one deep damage is taken from the total altitude minus sixteen (19 is safe i.e. 19-16 = 3 => no damage)
                                // If we are in water, count how many blocks above are also water
                                BlockData.Blocks block = currentBlock;
                                int waterCount = 0;
                                while (BlockHelper.Instance((byte)block).IsLiquid)
                                {
                                    waterCount++;
                                    block = (BlockData.Blocks)_Player.World.GetBlockId((int)_Player.Position.X, (int)_Player.Position.Y + waterCount, (int)_Player.Position.Z);
                                }

                                fallDamage -= waterCount * 16;
                                #endregion

                                if (fallDamage > 0)
                                {
                                    var roundedValue = Convert.ToInt16(Math.Round(fallDamage, 1));
                                    DamageClient(DamageCause.Fall, roundedValue);

                                    if (_Player.Health <= 0)
                                    {
                                        // Make sure that we don't think we have fallen onto the respawn
                                        _lastGroundY = -1;
                                    }
                                }
                            }
                            else if (blockCount < -0.5)
                            {
#if DEBUG
                                this.SendMessage(String.Format("Climbed {0} blocks", blockCount * -1));
#endif
                            }
                        }

                        _beginInAirY = -1;
                    }

                    if (_inAirStartTime != null)
                    {
                        // Check how long in the air for (e.g. flying) - don't count if we are in water
                        if (currentBlock != BlockData.Blocks.Water && currentBlock != BlockData.Blocks.Still_Water && currentBlock != BlockData.Blocks.Stationary_Water && AirTime.TotalSeconds > 5)
                        {
                            // TODO: make the number of seconds configurable
                            Kick("Flying!!");
                        }

                        _inAirStartTime = null;
                    }
                }
            }
        }
        public double Stance { get; set; }

        private readonly object _QueueSwapLock = new object();

        public int TimesEnqueuedForRecv;

        public ByteQueue GetBufferToProcess()
        {
            lock (_QueueSwapLock)
            {
                ByteQueue temp = _CurrentBuffer;
                _CurrentBuffer = _ProcessedBuffer;
                _ProcessedBuffer = temp;
            }

            return _ProcessedBuffer;
        }

        private void Recv_Start()
        {
            if (!Running)
            {
                DisposeRecvSystem();
                return;
            }

            //Logger.Log(Chraft.Logger.LogLevel.Info, "Start receiving");

            try
            {
                bool pending = _Socket.ReceiveAsync(_RecvSocketEvent);

                if (!pending)
                    Recv_Process(_RecvSocketEvent);
            }
            catch (Exception e)
            {
                _Player.Server.Logger.Log(Chraft.Logger.LogLevel.Error, e.Message);
                MarkToDispose();
                DisposeRecvSystem();
            }

        }

        private void Recv_Process(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                lock (_QueueSwapLock)
                    _CurrentBuffer.Enqueue(e.Buffer, 0, e.BytesTransferred);

                int newValue = Interlocked.Increment(ref TimesEnqueuedForRecv);

                if ((newValue - 1) == 0)
                    Server.RecvClientQueue.Enqueue(this);

                _Player.Server.NetworkSignal.Set();

                Recv_Start();
            }
        }

        private void Recv_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (!Running)
                DisposeRecvSystem();
            else
                Recv_Process(e);
        }

        public static void HandlePacketKeepAlive(Client client, KeepAlivePacket packet)
        {
            client.LastClientResponse = DateTime.Now;
            if (client.LastKeepAliveId > 0 && packet.KeepAliveID == client.LastKeepAliveId)
            {
                client.Ping = (int)Math.Round((DateTime.Now - client.KeepAliveStart).TotalMilliseconds, MidpointRounding.AwayFromZero);
            }

        }

        public static void HandlePacketCreativeInventoryAction(Client client, CreativeInventoryActionPacket packet)
        {
            if (client.Owner.GameMode == 1)

                if (packet.ItemID == -1 && packet.Damage == 0 && packet.Quantity == 0) // We are adding an item to our mouse cursor from the quick bar
                {
                    //may need to do something here
                    return;
                } 
                else
                {
                    if (packet.Slot != -1)// mouse cursor mode
                    {
                        client.Owner.Inventory[packet.Slot] = new ItemStack(packet.ItemID, (sbyte)packet.Quantity, packet.Damage);
                    }
                    
                   
                }
            else
                client.Kick("Invalid action: CreativeInventoryAction");
        }

        public static void HandlePacketAnimation(Client client, AnimationPacket packet)
        {
            Player p = client.Owner;
            AbsWorldCoords absCoords = new AbsWorldCoords(p.Position.X, p.Position.Y, p.Position.Z);
            foreach (Client c in p.Server.GetNearbyPlayers(p.World, absCoords))
            {
                if (c == client)
                    continue;
                c.SendPacket(new AnimationPacket
                {
                    Animation = packet.Animation,
                    PlayerId = p.EntityId
                });
            }
        }

        public static void HandlePacketRespawn(Client client, RespawnPacket packet)
        {
            client.Owner.HandleRespawn();
        }

        public static void HandlePacketChatMessage(Client client, ChatMessagePacket packet)
        {
            string clean = Chat.CleanMessage(packet.Message);

            if (clean.StartsWith("/"))
                client.ExecuteCommand(clean.Substring(1));
            else
                client.ExecuteChat(clean);
        }

        public static void HandleTransactionPacket(Client client, TransactionPacket packet)
        {
           //todo-something?
        }

        #region Use

        public static void HandlePacketUseBed(Client client, UseBedPacket packet)
        {
            throw new NotImplementedException();
        }

        public static void HandlePacketUseEntity(Client client, UseEntityPacket packet)
        {
            //Console.WriteLine(e.Packet.Target);
            //this.SendMessage("You are interacting with " + e.Packet.Target + " " + e.Packet.LeftClick);
            Player handledPlayer = client.Owner;
            AbsWorldCoords absCoords = new AbsWorldCoords(handledPlayer.Position.X, handledPlayer.Position.Y, handledPlayer.Position.Z);
            foreach (EntityBase eb in handledPlayer.Server.GetNearbyEntities(handledPlayer.World, absCoords))
            {
                if (eb.EntityId != packet.Target)
                    continue;

                if (eb is Player)
                {
                    Player player = (Player)eb;

                    if (packet.LeftClick)
                    {
                        if (player.Health > 0)
                            player.Client.DamageClient(DamageCause.EntityAttack, 0, handledPlayer);
                    }
                    else
                    {
                        // TODO: Store the object being ridden, so we can update player movement.
                        // This will ride the entity, sends -1 to dismount.
                        foreach (Client cl in handledPlayer.Server.GetNearbyPlayers(handledPlayer.World, absCoords))
                        {
                            cl.SendPacket(new AttachEntityPacket
                            {
                                EntityId = handledPlayer.EntityId,
                                VehicleId = player.EntityId
                            });
                        }
                    }
                }
                else if (eb is Mob)
                {
                    Mob m = (Mob)eb;

                    if (packet.LeftClick)
                    {
                        if (m.Health > 0)
                            m.DamageMob(handledPlayer);
                    }
                    else
                    {
                        // We are interacting with a Mob - tell it what we are using to interact with it
                        m.InteractWith(handledPlayer.Client, handledPlayer.Inventory.ActiveItem);

                        // TODO: move the following to appropriate mob locations
                        // TODO: Check Entity has saddle set.
                        //// This will ride the entity, sends -1 to dismount.
                        //foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
                        //{
                        //    c.PacketHandler.SendPacket(new AttachEntityPacket
                        //    {
                        //        EntityId = this.EntityId,
                        //        VehicleId = c.EntityId
                        //    });
                        //}
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

        public static void HandlePacketWindowClick(Client client, WindowClickPacket packet)
        {
            Interface iface = client.Owner.CurrentInterface ?? client.Owner.Inventory;
            iface.OnClicked(packet);
        }

        public static void HandlePacketCloseWindow(Client client, CloseWindowPacket packet)
        {
            if (client.Owner.CurrentInterface != null)
            {
                client.Owner.CurrentInterface.Close(false);
            }
            else if (client.Owner.Inventory != null && packet.WindowId == client.Owner.Inventory.Handle)
            {
                client.Owner.Inventory.Close(false);
            }
            client.Owner.CurrentInterface = null;
        }

        public static void HandlePacketHoldingChange(Client client, HoldingChangePacket packet)
        {
            client.Owner.Inventory.OnActiveChanged((short)(packet.Slot += 36));

            foreach (Client c in client.Owner.Server.GetNearbyPlayers(client.Owner.World, new AbsWorldCoords(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z)).Where(c => c != client))
            {
                c.SendHoldingEquipment(client);
            }
        }

        #endregion


        #region Block Con/Destruction

        public static void HandlePacketPlayerItemPlacement(Client client, PlayerBlockPlacementPacket packet)
        {
            Player player = client.Owner;
            // if(!Permissions.CanPlayerBuild(Username)) return;
            if (player.Inventory.Slots[player.Inventory.ActiveSlot].Type <= 255)
                return;

            UniversalCoords packetCoords = UniversalCoords.FromAbsWorld(packet.X, packet.Y, packet.Z);

            BlockData.Blocks adjacentBlockType = (BlockData.Blocks)player.World.GetBlockId(packetCoords); // Get block being built against.
            byte adjacentBlockData = player.World.GetBlockData(packetCoords);

            // Placed Item Info
            short pType = player.Inventory.Slots[player.Inventory.ActiveSlot].Type;
            short pMetaData = player.Inventory.Slots[player.Inventory.ActiveSlot].Durability;

            UniversalCoords coordsFromFace = player.World.FromFace(packetCoords, packet.Face);

            switch (packet.Face)
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
                        player.World.SetBlockAndData(packetCoords, (byte)BlockData.Blocks.Soil, 0x00);
                    }
                    break;

                case BlockData.Items.Sign:

                    if (packet.Face == BlockFace.Up) // Floor Sign
                    {
                        // Get the direction the player is facing.
                        switch (client.FacingDirection(8))
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

                        player.World.SetBlockAndData(coordsFromFace, (byte)BlockData.Blocks.Sign_Post, (byte)pMetaData);
                    }
                    else // Wall Sign
                    {
                        switch (packet.Face)
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

                        player.World.SetBlockAndData(coordsFromFace, (byte)BlockData.Blocks.Wall_Sign, (byte)pMetaData);
                    }
                    break;

                case BlockData.Items.Seeds:
                    if (adjacentBlockType == BlockData.Blocks.Soil && packet.Face == BlockFace.Down)
                    {
                        player.World.SetBlockAndData(coordsFromFace, (byte)BlockData.Blocks.Crops, 0x00);
                    }
                    break;
                case BlockData.Items.Reeds:
                    StructBlock block = new StructBlock(coordsFromFace, (byte)BlockData.Blocks.Reed, 0x00, player.World);
                    StructBlock targetBlock = new StructBlock(packetCoords, (byte)adjacentBlockType, adjacentBlockData, player.World);
                    BlockHelper.Instance((byte)BlockData.Blocks.Reed).Place(block, targetBlock, packet.Face);
                    break;
                case BlockData.Items.Redstone:
                    player.World.SetBlockAndData(coordsFromFace, (byte)BlockData.Blocks.Redstone_Wire, 0x00);
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
                        byte blockId = player.World.GetBlockId(coordsFromFace.WorldX, coordsFromFace.WorldY + 1,
                                                               coordsFromFace.WorldZ);
                        if (!BlockHelper.Instance(blockId).IsAir)
                            return;

                        switch (client.FacingDirection(4)) // Built on floor, set by facing dir
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
                            player.World.SetBlockAndData(coordsFromFace.WorldX, coordsFromFace.WorldY + 1, coordsFromFace.WorldZ, (byte)BlockData.Blocks.Iron_Door, (byte)MetaData.Door.IsTopHalf);
                            player.World.SetBlockAndData(coordsFromFace, (byte)BlockData.Blocks.Iron_Door, (byte)pMetaData);
                        }
                        else
                        {
                            player.World.SetBlockAndData(coordsFromFace.WorldX, coordsFromFace.WorldY + 1, coordsFromFace.WorldZ, (byte)BlockData.Blocks.Wooden_Door, (byte)MetaData.Door.IsTopHalf);
                            player.World.SetBlockAndData(coordsFromFace, (byte)BlockData.Blocks.Wooden_Door, (byte)pMetaData);
                        }

                        player.World.Update(UniversalCoords.FromAbsWorld(coordsFromFace.WorldX, coordsFromFace.WorldY + 1, coordsFromFace.WorldZ));
                    }
                    break;
            }
            if (player.GameMode == 0)
            {
                if (!player.Inventory.DamageItem(player.Inventory.ActiveSlot)) // If item isn't durable, remove it.
                    player.Inventory.RemoveItem(player.Inventory.ActiveSlot);
            }

            player.World.Update(coordsFromFace);
        }

        public static void HandlePacketPlayerBlockPlacement(Client client, PlayerBlockPlacementPacket packet)
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

            UniversalCoords coords = UniversalCoords.FromAbsWorld(packet.X, packet.Y, packet.Z);

            if (packet.X == -1 && packet.Y == -1 && packet.Z == -1 && packet.Face == BlockFace.Held)
            {
                // TODO: Implement item usage - food etc
                return;
            }

            Player player = client.Owner;

            BlockData.Blocks type = (BlockData.Blocks)player.World.GetBlockId(coords); // Get block being built against.
            byte metadata = player.World.GetBlockData(coords);
            StructBlock facingBlock = new StructBlock(coords, (byte)type, metadata, player.World);

            UniversalCoords coordsFromFace = player.World.FromFace(coords, packet.Face);

            if (BlockHelper.Instance((byte)type) is IBlockInteractive)
            {
                (BlockHelper.Instance((byte)type) as IBlockInteractive).Interact(player, facingBlock);
                return;
            }

            if (player.Inventory.Slots[player.Inventory.ActiveSlot].Type <= 0 || player.Inventory.Slots[player.Inventory.ActiveSlot].Count < 1)
                return;

            // TODO: Neaten this out, or address via handler?
            if (player.Inventory.Slots[player.Inventory.ActiveSlot].Type > 255 || packet.Face == BlockFace.Held) // Client is using an Item.
            {
                HandlePacketPlayerItemPlacement(client, packet);
                return;
            }

            // Built Block Info

            byte bType = (byte)player.Inventory.Slots[player.Inventory.ActiveSlot].Type;
            byte bMetaData = (byte)player.Inventory.Slots[player.Inventory.ActiveSlot].Durability;

            StructBlock bBlock = new StructBlock(coordsFromFace, bType, bMetaData, player.World);

            BlockHelper.Instance(bType).Place(player, bBlock, facingBlock, packet.Face);
        }

        public static void HandlePacketPlayerDigging(Client client, PlayerDiggingPacket packet)
        {
            Player player = client.Owner;

            UniversalCoords coords = UniversalCoords.FromAbsWorld(packet.X, packet.Y, packet.Z);

            byte type = player.World.GetBlockId(coords);
            byte data = player.World.GetBlockData(coords);

            switch (packet.Action)
            {
                case PlayerDiggingPacket.DigAction.StartDigging:
                    client.SendMessage(String.Format("SkyLight: {0}", player.World.GetSkyLight(coords)));
                    client.SendMessage(String.Format("BlockLight: {0}", player.World.GetBlockLight(coords)));
                    client.SendMessage(String.Format("Opacity: {0}", player.World.GetBlockChunk(coords).GetOpacity(coords)));
                    client.SendMessage(String.Format("Height: {0}", player.World.GetHeight(coords)));
                    client.SendMessage(String.Format("Data: {0}", player.World.GetBlockData(coords)));
                    //this.SendMessage()
                    if (BlockHelper.Instance(type).IsSingleHit)
                        goto case PlayerDiggingPacket.DigAction.FinishDigging;
                    if (BlockHelper.Instance(type) is BlockLeaves && player.Inventory.ActiveItem.Type == (short)BlockData.Items.Shears)
                        goto case PlayerDiggingPacket.DigAction.FinishDigging;
                    if (player.GameMode == 1)
                        goto case PlayerDiggingPacket.DigAction.FinishDigging;
                    break;

                case PlayerDiggingPacket.DigAction.FinishDigging:
                    StructBlock block = new StructBlock(coords, type, data, player.World);
                    BlockHelper.Instance(type).Destroy(player, block);
                    break;

                case PlayerDiggingPacket.DigAction.DropItem:
                    var slot = player.Inventory.ActiveSlot;
                    player.Inventory.RemoveItem(slot);
                    player.DropItem();
                    break;
            }
        }

        #endregion


        #region Movement and Updates

        public static void HandlePacketPlayer(Client client, PlayerPacket packet)
        {
            client.Owner.Ready = true;
            client.OnGround = packet.OnGround;
            client.Owner.UpdateEntities();
        }

        public static void HandlePacketPlayerRotation(Client client, PlayerRotationPacket packet)
        {
            client.Owner.RotateTo(packet.Yaw, packet.Pitch);
            client.OnGround = packet.OnGround;
            client.Owner.UpdateEntities();
        }

        public static void HandlePacketPlayerPositionRotation(Client client, PlayerPositionRotationPacket packet)
        {
            //client.Logger.Log(Chraft.Logger.LogLevel.Info, "Player position: {0} {1} {2}", packet.X, packet.Y, packet.Z);
            client.Owner.MoveTo(new AbsWorldCoords(packet.X, packet.Y - Player.EyeGroundOffset, packet.Z), packet.Yaw, packet.Pitch);
            client.OnGround = packet.OnGround;
            client.Stance = packet.Stance;

            client.CheckAndUpdateChunks(packet.X, packet.Z);
        }

        private double _LastX;
        private double _LastZ;
        private int _MovementsArrived;
        private Task _UpdateChunks;
        private CancellationTokenSource _UpdateChunksToken;

        public static void HandlePacketPlayerPosition(Client client, PlayerPositionPacket packet)
        {
            //client.Logger.Log(Chraft.Logger.LogLevel.Info, "Player position: {0} {1} {2}", packet.X, packet.Y, packet.Z);
            client.Owner.Ready = true;
            client.Owner.MoveTo(new AbsWorldCoords(packet.X, packet.Y, packet.Z));
            client.OnGround = packet.OnGround;
            client.Stance = packet.Stance;

            client.CheckAndUpdateChunks(packet.X, packet.Z);
        }

        public void StopUpdateChunks()
        {
            if (_UpdateChunksToken != null)
                _UpdateChunksToken.Cancel();
        }

        public void ScheduleUpdateChunks()
        {
            _UpdateChunksToken = new CancellationTokenSource();
            var token = _UpdateChunksToken.Token;
            _UpdateChunks = new Task(() => _Player.UpdateChunks(Settings.Default.SightRadius, token), token);
            _UpdateChunks.Start();
        }

        private void CheckAndUpdateChunks(double packetX, double packetZ)
        {
            ++_MovementsArrived;

            if (_MovementsArrived % 8 == 0)
            {
                double distance = Math.Pow(Math.Abs(packetX - _LastX), 2.0) + Math.Pow(Math.Abs(packetZ - _LastZ), 2.0);
                _MovementsArrived = 0;
                if (distance > 16 && (_UpdateChunks == null || _UpdateChunks.IsCompleted))
                {
                    _LastX = packetX;
                    _LastZ = packetZ;
                    ScheduleUpdateChunks();
                }
            }
        }
        #endregion

        #region Login

        public static void HandlePacketServerListPing(Client client, ServerListPingPacket packet)
        {
            // Received a ServerListPing, so send back Disconnect with the Reason string containing data (server description, number of users, number of slots), delimited by a §
            var clientCount = client.Owner.Server.GetAuthenticatedClients().Count();
            //client.SendPacket(new DisconnectPacket() { Reason = String.Format("{0}§{1}§{2}", client.Owner.Server.ToString(), clientCount, Chraft.Properties.Settings.Default.MaxPlayers) });
            client.Kick(String.Format("{0}§{1}§{2}", client.Owner.Server.ToString(), clientCount, Chraft.Properties.Settings.Default.MaxPlayers));
        }

        public static void HandlePacketDisconnect(Client client, DisconnectPacket packet)
        {
            client.Logger.Log(Logger.LogLevel.Info, client.Owner.DisplayName + " disconnected: " + packet.Reason);
            client.Stop();
        }

        public static void HandlePacketHandshake(Client client, HandshakePacket packet)
        {
            client.Owner.Username = Regex.Replace(packet.UsernameOrHash, Chat.DISALLOWED, "");
            client.Owner.DisplayName = client.Owner.Username;
            client.SendHandshake();
        }

        public static void HandlePacketLoginRequest(Client client, LoginRequestPacket packet)
        {
            if (!client.Owner.CheckUsername(packet.Username))
                client.Kick("Inconsistent username");
            else if (packet.ProtocolOrEntityId < ProtocolVersion)
                client.Kick("Outdated client");
            else
            {
                if (client.Owner.Server.UseOfficalAuthentication)
                {
                    try
                    {
                        string authenticated = Http.GetHttpResponse(new Uri(String.Format("http://www.minecraft.net/game/checkserver.jsp?user={0}&serverId={1}", packet.Username, client.Owner.Server.ServerHash)));
                        if (authenticated != "YES")
                        {
                            client.Kick("Authentication failed");
                            return;
                        }
                    }
                    catch (Exception exc)
                    {
                        client.Kick("Error while authenticating...");
                        client.Logger.Log(exc);
                        return;
                    }
                }

                client.SendLoginSequence();
            }
        }

        #endregion
    }
}
