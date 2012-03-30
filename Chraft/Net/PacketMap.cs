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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;

namespace Chraft.Net
{
    public static class PacketMap
    {
        public static void Initialize()
        {
            foreach (KeyValuePair<Type, PacketType> kvp in _map)
                _concurrentMap.TryAdd(kvp.Key, kvp.Value);

            // Lets free some memory
            _map = null;

        }
        private static Dictionary<Type, PacketType> _map = new Dictionary<Type, PacketType>
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
            { typeof(EnchantItemPacket), PacketType.EnchantItem},
            { typeof(CreateEntityPacket), PacketType.Entity },
            { typeof(EntityActionPacket), PacketType.EntityAction },
            { typeof(EntityEffectPacket), PacketType.EntityEffect },
            { typeof(EntityEquipmentPacket), PacketType.EntityEquipment },
            { typeof(EntityHeadLook), PacketType.EntityHeadLook},
            { typeof(EntityLookPacket), PacketType.EntityLook },
            { typeof(EntityLookAndRelativeMovePacket), PacketType.EntityLookAndRelativeMove },
            { typeof(EntityMetadataPacket), PacketType.EntityMetadata },
            { typeof(EntityPaintingPacket), PacketType.EntityPainting },
            { typeof(EntityRelativeMovePacket), PacketType.EntityRelativeMove },
            { typeof(EntityStatusPacket), PacketType.EntityStatus },
            { typeof(EntityTeleportPacket), PacketType.EntityTeleport },
            { typeof(EntityVelocityPacket), PacketType.EntityVelocity },
            { typeof(ExperiencePacket), PacketType.Experience },
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
            { typeof(PlayerAbilitiesPacket), PacketType.PlayerAbilities},
            { typeof(PlayerPacket), PacketType.Player },
            { typeof(PlayerBlockPlacementPacket), PacketType.PlayerBlockPlacement },
            { typeof(PlayerDiggingPacket), PacketType.PlayerDigging },
            { typeof(PlayerListItemPacket), PacketType.PlayerListItem },
            { typeof(PlayerPositionPacket), PacketType.PlayerPosition },
            { typeof(PlayerPositionRotationPacket), PacketType.PlayerPositionRotation },
            { typeof(PlayerRotationPacket), PacketType.PlayerRotation },
            { typeof(PluginMessagePacket), PacketType.PluginMessage},
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
            { typeof(UpdateHealthPacket), PacketType.UpdateHealth },
            { typeof(UpdateProgressBarPacket), PacketType.UpdateProgressBar },
            { typeof(UpdateSignPacket), PacketType.UpdateSign },
            { typeof(UseBedPacket), PacketType.UseBed },
            { typeof(UseEntityPacket), PacketType.UseEntity },
            { typeof(UpdateTileEntity), PacketType.UpdateTileEntity},
            { typeof(WindowClickPacket), PacketType.WindowClick },
            { typeof(WindowItemsPacket), PacketType.WindowItems }
        };

        private readonly static ConcurrentDictionary<Type, PacketType> _concurrentMap = new ConcurrentDictionary<Type, PacketType>();
        public static ConcurrentDictionary<Type, PacketType> Map { get { return _concurrentMap; } }

        public static PacketType GetPacketType(Type type)
        {
            PacketType packetType;
            if (_concurrentMap.TryGetValue(type, out packetType))
                return packetType;

            throw new KeyNotFoundException();
        }
    }
}
