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
using System;
using System.Linq;
using Chraft.Net.Packets;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;
using Chraft.World;
using Chraft.Entity;
using System.Text.RegularExpressions;
using Chraft.Utils;
using Chraft.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Chraft.Utilities.Config;
using System.Net.Sockets;
using Chraft.World.Blocks;
using Chraft.PluginSystem;
using Chraft.World.Blocks.Base;

namespace Chraft.Net
{
    public partial class Client : IClient
    {
        DateTime? _inAirStartTime = null;
        /// <summary>
        /// Returns the amount of time since the client was set as in the air
        /// </summary>
        internal TimeSpan AirTime
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
            internal set
            {
                if (_onGround != value)
                {
                    _onGround = value;
                   
                    byte? blockId = _player.World.GetBlockId(UniversalCoords.FromAbsWorld(_player.Position));

                    if (blockId == null)
                        return;

                    BlockData.Blocks currentBlock = (BlockData.Blocks) blockId;

                    if (!_onGround)
                    {
                        _beginInAirY = _player.Position.Y;
                        _inAirStartTime = DateTime.Now;
#if DEBUG
                        //this.SendMessage("In air");
#endif
                    }
                    else
                    {
#if DEBUG
                        //this.SendMessage("On ground");
#endif

                        double blockCount = 0;

                        if (_lastGroundY < _player.Position.Y)
                        {
                            // We have climbed (using _lastGroundY gives us a more accurate value than using _beginInAirY when climbing)
                            blockCount = (_lastGroundY - _player.Position.Y);
                        }
                        else
                        {
                            // We have fallen
                            double startY = Math.Max(_lastGroundY, _beginInAirY);
                            blockCount = (startY - _player.Position.Y);
                        }
                        _lastGroundY = _player.Position.Y;

                        if (blockCount != 0)
                        {
                            if (blockCount > 0.5)
                            {
#if DEBUG
                                //this.SendMessage(String.Format("Fell {0} blocks", blockCount));
#endif
                                double fallDamage = (blockCount - 3);// (we don't devide by two because DamageClient uses whole numbers i.e. 20 = 10 health)

                                #region Adjust based on falling into water
                                // For each sixteen blocks of altitude the water must be one block deep, if the jump altitude is higher as sixteen blocks and the water is only one deep damage is taken from the total altitude minus sixteen (19 is safe i.e. 19-16 = 3 => no damage)
                                // If we are in water, count how many blocks above are also water
                                BlockData.Blocks block = currentBlock;
                                int waterCount = 0;
                                while (BlockHelper.Instance.IsLiquid((byte)block))
                                {
                                    waterCount++;
                                    block = (BlockData.Blocks)_player.World.GetBlockId((int)_player.Position.X, (int)_player.Position.Y + waterCount, (int)_player.Position.Z);
                                }

                                fallDamage -= waterCount * 16;
                                #endregion

                                if (fallDamage > 0)
                                {
                                    var roundedValue = Convert.ToInt16(Math.Round(fallDamage, 1));
                                    Owner.Damage(DamageCause.Fall, roundedValue);

                                    if (_player.Health <= 0)
                                    {
                                        // Make sure that we don't think we have fallen onto the respawn
                                        _lastGroundY = -1;
                                    }
                                }
                            }
                            else if (blockCount < -0.5)
                            {
#if DEBUG
                                //this.SendMessage(String.Format("Climbed {0} blocks", blockCount * -1));
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
        public double Stance { get; internal set; }

        private readonly object _QueueSwapLock = new object();

        public int TimesEnqueuedForRecv;

        internal ByteQueue GetBufferToProcess()
        {
            lock (_QueueSwapLock)
            {
                ByteQueue temp = _currentBuffer;
                _currentBuffer = _processedBuffer;
                _processedBuffer = temp;
            }

            return _processedBuffer;
        }

        private void Recv_Start()
        {
            if (!Running)
            {
                DisposeRecvSystem();
                return;
            }
            
            if(!_socket.Connected)
            {
                Stop();
                return;
            }

            try
            {
                bool pending = _socket.ReceiveAsync(_recvSocketEvent);

                if (!pending)
                    Recv_Completed(null, _recvSocketEvent);
            }
            catch (Exception e)
            {
                Server.Logger.Log(LogLevel.Error, e.Message);
                Stop();
            }

        }

        private void Recv_Process(SocketAsyncEventArgs e)
        {         
            lock (_QueueSwapLock)
                _currentBuffer.Enqueue(e.Buffer, 0, e.BytesTransferred);

            int newValue = Interlocked.Increment(ref TimesEnqueuedForRecv);

            if ((newValue - 1) == 0)
                Server.RecvClientQueue.Enqueue(this);

            Server.NetworkSignal.Set();

            Recv_Start();       
        }

        private void Recv_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (!Running)
                DisposeRecvSystem();
            else if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                Client client;
                if (Server.AuthClients.TryGetValue(SessionID, out client))
                {
                    MarkToDispose();
                    DisposeRecvSystem();
                }
                _nextActivityCheck = DateTime.MinValue;
                //Logger.Log(LogLevel.Error, "Error receiving: {0}", e.SocketError);
            }
            else
            {
                if (DateTime.Now + TimeSpan.FromSeconds(5) > _nextActivityCheck)
                    _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(5);
                Recv_Process(e);
            }
        }

#region Misc
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

                if (packet.Item.Type == -1 && packet.Item.Durability == 0 && packet.Item.Count == 0) // We are adding an item to our mouse cursor from the quick bar
                {
                    //may need to do something here
                    return;
                }
                else
                {
                    if (packet.Slot != -1)// mouse cursor mode
                    {
                        client.Owner.Inventory[packet.Slot] = packet.Item;
                    }


                }
            else
                client.Kick("Invalid action: CreativeInventoryAction");
        }

        public static void HandlePacketAnimation(Client client, AnimationPacket packet)
        {
            Player p = client.Owner;

            UniversalCoords coords = UniversalCoords.FromAbsWorld(p.Position);
            foreach (Client c in p.Server.GetNearbyPlayersInternal(p.World, coords))
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

        public static void HandlePacketEnchantItem(Client client, EnchantItemPacket packet)
        {
            // TODO: Implement item enchantment
        }

        public static void HandlePacketPlayerActivites(Client client, PlayerAbilitiesPacket packet)
        {
            //TODO : Implement player abilities.
        }

#endregion

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
            UniversalCoords coords = UniversalCoords.FromAbsWorld(handledPlayer.Position);
            foreach (EntityBase eb in handledPlayer.Server.GetNearbyEntitiesInternal(handledPlayer.World, coords))
            {
                if (eb.EntityId != packet.Target)
                    continue;

                if (eb is Player)
                {
                    Player player = (Player)eb;

                    if (packet.LeftClick)
                    {
                        handledPlayer.Attack(player);
                    }
                    else
                    {
                        // TODO: Store the object being ridden, so we can update player movement.
                        // This will ride the entity, sends -1 to dismount.
                        foreach (Client cl in handledPlayer.Server.GetNearbyPlayersInternal(handledPlayer.World, coords))
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
                        handledPlayer.Attack(m);
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
            client.Owner.Inventory.OnActiveChanged((packet.Slot += 36));

            foreach (Client c in client.Owner.Server.GetNearbyPlayersInternal(client.Owner.World, UniversalCoords.FromAbsWorld(client.Owner.Position)).Where(c => c != client))
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

            UniversalCoords baseBlockCoords = UniversalCoords.FromWorld(packet.X, packet.Y, packet.Z);

            Chunk chunk = player.World.GetChunk(baseBlockCoords) as Chunk;

            if (chunk == null)
                return;

            BlockData.Blocks baseBlockType = chunk.GetType(baseBlockCoords); // Get block being built against.
            byte baseBlockData = chunk.GetData(baseBlockCoords);

            StructBlock baseBlock = new StructBlock(baseBlockCoords, (byte)baseBlockType, (byte)baseBlockData, player.World);

            // Placed Item Info
            short pType = player.Inventory.Slots[player.Inventory.ActiveSlot].Type;
            short pMetaData = player.Inventory.Slots[player.Inventory.ActiveSlot].Durability;

            UniversalCoords newBlockCoords = player.World.FromFace(baseBlockCoords, packet.Face);
            StructBlock newBlock;
            byte newBlockId = 0;

            switch (packet.Face)
            {
                case BlockFace.Held:
                    return; // TODO: Process buckets, food, etc.
            }

            switch (baseBlockType)
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
                    if (baseBlockType == BlockData.Blocks.Dirt || baseBlockType == BlockData.Blocks.Grass)
                    {
                        // Think the client has a Notch bug where hoe's durability is not updated properly.
                        BlockHelper.Instance.CreateBlockInstance((byte)BlockData.Blocks.Soil).Spawn(baseBlock);
                    }
                    return;
                case BlockData.Items.Ink_Sack:
                    if (pMetaData != 15)
                        return;
                    if (baseBlockType == BlockData.Blocks.Red_Mushroom || baseBlockType == BlockData.Blocks.Brown_Mushroom)
                    {
                        BlockBaseMushroom baseMushroom = (BlockBaseMushroom)BlockHelper.Instance.CreateBlockInstance((byte)baseBlockType);
                        baseMushroom.Fertilize(player, baseBlock);
                    }
                    return;
                case BlockData.Items.Minecart:
                case BlockData.Items.Boat:
                case BlockData.Items.Storage_Minecart:
                case BlockData.Items.Powered_Minecart:
                    // TODO: Create new object
                    break;
                case BlockData.Items.Sign:
                    if (packet.Face == BlockFace.Up)
                        newBlockId = (byte)BlockData.Blocks.Sign_Post;
                    else
                        newBlockId = (byte)BlockData.Blocks.Wall_Sign;
                    break;
                case BlockData.Items.Seeds:
                    newBlockId = (byte)BlockData.Blocks.Crops;
                    break;
                case BlockData.Items.Reeds:
                    newBlockId = (byte)BlockData.Blocks.Reed;
                    break;
                case BlockData.Items.Redstone:
                    newBlockId = (byte)BlockData.Blocks.Redstone_Wire;
                    break;
                case BlockData.Items.Iron_Door:
                    newBlockId = (byte) BlockData.Blocks.Iron_Door;
                    break;
                case BlockData.Items.Wooden_Door:
                    newBlockId = (byte)BlockData.Blocks.Wooden_Door;
                    break;
            }

            if (newBlockId != 0)
            {
                newBlock = new StructBlock(newBlockCoords, newBlockId, 0, player.World);
                BlockHelper.Instance.CreateBlockInstance(newBlockId).Place(player, newBlock, baseBlock, packet.Face);
            }
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

            UniversalCoords coords = UniversalCoords.FromWorld(packet.X, packet.Y, packet.Z);

            if (packet.X == -1 && packet.Y == -1 && packet.Z == -1 && packet.Face == BlockFace.Held)
            {
                // TODO: Implement item usage - food etc
                return;
            }

            Player player = client.Owner;

            Chunk chunk = player.World.GetChunk(coords) as Chunk;

            if (chunk == null)
                return;

            BlockData.Blocks type = chunk.GetType(coords); // Get block being built against.
            byte metadata = chunk.GetData(coords);
            StructBlock facingBlock = new StructBlock(coords, (byte)type, metadata, player.World);

            UniversalCoords coordsFromFace = player.World.FromFace(coords, packet.Face);

            if (BlockHelper.Instance.CreateBlockInstance((byte)type) is IBlockInteractive)
            {
                (BlockHelper.Instance.CreateBlockInstance((byte)type) as IBlockInteractive).Interact(player, facingBlock);
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

            BlockHelper.Instance.CreateBlockInstance(bType).Place(player, bBlock, facingBlock, packet.Face);
        }

        public static void HandlePacketPlayerDigging(Client client, PlayerDiggingPacket packet)
        {
            Player player = client.Owner;

            UniversalCoords coords = UniversalCoords.FromWorld(packet.X, packet.Y, packet.Z);

            Chunk chunk = player.World.GetChunk(coords) as Chunk;

            if (chunk == null)
                return;

            byte type = (byte)chunk.GetType(coords);
            byte data = chunk.GetData(coords);

            switch (packet.Action)
            {
                case PlayerDiggingPacket.DigAction.StartDigging:
#if DEBUG
                    UniversalCoords oneUp = UniversalCoords.FromWorld(coords.WorldX, coords.WorldY + 1, coords.WorldZ);
                    client.SendMessage(String.Format("SkyLight: {0}", player.World.GetSkyLight(oneUp)));
                    client.SendMessage(String.Format("BlockLight: {0}", player.World.GetBlockLight(oneUp)));
                    client.SendMessage(String.Format("Opacity: {0}", player.World.GetChunk(oneUp, false, false).GetOpacity(oneUp)));
                    client.SendMessage(String.Format("Height: {0}", player.World.GetHeight(oneUp)));
                    client.SendMessage(String.Format("Data: {0}", player.World.GetBlockData(oneUp)));
                    //this.SendMessage()
#endif
                    if (BlockHelper.Instance.IsSingleHit(type))
                        goto case PlayerDiggingPacket.DigAction.FinishDigging;
                    if (BlockHelper.Instance.CreateBlockInstance(type) is BlockLeaves && player.Inventory.ActiveItem.Type == (short)BlockData.Items.Shears)
                        goto case PlayerDiggingPacket.DigAction.FinishDigging;
                    if (player.GameMode == 1)
                        goto case PlayerDiggingPacket.DigAction.FinishDigging;
                    break;

                case PlayerDiggingPacket.DigAction.FinishDigging:
                    StructBlock block = new StructBlock(coords, type, data, player.World);
                    BlockHelper.Instance.CreateBlockInstance(type).Destroy(player, block);
                    break;

                case PlayerDiggingPacket.DigAction.DropItem:
                    player.DropActiveSlotItem();
                    break;
            }
        }

        #endregion


        #region Movement and Updates

        public static void HandlePacketPlayer(Client client, PlayerPacket packet)
        {
            client.Owner.Ready = true;
            client.OnGround = packet.OnGround;
        }

        public static void HandlePacketPlayerRotation(Client client, PlayerRotationPacket packet)
        {
            client.Owner.RotateTo(packet.Yaw, packet.Pitch);
            client.OnGround = packet.OnGround;
            client.Owner.UpdateEntities();
        }

        public static void HandlePacketPlayerPositionRotation(Client client, PlayerPositionRotationPacket packet)
        {
            double feetY = packet.Y - client.Owner.EyeHeight;
            if(client.WaitForInitialPosAck)
            {
                AbsWorldCoords coords = new AbsWorldCoords(packet.X, feetY, packet.Z);
                if(coords == client.Owner.LoginPosition)
                {
                    client.WaitForInitialPosAck = false;
                    client.SendSecondLoginSequence();
                }
            }
            else
            {
                double threshold = 0.001;
                double diffX = Math.Abs(client.Owner.Position.X - packet.X);
                double diffY = Math.Abs(client.Owner.Position.Y - feetY);
                double diffZ = Math.Abs(client.Owner.Position.Z - packet.Z);
                if (diffX < threshold && diffY < threshold && diffZ < threshold)
                    return;

                client.Owner.MoveTo(new AbsWorldCoords(packet.X, feetY, packet.Z), packet.Yaw, packet.Pitch);
                client.OnGround = packet.OnGround;
                client.Stance = packet.Stance;

                client.CheckAndUpdateChunks(packet.X, packet.Z);
            }
        }

        public static void HandlePacketEntityAction(Client client, EntityActionPacket packet)
        {
            switch (packet.Action)
            {
                case EntityActionPacket.ActionType.Crouch:
                    client.Owner.StartCrouching();
                    break;
                case EntityActionPacket.ActionType.Uncrouch:
                    client.Owner.StopCrouching();
                    break;
                case EntityActionPacket.ActionType.StartSprinting:
                    client.Owner.StartSprinting();
                    break;
                case EntityActionPacket.ActionType.StopSprinting:
                    client.Owner.StopSprinting();
                    break;
                default:
                    break;
            }
        }

        private double _lastX;
        private double _lastZ;
        private int _movementsArrived;
        private Task _updateChunks;
        private CancellationTokenSource _updateChunksToken;

        public static void HandlePacketPlayerPosition(Client client, PlayerPositionPacket packet)
        {
            if(client.WaitForInitialPosAck)
                return;

            //client.Logger.Log(Chraft.LogLevel.Info, "Player position: {0} {1} {2}", packet.X, packet.Y, packet.Z);
            client.Owner.Ready = true;
            double threshold = 0.001;
            double diffX = Math.Abs(client.Owner.Position.X - packet.X);
            double diffY = Math.Abs(client.Owner.Position.Y - packet.Y);
            double diffZ = Math.Abs(client.Owner.Position.Z - packet.Z);
            if (diffX < threshold && diffY < threshold && diffZ < threshold)
                return;

            client.Owner.MoveTo(new AbsWorldCoords(packet.X, packet.Y, packet.Z));
            client.OnGround = packet.OnGround;
            client.Stance = packet.Stance;

            client.CheckAndUpdateChunks(packet.X, packet.Z);
        }

        public static void HandlePacketUpdateSign(Client client, UpdateSignPacket packet)
        {
            BlockData.Blocks blockId = (BlockData.Blocks)client.Owner.World.GetBlockId(packet.X, packet.Y, packet.Z);

            UniversalCoords coords = UniversalCoords.FromWorld(packet.X, packet.Y, packet.Z);
            if (blockId == BlockData.Blocks.Sign_Post)
            {
                BlockSignPost sign = (BlockSignPost)BlockHelper.Instance.CreateBlockInstance((byte)blockId);
                sign.SaveText(coords, client.Owner, packet.Lines);
            }
        }

        internal void StopUpdateChunks()
        {
            if (_updateChunksToken != null)
            {
                _updateChunksToken.Cancel();
            }
        }

        internal void ScheduleUpdateChunks()
        {
            _updateChunksToken = new CancellationTokenSource();
            var token = _updateChunksToken.Token;
            _updateChunks = Task.Factory.StartNew(() => _player.UpdateChunks(ChraftConfig.SightRadius, token), token);
        }

        private void CheckAndUpdateChunks(double packetX, double packetZ)
        {
            ++_movementsArrived;

            if (_movementsArrived % 8 == 0)
            {
                double distance = Math.Pow(Math.Abs(packetX - _lastX), 2.0) + Math.Pow(Math.Abs(packetZ - _lastZ), 2.0);
                _movementsArrived = 0;
                if (distance > 16)
                {
                    Owner.MarkToSave();
                    if (_updateChunks == null || _updateChunks.IsCompleted)
                    {
                        _lastX = packetX;
                        _lastZ = packetZ;
                        ScheduleUpdateChunks();
                    }
                }
            }
        }
        #endregion

        #region Login

        public static void HandlePacketServerListPing(Client client, ServerListPingPacket packet)
        {
            // Received a ServerListPing, so send back Disconnect with the Reason string containing data (server description, number of users, number of slots), delimited by a §
            var clientCount = client.Server.GetAuthenticatedClients().Count();
            //client.SendPacket(new DisconnectPacket() { Reason = String.Format("{0}§{1}§{2}", client.Owner.Server.ToString(), clientCount, Chraft.Properties.ChraftConfig.MaxPlayers) });
            client.Kick(String.Format("{0}§{1}§{2}", client.Server, clientCount, ChraftConfig.MaxPlayers));
        }

        public static void HandlePacketDisconnect(Client client, DisconnectPacket packet)
        {
            client.Logger.Log(LogLevel.Info, client.Owner.DisplayName + " disconnected: " + packet.Reason);
            client.Stop();
        }

        public static void HandlePacketHandshake(Client client, HandshakePacket packet)
        {
            var usernameHost = Regex.Replace(packet.UsernameAndIpOrHash, Chat.DISALLOWED, "").Split(';');
            client.Username = usernameHost[0];
            client.Host = usernameHost[1];
            client.SendHandshake();
        }

        public static void HandlePacketLoginRequest(Client client, LoginRequestPacket packet)
        {
            if (!client.CheckUsername(packet.Username))
                client.Kick("Inconsistent username");
            else if (packet.ProtocolOrEntityId < ProtocolVersion)
                client.Kick("Outdated client");
            else
            {
                if (client.Server.UseOfficalAuthentication)
                {
                    try
                    {
                        string authenticated = Http.GetHttpResponse(new Uri(String.Format("http://www.minecraft.net/game/checkserver.jsp?user={0}&serverId={1}", packet.Username, client.Server.ServerHash)));
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

                Task.Factory.StartNew(client.SendLoginSequence);
            }
        }

        #endregion
    }
}
