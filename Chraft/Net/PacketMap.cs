using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;

namespace Chraft.Net
{
	public static class PacketMap
	{
		private static readonly Dictionary<PacketType, Type> _Map = new Dictionary<PacketType, Type>
		{
			{ PacketType.AddObjectVehicle, typeof(AddObjectVehiclePacket) },
			{ PacketType.Animation, typeof(AnimationPacket) },
			{ PacketType.AttachEntity, typeof(AttachEntityPacket) },
			{ PacketType.BlockChange, typeof(BlockChangePacket) },
			{ PacketType.ChatMessage, typeof(ChatMessagePacket) },
			{ PacketType.CloseWindow, typeof(CloseWindowPacket) },
			{ PacketType.CollectItem, typeof(CollectItemPacket) },
			{ PacketType.DestroyEntity, typeof(DestroyEntityPacket) },
			{ PacketType.Disconnect, typeof(DisconnectPacket) },
			{ PacketType.Entity, typeof(CreateEntityPacket) },
			{ PacketType.EntityAction, typeof(EntityActionPacket) },
			{ PacketType.EntityEquipment, typeof(EntityEquipmentPacket) },
			{ PacketType.EntityLook, typeof(EntityLookPacket) },
			{ PacketType.EntityLookAndRelativeMove, typeof(EntityLookAndRelativeMovePacket) },
			{ PacketType.EntityMetadata, typeof(EntityMetadataPacket) },
			{ PacketType.EntityPainting, typeof(EntityPaintingPacket) },
			{ PacketType.EntityRelativeMove, typeof(EntityRelativeMovePacket) },
			{ PacketType.EntityStatus, typeof(EntityStatusPacket) },
			{ PacketType.EntityTeleport, typeof(EntityTeleportPacket) },
			{ PacketType.EntityVelocity, typeof(EntityVelocityPacket) },
			{ PacketType.Explosion, typeof(ExplosionPacket) },
			{ PacketType.Handshake, typeof(HandshakePacket) },
			{ PacketType.HoldingChange, typeof(HoldingChangePacket) },
			{ PacketType.KeepAlive, typeof(KeepAlivePacket) },
			{ PacketType.LoginRequest, typeof(LoginRequestPacket) },
			{ PacketType.MapChunk, typeof(MapChunkPacket) },
			{ PacketType.MobSpawn, typeof(MobSpawnPacket) },
			{ PacketType.MultiBlockChange, typeof(MultiBlockChangePacket) },
			{ PacketType.NamedEntitySpawn, typeof(NamedEntitySpawnPacket) },
			{ PacketType.OpenWindow, typeof(OpenWindowPacket) },
			{ PacketType.PickupSpawn, typeof(SpawnItemPacket) },
			{ PacketType.Player, typeof(PlayerPacket) },
			{ PacketType.PlayerBlockPlacement, typeof(PlayerBlockPlacementPacket) },
			{ PacketType.PlayerDigging, typeof(PlayerDiggingPacket) },
			{ PacketType.PlayerPosition, typeof(PlayerPositionPacket) },
			{ PacketType.PlayerPositionRotation, typeof(PlayerPositionRotationPacket) },
			{ PacketType.PlayerRotation, typeof(PlayerRotationPacket) },
			{ PacketType.PlayNoteBlock, typeof(PlayNoteBlockPacket) },
			{ PacketType.PreChunk, typeof(PreChunkPacket) },
			{ PacketType.Respawn, typeof(RespawnPacket) },
			{ PacketType.SetSlot, typeof(SetSlotPacket) },
            { PacketType.SoundEffect, typeof(SoundEffectPacket) },
			{ PacketType.SpawnPosition, typeof(SpawnPositionPacket) },
			{ PacketType.TimeUpdate, typeof(TimeUpdatePacket) },
			{ PacketType.Transaction, typeof(TransactionPacket) },
			{ PacketType.UnknownA, typeof(UnknownAPacket) },
			{ PacketType.UpdateHealth, typeof(UpdateHealthPacket) },
			{ PacketType.UpdateProgressBar, typeof(UpdateProgressBarPacket) },
			{ PacketType.UpdateSign, typeof(UpdateSignPacket) },
			{ PacketType.UseBed, typeof(UseBedPacket) },
			{ PacketType.UseEntity, typeof(UseEntityPacket) },
			{ PacketType.WindowClick, typeof(WindowClickPacket) },
			{ PacketType.WindowItems, typeof(WindowItemsPacket) }
		};

		public static Dictionary<PacketType, Type> Map { get { return _Map; } }

		public static PacketType GetPacketType(Type type)
		{
			foreach (PacketType t in Map.Keys)
			{
				if (Map[t] == type)
					return t;
			}
			throw new KeyNotFoundException();
		}
	}
}
