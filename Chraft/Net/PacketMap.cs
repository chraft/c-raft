using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;

namespace Chraft.Net
{
    public static class PacketMap
    {
        private static readonly Dictionary<Type, PacketType> _Map = new Dictionary<Type, PacketType>
        {
            { typeof(AddObjectVehiclePacket), PacketType.AddObjectVehicle },
            { typeof(AnimationPacket), PacketType.Animation },
            { typeof(AttachEntityPacket), PacketType.AttachEntity },
            { typeof(BlockChangePacket), PacketType.BlockChange },
            { typeof(BlockActionPacket), PacketType.BlockAction },
            { typeof(ChatMessagePacket), PacketType.ChatMessage },
            { typeof(CloseWindowPacket), PacketType.CloseWindow },
            { typeof(CollectItemPacket), PacketType.CollectItem },
			{ typeof(CreativeInventoryActionPacket), PacketType.CreativeInventoryAction },
            { typeof(DestroyEntityPacket), PacketType.DestroyEntity },
            { typeof(DisconnectPacket), PacketType.Disconnect },
            { typeof(CreateEntityPacket), PacketType.Entity },
            { typeof(EntityActionPacket), PacketType.EntityAction },
            { typeof(EntityEffectPacket), PacketType.EntityEffect },
            { typeof(EntityEquipmentPacket), PacketType.EntityEquipment },
            { typeof(EntityLookPacket), PacketType.EntityLook },
            { typeof(EntityLookAndRelativeMovePacket), PacketType.EntityLookAndRelativeMove },
            { typeof(EntityMetadataPacket), PacketType.EntityMetadata },
            { typeof(EntityPaintingPacket), PacketType.EntityPainting },
            { typeof(EntityRelativeMovePacket), PacketType.EntityRelativeMove },
            { typeof(EntityStatusPacket), PacketType.EntityStatus },
            { typeof(EntityTeleportPacket), PacketType.EntityTeleport },
            { typeof(EntityVelocityPacket), PacketType.EntityVelocity },
            { typeof(ExperienceOrbPacket), PacketType.ExperienceOrb },
            { typeof(ExplosionPacket), PacketType.Explosion },
            { typeof(HandshakePacket), PacketType.Handshake },
            { typeof(HoldingChangePacket), PacketType.HoldingChange },
            { typeof(KeepAlivePacket), PacketType.KeepAlive },
            { typeof(IncrementStatisticPacket), PacketType.IncrementStatistic },
            { typeof(LoginRequestPacket), PacketType.LoginRequest },
            { typeof(MapDataPacket), PacketType.MapData },
            { typeof(MapChunkPacket), PacketType.MapChunk },
            { typeof(MobSpawnPacket), PacketType.MobSpawn },
            { typeof(MultiBlockChangePacket), PacketType.MultiBlockChange },
            { typeof(NamedEntitySpawnPacket), PacketType.NamedEntitySpawn },
            { typeof(NewInvalidStatePacket), PacketType.NewInvalidState },
            { typeof(OpenWindowPacket), PacketType.OpenWindow },
            { typeof(SpawnItemPacket), PacketType.PickupSpawn },
            { typeof(PlayerPacket), PacketType.Player },
            { typeof(PlayerBlockPlacementPacket), PacketType.PlayerBlockPlacement },
            { typeof(PlayerDiggingPacket), PacketType.PlayerDigging },
            { typeof(PlayerListItemPacket), PacketType.PlayerListItem },
            { typeof(PlayerPositionPacket), PacketType.PlayerPosition },
            { typeof(PlayerPositionRotationPacket), PacketType.PlayerPositionRotation },
            { typeof(PlayerRotationPacket), PacketType.PlayerRotation },
            { typeof(PreChunkPacket), PacketType.PreChunk },
            { typeof(RemoveEntityEffectPacket), PacketType.RemoveEntityEffect },
            { typeof(RespawnPacket), PacketType.Respawn },
            { typeof(ServerListPingPacket), PacketType.ServerListPing },
            { typeof(SetSlotPacket), PacketType.SetSlot },
            { typeof(SoundEffectPacket), PacketType.SoundEffect },
            { typeof(SpawnPositionPacket), PacketType.SpawnPosition },
            { typeof(TimeUpdatePacket), PacketType.TimeUpdate },
            { typeof(ThunderBoltPacket), PacketType.Thunderbolt },
            { typeof(TransactionPacket), PacketType.Transaction },
            { typeof(UnknownAPacket), PacketType.UnknownA },
            { typeof(UpdateHealthPacket), PacketType.UpdateHealth },
            { typeof(UpdateProgressBarPacket), PacketType.UpdateProgressBar },
            { typeof(UpdateSignPacket), PacketType.UpdateSign },
            { typeof(UseBedPacket), PacketType.UseBed },
            { typeof(UseEntityPacket), PacketType.UseEntity },
            { typeof(WindowClickPacket), PacketType.WindowClick },
            { typeof(WindowItemsPacket), PacketType.WindowItems }
        };

        public static Dictionary<Type, PacketType> Map { get { return _Map; } }

        public static PacketType GetPacketType(Type type)
        {
            PacketType packetType;
            if(_Map.TryGetValue(type, out packetType))
                return packetType;
            
            throw new KeyNotFoundException();
        }
    }
}
