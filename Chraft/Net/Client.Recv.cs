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
using System.Security.Cryptography;
using Chraft.Entity.Items;
using Chraft.Net.Packets;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;
using Chraft.World;
using Chraft.Entity;
using Chraft.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Chraft.Utilities.Config;
using System.Net.Sockets;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;
using System.Text;

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
                return DateTime.Now - _inAirStartTime.Value;
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

                    BlockData.Blocks currentBlock = (BlockData.Blocks)blockId;

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

                        if (Owner.GetMetaData().IsSprinting)
                            Owner.Exhaustion += 800;
                        else
                            Owner.Exhaustion += 200;

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
                        if (currentBlock != BlockData.Blocks.Water && currentBlock != BlockData.Blocks.Still_Water && currentBlock != BlockData.Blocks.Lava && currentBlock != BlockData.Blocks.Still_Lava && AirTime.TotalSeconds > 5)
                        {
                            // TODO: make the number of seconds configurable
                            Kick("Flying!!");
                        }

                        _inAirStartTime = null;
                    }
                }
            }
        }

        public int CurrentSightRadius { get; set; }

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

            if (!_socket.Connected)
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
            if (client.Owner.GameMode == GameMode.Creative)

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

        public static void HandlePacketPlayerBlockPlacement(Client client, PlayerBlockPlacementPacket packet)
        {
            var baseBlockcoords = UniversalCoords.FromWorld(packet.X, packet.Y, packet.Z);
            var player = client.Owner;

            // Consume food, charge the bow etc
            if (packet.X == -1 && packet.Y == -1 && packet.Z == -1 && packet.Face == BlockFace.Held)
            {
                if (ItemHelper.IsVoid(player.Inventory.ActiveItem))
                    return;

                if (player.Inventory.ActiveItem is IItemConsumable)
                {
                    var consumable = player.Inventory.ActiveItem as IItemConsumable;
                    consumable.StartConsuming();
                }
                return;
            }

            var chunk = player.World.GetChunk(baseBlockcoords) as Chunk;

            if (chunk == null)
                return;

            var baseBlockType = chunk.GetType(baseBlockcoords); // Get block being built against.
            byte baseBlockMeta = chunk.GetData(baseBlockcoords);
            var baseBlock = new StructBlock(baseBlockcoords, (byte)baseBlockType, baseBlockMeta, player.World);


            // Interaction with the blocks - chest, furnace, enchantment table etc
            if (BlockHelper.Instance.IsInteractive(baseBlockType))
            {
                (BlockHelper.Instance.CreateBlockInstance(baseBlock.Type) as IBlockInteractive).Interact(client.Owner, baseBlock);
                return;
            }

            if (ItemHelper.IsVoid(player.Inventory.ActiveItem))
                return;


            if (player.Inventory.ActiveItem is IItemUsable)
            {
                var consumable = player.Inventory.ActiveItem as IItemUsable;
                consumable.Use(baseBlock, packet.Face);
                //HandlePacketPlayerItemPlacement(client, packet);
            }

            if (player.Inventory.ActiveItem is IItemPlaceable)
            {
                var consumable = player.Inventory.ActiveItem as IItemPlaceable;
                consumable.Place(baseBlock, packet.Face);
            }
        }

        public static void HandlePacketPlayerDigging(Client client, PlayerDiggingPacket packet)
        {
            var player = client.Owner;

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
                    if (player.GameMode == GameMode.Creative)
                        goto case PlayerDiggingPacket.DigAction.FinishDigging;
                    break;
                case PlayerDiggingPacket.DigAction.CancelledDigging:
                    break;
                case PlayerDiggingPacket.DigAction.FinishDigging:
                    var block = new StructBlock(coords, type, data, player.World);
                    player.Exhaustion += 25;
                    player.Inventory.ActiveItem.DestroyBlock(block);
                    break;

                case PlayerDiggingPacket.DigAction.DropItem:
                    player.DropActiveSlotItem();
                    break;
                case PlayerDiggingPacket.DigAction.ShootArrow: // Finish eating food too
                    client.Owner.FinishUseActiveSlotItem();
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
            if (client.WaitForInitialPosAck)
            {
                AbsWorldCoords coords = new AbsWorldCoords(packet.X, feetY, packet.Z);
                if (coords == client.Owner.LoginPosition)
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
            if (client.WaitForInitialPosAck)
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

        public static void HandlePacketLocaleAndViewDistance(Client client, ClientSettingsPacket packet)
        {
            if (packet.ViewDistance < ChraftConfig.MaxSightRadius)
                client.CurrentSightRadius = packet.ViewDistance;
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
            int sightRadius = ChraftConfig.MaxSightRadius;

            if (Server.EnableUserSightRadius)
                sightRadius = CurrentSightRadius > ChraftConfig.MaxSightRadius ? ChraftConfig.MaxSightRadius : CurrentSightRadius;

            _updateChunks = Task.Factory.StartNew(() => _player.UpdateChunks(sightRadius, token), token);
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

        public static bool IsAuthenticated(Client client)
        {
            if (client.Server.UseOfficalAuthentication)
            {
                try
                {
                    var uri = new Uri(
                        String.Format(
                            "http://session.minecraft.net/game/checkserver.jsp?user={0}&serverId={1}",
                            client.Username,
                        // As per http://mc.kev009.com/Protocol_Encryption
                            PacketCryptography.JavaHexDigest(Encoding.UTF8.GetBytes(client.ConnectionId)
                                                                            .Concat(client.SharedKey)
                                                                            .Concat(PacketCryptography.PublicKeyToAsn1(client.Server.ServerKey))
                                                                            .ToArray())
                            ));

                    string authenticated = Http.GetHttpResponse(uri);
                    if (authenticated != "YES")
                    {
                        client.Kick("Authentication failed");
                        return false;
                    }
                }
                catch (Exception exc)
                {
                    client.Kick("Error while authenticating...");
                    client.Logger.Log(exc);
                    return false;
                }

                return true;
            }

            return true;
        }

        public static void HandlePacketServerListPing(Client client, ServerListPingPacket packet)
        {
            // Received a ServerListPing, so send back Disconnect with the Reason string containing data (server description, number of users, number of slots), delimited by a §
            var clientCount = client.Server.GetAuthenticatedClients().Count();
            client.Kick(String.Format("§1\0{0}\0{1}\0{2}\0{3}\0{4}", ProtocolVersion, MinecraftServerVersion, ChraftConfig.MOTD, clientCount, ChraftConfig.MaxPlayers));
        }

        public static void HandlePacketDisconnect(Client client, DisconnectPacket packet)
        {
            client.Logger.Log(LogLevel.Info, client.Owner.DisplayName + " disconnected: " + packet.Reason);
            client.Stop();
        }

        public static void HandlePacketHandshake(Client client, HandshakePacket packet)
        {
            client.Username = packet.Username;
            if (!client.CheckUsername(packet.Username))
                client.Kick("Inconsistent username");
            if (packet.ProtocolVersion < ProtocolVersion)
                client.Kick("Outdated client");
            else
            {
                client.Host = packet.ServerHost + ":" + packet.ServerPort;

                if (client.Server.EncryptionEnabled)
                    client.SendEncryptionRequest();
                else if (IsAuthenticated(client))
                    Task.Factory.StartNew(client.SendLoginSequence);

            }
        }

        public static void HandlePacketClientStatus(Client client, ClientStatusPacket packet)
        {
            Console.WriteLine("Status arrived {0}", packet.Status);
            if (packet.Status == 0 && IsAuthenticated(client))
                Task.Factory.StartNew(client.SendLoginSequence);
        }

        public static void HandlePacketEncryptionResponse(Client client, EncryptionKeyResponse packet)
        {
            client.SharedKey = PacketCryptography.Decrypt(packet.SharedSecret);

            RijndaelManaged recv = PacketCryptography.GenerateAES(client.SharedKey);
            RijndaelManaged send = PacketCryptography.GenerateAES(client.SharedKey);

            client.Decrypter = recv.CreateDecryptor();
            
            byte[] packetToken = PacketCryptography.Decrypt(packet.VerifyToken);

            if (!packetToken.SequenceEqual(PacketCryptography.VerifyToken))
            {
                client.Kick("Wrong token");
                return;
            }

            client.Send_Sync_Packet(new EncryptionKeyResponse());

            client.Encrypter = send.CreateEncryptor();

        }

        public static void HandleTabCompletePacket(Client client, TabCompletePacket packet)
        {
            var str = new StringBuilder();
            if (string.IsNullOrEmpty(packet.Text.Trim()))
                return;

            if (packet.Text.StartsWith("/"))
            {
                str.Append(client.Server.ClientCommandHandler.AutoComplete(client, packet.Text));
                if (!string.IsNullOrEmpty(str.ToString()))
                    client.Send_Sync_Packet(new TabCompletePacket { Text = str.ToString() });
                return;
            }
            str.Append(PluginSystem.Commands.AutoComplete.GetPlayers(client, packet.Text));
            if (string.IsNullOrEmpty(str.ToString()))
                return;
            client.Send_Sync_Packet(new TabCompletePacket { Text = str.ToString() });
        }

        #endregion
    }
}
