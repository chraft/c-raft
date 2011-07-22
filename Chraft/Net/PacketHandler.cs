using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace Chraft.Net
{
	public class PacketHandler : IDisposable
	{
		public BigEndianStream Net { get; private set; }
		public Server Server { get; private set; }
		public Logger Logger { get { return Server.Logger; } }

		public event PacketEventHandler<AddObjectVehiclePacket> AddObjectVehicle;
		public event PacketEventHandler<AnimationPacket> Animation;
		public event PacketEventHandler<AttachEntityPacket> AttachEntity;
		public event PacketEventHandler<BlockChangePacket> BlockChange;
		public event PacketEventHandler<ChatMessagePacket> ChatMessage;
		public event PacketEventHandler<CloseWindowPacket> CloseWindow;
		public event PacketEventHandler<CollectItemPacket> CollectItem;
		public event PacketEventHandler<DestroyEntityPacket> DestroyEntity;
		public event PacketEventHandler<DisconnectPacket> Disconnect;
		public event PacketEventHandler<CreateEntityPacket> Entity;
		public event PacketEventHandler<EntityActionPacket> EntityAction;
		public event PacketEventHandler<EntityEquipmentPacket> EntityEquipment;
		public event PacketEventHandler<EntityLookPacket> EntityLook;
		public event PacketEventHandler<EntityLookAndRelativeMovePacket> EntityLookAndRelativeMove;
		public event PacketEventHandler<EntityMetadataPacket> EntityMetadata;
		public event PacketEventHandler<EntityPaintingPacket> EntityPainting;
		public event PacketEventHandler<EntityRelativeMovePacket> EntityRelativeMove;
		public event PacketEventHandler<EntityStatusPacket> EntityStatus;
		public event PacketEventHandler<EntityTeleportPacket> EntityTeleport;
		public event PacketEventHandler<EntityVelocityPacket> EntityVelocity;
		public event PacketEventHandler<ExplosionPacket> Explosion;
		public event PacketEventHandler<HandshakePacket> Handshake;
		public event PacketEventHandler<HoldingChangePacket> HoldingChange;
		public event PacketEventHandler<IncrementStatisticPacket> IncrementStatistic;
		public event PacketEventHandler<InvalidStatePacket> InvalidState;
		public event PacketEventHandler<KeepAlivePacket> KeepAlive;
		public event PacketEventHandler<LoginRequestPacket> LoginRequest;
		public event PacketEventHandler<MapChunkPacket> MapChunk;
		public event PacketEventHandler<MobSpawnPacket> MobSpawn;
		public event PacketEventHandler<MultiBlockChangePacket> MultiBlockChange;
		public event PacketEventHandler<NamedEntitySpawnPacket> NamedEntitySpawn;
		public event PacketEventHandler<OpenWindowPacket> OpenWindow;
		public event PacketEventHandler<SpawnItemPacket> PickupSpawn;
		public event PacketEventHandler<PlayerPacket> Player;
		public event PacketEventHandler<PlayerBlockPlacementPacket> PlayerBlockPlacement;
		public event PacketEventHandler<PlayerDiggingPacket> PlayerDigging;
		public event PacketEventHandler<PlayerPositionPacket> PlayerPosition;
		public event PacketEventHandler<PlayerPositionRotationPacket> PlayerPositionRotation;
		public event PacketEventHandler<PlayerRotationPacket> PlayerRotation;
		public event PacketEventHandler<PlayNoteBlockPacket> PlayNoteBlock;
		public event PacketEventHandler<PreChunkPacket> PreChunk;
		public event PacketEventHandler<RespawnPacket> Respawn;
		public event PacketEventHandler<SetSlotPacket> SetSlot;
		public event PacketEventHandler<SpawnPositionPacket> SpawnPosition;
		public event PacketEventHandler<TimeUpdatePacket> TimeUpdate;
		public event PacketEventHandler<TransactionPacket> Transaction;
		public event PacketEventHandler<UnknownAPacket> UnknownA;
		public event PacketEventHandler<UpdateHealthPacket> UpdateHealth;
		public event PacketEventHandler<UpdateProgressBarPacket> UpdateProgressBar;
		public event PacketEventHandler<UpdateSignPacket> UpdateSign;
		public event PacketEventHandler<UseBedPacket> UseBed;
		public event PacketEventHandler<UseEntityPacket> UseEntity;
		public event PacketEventHandler<WeatherPacket> Weather;
		public event PacketEventHandler<WindowClickPacket> WindowClick;
		public event PacketEventHandler<WindowItemsPacket> WindowItems;

		private readonly Queue<Packet> PacketQueue = new Queue<Packet>();
		private readonly Thread QueueThread;
		private volatile bool Running = true;

		public PacketHandler(Server server, BigEndianStream stream)
		{
			Server = server;
			Net = stream;

			QueueThread = new Thread(QueueProc);
			QueueThread.IsBackground = true;
			QueueThread.Start();
		}

		public PacketHandler(Server server, TcpClient tcp)
			: this(server, new BigEndianStream(tcp.GetStream()))
		{
		}

		public void SendPacket(Packet packet)
		{
			lock (PacketQueue)
				PacketQueue.Enqueue(packet);
		}

		private void QueueProc()
		{
			while (Running)
			{
				while (PacketQueue.Count > 0 && Running)
				{
					Packet packet;
					lock (PacketQueue)
						packet = PacketQueue.Dequeue();
					try
					{
						Net.WritePacket(packet);
					}
					catch (Exception ex)
					{
						Dispose();
						Logger.Log(Logger.LogLevel.Info, "Disconnecting client due to Tx failure: " + ex.Message);
						return;
					}
				}
				Thread.Sleep(10);
			}
		}

		public void Dispose()
		{
			Running = false;
		}

		public bool ProcessPacket()
		{
			Packet p;
			try
			{
				p = Net.ReadPacket();
			}
			catch (Exception ex)
			{
				Dispose();
				Logger.Log(Logger.LogLevel.Info, "Disconnecting client: " + ex.Message);
				return false;
			}
			PacketType type = p.GetPacketType();

			switch (type)
			{
			case PacketType.AddObjectVehicle: OnAddObjectVehicle((AddObjectVehiclePacket)p); break;
			case PacketType.Animation: OnAnimation((AnimationPacket)p); break;
			case PacketType.AttachEntity: OnAttachEntity((AttachEntityPacket)p); break;
			case PacketType.BlockChange: OnBlockChange((BlockChangePacket)p); break;
			case PacketType.ChatMessage: OnChatMessage((ChatMessagePacket)p); break;
			case PacketType.CloseWindow: OnCloseWindow((CloseWindowPacket)p); break;
			case PacketType.CollectItem: OnCollectItem((CollectItemPacket)p); break;
			case PacketType.DestroyEntity: OnDestroyEntity((DestroyEntityPacket)p); break;
			case PacketType.Disconnect: OnDisconnect((DisconnectPacket)p); break;
			case PacketType.Entity: OnEntity((CreateEntityPacket)p); break;
			case PacketType.EntityAction: OnEntityAction((EntityActionPacket)p); break;
			case PacketType.EntityEquipment: OnEntityEquipment((EntityEquipmentPacket)p); break;
			case PacketType.EntityLook: OnEntityLook((EntityLookPacket)p); break;
			case PacketType.EntityLookAndRelativeMove: OnEntityLookAndRelativeMove((EntityLookAndRelativeMovePacket)p); break;
			case PacketType.EntityMetadata: OnEntityMetadata((EntityMetadataPacket)p); break;
			case PacketType.EntityPainting: OnEntityPainting((EntityPaintingPacket)p); break;
			case PacketType.EntityRelativeMove: OnEntityRelativeMove((EntityRelativeMovePacket)p); break;
			case PacketType.EntityStatus: OnEntityStatus((EntityStatusPacket)p); break;
			case PacketType.EntityTeleport: OnEntityTeleport((EntityTeleportPacket)p); break;
			case PacketType.EntityVelocity: OnEntityVelocity((EntityVelocityPacket)p); break;
			case PacketType.Explosion: OnExplosion((ExplosionPacket)p); break;
			case PacketType.Handshake: OnHandshake((HandshakePacket)p); break;
			case PacketType.HoldingChange: OnHoldingChange((HoldingChangePacket)p); break;
			case PacketType.KeepAlive: OnKeepAlive((KeepAlivePacket)p); break;
			case PacketType.LoginRequest: OnLoginRequest((LoginRequestPacket)p); break;
			case PacketType.MapChunk: OnMapChunk((MapChunkPacket)p); break;
			case PacketType.MobSpawn: OnMobSpawn((MobSpawnPacket)p); break;
			case PacketType.MultiBlockChange: OnMultiBlockChange((MultiBlockChangePacket)p); break;
			case PacketType.NamedEntitySpawn: OnNamedEntitySpawn((NamedEntitySpawnPacket)p); break;
			case PacketType.OpenWindow: OnOpenWindow((OpenWindowPacket)p); break;
			case PacketType.PickupSpawn: OnPickupSpawn((SpawnItemPacket)p); break;
			case PacketType.Player: OnPlayer((PlayerPacket)p); break;
			case PacketType.PlayerBlockPlacement: OnPlayerBlockPlacement((PlayerBlockPlacementPacket)p); break;
			case PacketType.PlayerDigging: OnPlayerDigging((PlayerDiggingPacket)p); break;
			case PacketType.PlayerPosition: OnPlayerPosition((PlayerPositionPacket)p); break;
			case PacketType.PlayerPositionRotation: OnPlayerPositionRotation((PlayerPositionRotationPacket)p); break;
			case PacketType.PlayerRotation: OnPlayerRotation((PlayerRotationPacket)p); break;
			case PacketType.PlayNoteBlock: OnPlayNoteBlock((PlayNoteBlockPacket)p); break;
			case PacketType.PreChunk: OnPreChunk((PreChunkPacket)p); break;
			case PacketType.Respawn: OnRespawn((RespawnPacket)p); break;
			case PacketType.SetSlot: OnSetSlot((SetSlotPacket)p); break;
			case PacketType.SpawnPosition: OnSpawnPosition((SpawnPositionPacket)p); break;
			case PacketType.TimeUpdate: OnTimeUpdate((TimeUpdatePacket)p); break;
			case PacketType.Transaction: OnTransaction((TransactionPacket)p); break;
			case PacketType.UnknownA: OnUnknownA((UnknownAPacket)p); break;
			case PacketType.UpdateHealth: OnUpdateHealth((UpdateHealthPacket)p); break;
			case PacketType.UpdateProgressBar: OnUpdateProgressBar((UpdateProgressBarPacket)p); break;
			case PacketType.UpdateSign: OnUpdateSign((UpdateSignPacket)p); break;
			case PacketType.UseBed: OnUseBed((UseBedPacket)p); break;
			case PacketType.UseEntity: OnUseEntity((UseEntityPacket)p); break;
			case PacketType.Weather: OnWeather((WeatherPacket)p); break;
			case PacketType.WindowClick: OnWindowClick((WindowClickPacket)p); break;
			case PacketType.WindowItems: OnWindowItems((WindowItemsPacket)p); break;
			}
			return true;
		}

		private void OnAddObjectVehicle(AddObjectVehiclePacket p) { if (AddObjectVehicle != null) AddObjectVehicle.Invoke(this, new PacketEventArgs<AddObjectVehiclePacket>(p)); }
		private void OnAnimation(AnimationPacket p) { if (Animation != null) Animation.Invoke(this, new PacketEventArgs<AnimationPacket>(p)); }
		private void OnAttachEntity(AttachEntityPacket p) { if (AttachEntity != null) AttachEntity.Invoke(this, new PacketEventArgs<AttachEntityPacket>(p)); }
		private void OnBlockChange(BlockChangePacket p) { if (BlockChange != null) BlockChange.Invoke(this, new PacketEventArgs<BlockChangePacket>(p)); }
		private void OnChatMessage(ChatMessagePacket p) { if (ChatMessage != null) ChatMessage.Invoke(this, new PacketEventArgs<ChatMessagePacket>(p)); }
		private void OnCloseWindow(CloseWindowPacket p) { if (CloseWindow != null) CloseWindow.Invoke(this, new PacketEventArgs<CloseWindowPacket>(p)); }
		private void OnCollectItem(CollectItemPacket p) { if (CollectItem != null) CollectItem.Invoke(this, new PacketEventArgs<CollectItemPacket>(p)); }
		private void OnDestroyEntity(DestroyEntityPacket p) { if (DestroyEntity != null) DestroyEntity.Invoke(this, new PacketEventArgs<DestroyEntityPacket>(p)); }
		private void OnDisconnect(DisconnectPacket p) { if (Disconnect != null) Disconnect.Invoke(this, new PacketEventArgs<DisconnectPacket>(p)); }
		private void OnEntity(CreateEntityPacket p) { if (Entity != null) Entity.Invoke(this, new PacketEventArgs<CreateEntityPacket>(p)); }
		private void OnEntityAction(EntityActionPacket p) { if (EntityAction != null) EntityAction.Invoke(this, new PacketEventArgs<EntityActionPacket>(p)); }
		private void OnEntityEquipment(EntityEquipmentPacket p) { if (EntityEquipment != null) EntityEquipment.Invoke(this, new PacketEventArgs<EntityEquipmentPacket>(p)); }
		private void OnEntityLook(EntityLookPacket p) { if (EntityLook != null) EntityLook.Invoke(this, new PacketEventArgs<EntityLookPacket>(p)); }
		private void OnEntityLookAndRelativeMove(EntityLookAndRelativeMovePacket p) { if (EntityLookAndRelativeMove != null) EntityLookAndRelativeMove.Invoke(this, new PacketEventArgs<EntityLookAndRelativeMovePacket>(p)); }
		private void OnEntityMetadata(EntityMetadataPacket p) { if (EntityMetadata != null) EntityMetadata.Invoke(this, new PacketEventArgs<EntityMetadataPacket>(p)); }
		private void OnEntityPainting(EntityPaintingPacket p) { if (EntityPainting != null) EntityPainting.Invoke(this, new PacketEventArgs<EntityPaintingPacket>(p)); }
		private void OnEntityRelativeMove(EntityRelativeMovePacket p) { if (EntityRelativeMove != null) EntityRelativeMove.Invoke(this, new PacketEventArgs<EntityRelativeMovePacket>(p)); }
		private void OnEntityStatus(EntityStatusPacket p) { if (EntityStatus != null) EntityStatus.Invoke(this, new PacketEventArgs<EntityStatusPacket>(p)); }
		private void OnEntityTeleport(EntityTeleportPacket p) { if (EntityTeleport != null) EntityTeleport.Invoke(this, new PacketEventArgs<EntityTeleportPacket>(p)); }
		private void OnEntityVelocity(EntityVelocityPacket p) { if (EntityVelocity != null) EntityVelocity.Invoke(this, new PacketEventArgs<EntityVelocityPacket>(p)); }
		private void OnExplosion(ExplosionPacket p) { if (Explosion != null) Explosion.Invoke(this, new PacketEventArgs<ExplosionPacket>(p)); }
		private void OnHandshake(HandshakePacket p) { if (Handshake != null) Handshake.Invoke(this, new PacketEventArgs<HandshakePacket>(p)); }
		private void OnHoldingChange(HoldingChangePacket p) { if (HoldingChange != null) HoldingChange.Invoke(this, new PacketEventArgs<HoldingChangePacket>(p)); }
		private void OnKeepAlive(KeepAlivePacket p) { if (KeepAlive != null) KeepAlive.Invoke(this, new PacketEventArgs<KeepAlivePacket>(p)); }
		private void OnLoginRequest(LoginRequestPacket p) { if (LoginRequest != null) LoginRequest.Invoke(this, new PacketEventArgs<LoginRequestPacket>(p)); }
		private void OnMapChunk(MapChunkPacket p) { if (MapChunk != null) MapChunk.Invoke(this, new PacketEventArgs<MapChunkPacket>(p)); }
		private void OnMobSpawn(MobSpawnPacket p) { if (MobSpawn != null) MobSpawn.Invoke(this, new PacketEventArgs<MobSpawnPacket>(p)); }
		private void OnMultiBlockChange(MultiBlockChangePacket p) { if (MultiBlockChange != null) MultiBlockChange.Invoke(this, new PacketEventArgs<MultiBlockChangePacket>(p)); }
		private void OnNamedEntitySpawn(NamedEntitySpawnPacket p) { if (NamedEntitySpawn != null) NamedEntitySpawn.Invoke(this, new PacketEventArgs<NamedEntitySpawnPacket>(p)); }
		private void OnOpenWindow(OpenWindowPacket p) { if (OpenWindow != null) OpenWindow.Invoke(this, new PacketEventArgs<OpenWindowPacket>(p)); }
		private void OnPickupSpawn(SpawnItemPacket p) { if (PickupSpawn != null) PickupSpawn.Invoke(this, new PacketEventArgs<SpawnItemPacket>(p)); }
		private void OnPlayer(PlayerPacket p) { if (Player != null) Player.Invoke(this, new PacketEventArgs<PlayerPacket>(p)); }
		private void OnPlayerBlockPlacement(PlayerBlockPlacementPacket p) { if (PlayerBlockPlacement != null) PlayerBlockPlacement.Invoke(this, new PacketEventArgs<PlayerBlockPlacementPacket>(p)); }
		private void OnPlayerDigging(PlayerDiggingPacket p) { if (PlayerDigging != null) PlayerDigging.Invoke(this, new PacketEventArgs<PlayerDiggingPacket>(p)); }
		private void OnPlayerPosition(PlayerPositionPacket p) { if (PlayerPosition != null) PlayerPosition.Invoke(this, new PacketEventArgs<PlayerPositionPacket>(p)); }
		private void OnPlayerPositionRotation(PlayerPositionRotationPacket p) { if (PlayerPositionRotation != null) PlayerPositionRotation.Invoke(this, new PacketEventArgs<PlayerPositionRotationPacket>(p)); }
		private void OnPlayerRotation(PlayerRotationPacket p) { if (PlayerRotation != null) PlayerRotation.Invoke(this, new PacketEventArgs<PlayerRotationPacket>(p)); }
		private void OnPlayNoteBlock(PlayNoteBlockPacket p) { if (PlayNoteBlock != null) PlayNoteBlock.Invoke(this, new PacketEventArgs<PlayNoteBlockPacket>(p)); }
		private void OnPreChunk(PreChunkPacket p) { if (PreChunk != null) PreChunk.Invoke(this, new PacketEventArgs<PreChunkPacket>(p)); }
		private void OnRespawn(RespawnPacket p) { if (Respawn != null) Respawn.Invoke(this, new PacketEventArgs<RespawnPacket>(p)); }
		private void OnSetSlot(SetSlotPacket p) { if (SetSlot != null) SetSlot.Invoke(this, new PacketEventArgs<SetSlotPacket>(p)); }
		private void OnSpawnPosition(SpawnPositionPacket p) { if (SpawnPosition != null) SpawnPosition.Invoke(this, new PacketEventArgs<SpawnPositionPacket>(p)); }
		private void OnTimeUpdate(TimeUpdatePacket p) { if (TimeUpdate != null) TimeUpdate.Invoke(this, new PacketEventArgs<TimeUpdatePacket>(p)); }
		private void OnTransaction(TransactionPacket p) { if (Transaction != null) Transaction.Invoke(this, new PacketEventArgs<TransactionPacket>(p)); }
		private void OnUnknownA(UnknownAPacket p) { if (UnknownA != null) UnknownA.Invoke(this, new PacketEventArgs<UnknownAPacket>(p)); }
		private void OnUpdateHealth(UpdateHealthPacket p) { if (UpdateHealth != null) UpdateHealth.Invoke(this, new PacketEventArgs<UpdateHealthPacket>(p)); }
		private void OnUpdateProgressBar(UpdateProgressBarPacket p) { if (UpdateProgressBar != null) UpdateProgressBar.Invoke(this, new PacketEventArgs<UpdateProgressBarPacket>(p)); }
		private void OnUpdateSign(UpdateSignPacket p) { if (UpdateSign != null) UpdateSign.Invoke(this, new PacketEventArgs<UpdateSignPacket>(p)); }
		private void OnUseBed(UseBedPacket p) { if (UseBed != null) UseBed.Invoke(this, new PacketEventArgs<UseBedPacket>(p)); }
		private void OnUseEntity(UseEntityPacket p) { if (UseEntity != null) UseEntity.Invoke(this, new PacketEventArgs<UseEntityPacket>(p)); }
		private void OnWeather(WeatherPacket p) { if (Weather != null) Weather.Invoke(this, new PacketEventArgs<WeatherPacket>(p)); }
		private void OnWindowClick(WindowClickPacket p) { if (WindowClick != null) WindowClick.Invoke(this, new PacketEventArgs<WindowClickPacket>(p)); }
		private void OnWindowItems(WindowItemsPacket p) { if (WindowItems != null) WindowItems.Invoke(this, new PacketEventArgs<WindowItemsPacket>(p)); }
	}
}
