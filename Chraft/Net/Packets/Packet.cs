using System;
using Chraft.Interfaces;
using Chraft.World;
using Chraft.Entity;

namespace Chraft.Net.Packets
{
    /// <summary>
    /// Contains all the packet read / write methods.
    /// Propeties must be read in order as specified by the protocol http://mc.kev009.com/Protocol
    /// use sbytes for handling bytes of negative value (-128 to 127) otherwise normal bytes (0-255) are fine
    /// </summary>

    public abstract class Packet
    {
        public abstract void Read(BigEndianStream stream);
        public abstract void Write(BigEndianStream stream);
        public void WriteFlush(BigEndianStream stream)
        {
            Write(stream);
            stream.Flush();
        }

        public PacketType GetPacketType()
        {
            return PacketMap.GetPacketType(GetType());
        }

        public static Packet Read(PacketType type, BigEndianStream stream)
        {
            try
            {
                if (!PacketMap.Map.ContainsKey(type))
                {
                    Console.WriteLine("ERROR Unknown packet type: {0}", type);
                    return null;
                }
                Type ptype = PacketMap.Map[type];
                Packet packet = (Packet)ptype.GetConstructor(new Type[0]).Invoke(new object[0]);
                packet.Read(stream);
                return packet;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing packet of type {0}: {1}", type.ToString(), ex);
                return null;
            }
        }
    }

    public class KeepAlivePacket : Packet
    {
        public int KeepAliveID { get; set; }

        public override void Read(BigEndianStream stream)
        {
            KeepAliveID = stream.ReadInt();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(KeepAliveID);
        }
    }

    public class LoginRequestPacket : Packet
    {
        public int ProtocolOrEntityId { get; set; }
        public string Username { get; set; }
        public long MapSeed { get; set; }
        public int ServerMode { get; set; }
        public sbyte Dimension { get; set; }
        public sbyte Unknown { get; set; }
        public byte WorldHeight { get; set; }
        public byte MaxPlayers { get; set; }

        public override void Read(BigEndianStream stream)
        {
            ProtocolOrEntityId = stream.ReadInt();
            Username = stream.ReadString16(16);
            MapSeed = stream.ReadLong();
            ServerMode = stream.ReadInt();
            Dimension = stream.ReadSByte();
            Unknown = stream.ReadSByte();
            WorldHeight = stream.ReadByte();
            MaxPlayers = stream.ReadByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(ProtocolOrEntityId);
            stream.Write(Username);
            stream.Write(MapSeed);
            stream.Write(ServerMode);
            stream.Write(Dimension);
            stream.Write(Unknown);
            stream.Write(WorldHeight);
            stream.Write(MaxPlayers);
        }
    }

    public class HandshakePacket : Packet
    {
        public string UsernameOrHash { get; set; }

        public override void Read(BigEndianStream stream)
        {
            UsernameOrHash = stream.ReadString16(16);
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(UsernameOrHash);
        }
    }

    public class ChatMessagePacket : Packet
    {
        public string Message { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Message = stream.ReadString16(100);
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(Message);
        }
    }

    public class TimeUpdatePacket : Packet
    {
        public long Time { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Time = stream.ReadLong();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(Time);
        }
    }

    public class EntityEquipmentPacket : Packet
    {
        public int EntityId { get; set; }
        public short Slot { get; set; }
        public short ItemId { get; set; }
        public short Durability { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            Slot = stream.ReadShort();
            ItemId = stream.ReadShort();
            Durability = stream.ReadShort();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(Slot);
            stream.Write(ItemId);
            stream.Write(Durability);
        }
    }

    public class SpawnPositionPacket : Packet
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadInt();
            Z = stream.ReadInt();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
        }
    }

    public class UseEntityPacket : Packet
    {
        public int User { get; set; }
        public int Target { get; set; }
        public bool LeftClick { get; set; }

        public override void Read(BigEndianStream stream)
        {
            User = stream.ReadInt();
            Target = stream.ReadInt();
            LeftClick = stream.ReadBool();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(User);
            stream.Write(Target);
            stream.Write(LeftClick);
        }
    }

    public class UpdateHealthPacket : Packet
    {
        public short Health { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Health = stream.ReadShort();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(Health);
        }
    }

    public class RespawnPacket : Packet
    {
        public sbyte World { get; set; }
        public sbyte Unknown { get; set; }
        public sbyte CreativeMode { get; set; } // 0 for survival, 1 for creative.
        public short WorldHeight { get; set; } // Default 128
        public long MapSeed { get; set; }


        public override void Read(BigEndianStream stream)
        {
            World = stream.ReadSByte();
            Unknown = stream.ReadSByte();
            CreativeMode = stream.ReadSByte();
            WorldHeight = stream.ReadShort();
            MapSeed = stream.ReadLong();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(World);
            stream.Write(Unknown);
            stream.Write(CreativeMode);
            stream.Write(WorldHeight);
            stream.Write(MapSeed);
        }
    }

    public class PlayerPacket : Packet
    {
        public bool OnGround { get; set; }

        public override void Read(BigEndianStream stream)
        {
            OnGround = stream.ReadBool();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(OnGround);
        }
    }

    public class PlayerPositionPacket : Packet
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Stance { get; set; }
        public double Z { get; set; }
        public bool OnGround { get; set; }

        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadDouble();
            Y = stream.ReadDouble();
            Stance = stream.ReadDouble();
            Z = stream.ReadDouble();
            OnGround = stream.ReadBool();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Stance);
            stream.Write(Z);
            stream.Write(OnGround);
        }
    }

    public class PlayerRotationPacket : Packet
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public bool OnGround { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Yaw = stream.ReadFloat();
            Pitch = stream.ReadFloat();
            OnGround = stream.ReadBool();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(Yaw);
            stream.Write(Pitch);
            stream.Write(OnGround);
        }
    }

    public class PlayerPositionRotationPacket : Packet
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Stance { get; set; }
        public double Z { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public bool OnGround { get; set; }

        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadDouble();
            Stance = stream.ReadDouble();
            Y = stream.ReadDouble();
            Z = stream.ReadDouble();
            Yaw = stream.ReadFloat();
            Pitch = stream.ReadFloat();
            OnGround = stream.ReadBool();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Stance);
            stream.Write(Z);
            stream.Write(Yaw);
            stream.Write(Pitch);
            stream.Write(OnGround);
        }
    }

    public class PlayerDiggingPacket : Packet
    {
        public DigAction Action { get; set; }
        public int X { get; set; }
        public sbyte Y { get; set; }
        public int Z { get; set; }
        public sbyte Face { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Action = (DigAction)stream.ReadByte();
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();
            Face = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write((byte)Action);
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
            stream.Write(Face);
        }
        public enum DigAction : byte
        {
            StartDigging = 0,
            FinishDigging = 2,
            DropItem = 4
        }
    }

    public class PlayerBlockPlacementPacket : Packet
    {
        public int X { get; set; }
        public sbyte Y { get; set; }
        public int Z { get; set; }
        public BlockFace Face { get; set; }
        public ItemStack Item { get; set; }
        
        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();
            Face = (BlockFace)stream.ReadSByte(); 
            Item = ItemStack.Read(stream);
            //amount in hand and durability are handled int ItemStack.Read
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
            stream.Write((sbyte)Face);
            (Item ?? ItemStack.Void).Write(stream);
        }
    }

    public class HoldingChangePacket : Packet
    {
        public short Slot { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Slot = stream.ReadShort();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(Slot);
        }
    }

    public class UseBedPacket : Packet
    {
        public int PlayerId { get; set; }
        public sbyte InBed { get; set; }
        public int X { get; set; }
        public sbyte Y { get; set; }
        public int Z { get; set; }

        public override void Read(BigEndianStream stream)
        {
            PlayerId = stream.ReadInt();
            InBed = stream.ReadSByte();
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(PlayerId);
            stream.Write(InBed);
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
        }
    }

    public class AnimationPacket : Packet
    {
        public int PlayerId { get; set; }
        public sbyte Animation { get; set; }

        public override void Read(BigEndianStream stream)
        {
            PlayerId = stream.ReadInt();
            Animation = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(PlayerId);
            stream.Write(Animation);
        }
    }

    public class EntityActionPacket : Packet
    {
        public int PlayerId { get; set; }
        public sbyte Action { get; set; }

        public override void Read(BigEndianStream stream)
        {
            PlayerId = stream.ReadInt();
            Action = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(PlayerId);
            stream.Write(Action);
        }
    }

    public class NamedEntitySpawnPacket : Packet
    {
        public int EntityId { get; set; }
        public string PlayerName { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }
        public short CurrentItem { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            PlayerName = stream.ReadString16(16);
            X = (double)stream.ReadInt() / 32.0d;
            Y = (double)stream.ReadInt() / 32.0d;
            Z = (double)stream.ReadInt() / 32.0d;
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
            CurrentItem = stream.ReadShort();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(PlayerName);
            stream.Write((int)(X * 32));
            stream.Write((int)(Y * 32));
            stream.Write((int)(Z * 32));
            stream.Write(Yaw);
            stream.Write(Pitch);
            stream.Write(CurrentItem);
        }
    }

    public class SpawnItemPacket : Packet
    {
        public int EntityId { get; set; }
        public short ItemId { get; set; }
        public sbyte Count { get; set; }
        public short Durability { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }
        public sbyte Roll { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            ItemId = stream.ReadShort();
            Count = stream.ReadSByte();
            Durability = stream.ReadShort();
            X = (double)stream.ReadInt() / 32.0d;
            Y = (double)stream.ReadInt() / 32.0d;
            Z = (double)stream.ReadInt() / 32.0d;
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
            Roll = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(ItemId);
            stream.Write(Count);
            stream.Write(Durability);
            stream.Write((int)(X * 32));
            stream.Write((int)(Y * 32));
            stream.Write((int)(Z * 32));
            stream.Write(Yaw);
            stream.Write(Pitch);
            stream.Write(Roll);
        }
    }

    public class CollectItemPacket : Packet
    {
        public int EntityId { get; set; }
        public int PlayerId { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            PlayerId = stream.ReadInt();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(PlayerId);
        }
    }

    public class AddObjectVehiclePacket : Packet
    {
        public int EntityId { get; set; }
        public ObjectType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int UnknownFlag { get; set; }
        public short UnknownA { get; set; }
        public short UnknownB { get; set; }
        public short UnknownC { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            Type = (ObjectType)stream.ReadSByte();
            X = (double)stream.ReadInt() / 32.0d; // ((double)intX / 32.0d) => representation of X as double
            Y = (double)stream.ReadInt() / 32.0d;
            Z = (double)stream.ReadInt() / 32.0d;
            UnknownFlag = stream.ReadInt();
            UnknownA = stream.ReadShort();
            UnknownB = stream.ReadShort();
            UnknownC = stream.ReadShort();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write((sbyte)Type);
            stream.Write((int)(X * 32));
            stream.Write((int)(Y * 32));
            stream.Write((int)(Z * 32));
            stream.Write(UnknownFlag);
            stream.Write(UnknownA);
            stream.Write(UnknownB);
            stream.Write(UnknownC);
        }

        public enum ObjectType : sbyte
        {
            Boat = 1,
            Minecart = 10,
            StorageCart = 11,
	        PoweredCart = 12,
            ActivatedTNT = 50,
            Arrow = 60,
            ThrownSnowball = 61,
            ThrownEgg = 62,
            FallingSand = 70,
            FallingGravel = 71,
            FishingFloat = 90,
        }
    }

    public class MobSpawnPacket : Packet
    {
        public int EntityId { get; set; }
        public MobType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }
        public MetaData Data { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            Type = (MobType)stream.ReadByte();
            X = (double)stream.ReadInt() / 32.0d;
            Y = (double)stream.ReadInt() / 32.0d;
            Z = (double)stream.ReadInt() / 32.0d;
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
            Data = stream.ReadMetaData();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write((byte)Type);
            stream.Write((int)(X * 32));
            stream.Write((int)(Y * 32));
            stream.Write((int)(Z * 32));
            stream.Write(Yaw);
            stream.Write(Pitch);
            stream.Write(Data);
        }
    }

    public class EntityPaintingPacket : Packet
    {
        public int EntityId { get; set; }
        public string Title { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int GraphicId { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            Title = stream.ReadString16(13);
            X = stream.ReadInt();
            Y = stream.ReadInt();
            Z = stream.ReadInt();
            GraphicId = stream.ReadInt();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(Title);
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
            stream.Write(GraphicId);
        }
    }

    public class UnknownAPacket : Packet
    {
        public float Sink1 { get; set; }
        public float Sink2 { get; set; }
        public float Sink3 { get; set; }
        public float Sink4 { get; set; }
        public bool Sink5 { get; set; }
        public bool Sink6 { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Sink1 = stream.ReadFloat();
            Sink2 = stream.ReadFloat();
            Sink3 = stream.ReadFloat();
            Sink4 = stream.ReadFloat();
            Sink5 = stream.ReadBool();
            Sink6 = stream.ReadBool();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(Sink1);
            stream.Write(Sink2);
            stream.Write(Sink3);
            stream.Write(Sink4);
            stream.Write(Sink5);
            stream.Write(Sink6);
        }
    }

    public class EntityVelocityPacket : Packet
    {
        public int EntityId { get; set; }
        public short VelocityX { get; set; }
        public short VelocityY { get; set; }
        public short VelocityZ { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            VelocityX = stream.ReadShort();
            VelocityY = stream.ReadShort();
            VelocityZ = stream.ReadShort();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(VelocityX);
            stream.Write(VelocityY);
            stream.Write(VelocityZ);
        }
    }

    public class DestroyEntityPacket : Packet
    {
        public int EntityId { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
        }
    }

    public class CreateEntityPacket : Packet
    {
        public int EntityId { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
        }
    }

    public class EntityRelativeMovePacket : Packet
    {
        public int EntityId { get; set; }
        public sbyte DeltaX { get; set; }
        public sbyte DeltaY { get; set; }
        public sbyte DeltaZ { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            DeltaX = stream.ReadSByte();
            DeltaY = stream.ReadSByte();
            DeltaZ = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(DeltaX);
            stream.Write(DeltaY);
            stream.Write(DeltaZ);
        }
    }

    public class EntityLookPacket : Packet
    {
        public int EntityId { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(Yaw);
            stream.Write(Pitch);
        }
    }

    public class EntityLookAndRelativeMovePacket : Packet
    {
        public int EntityId { get; set; }
        public sbyte DeltaX { get; set; }
        public sbyte DeltaY { get; set; }
        public sbyte DeltaZ { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            DeltaX = stream.ReadSByte();
            DeltaY = stream.ReadSByte();
            DeltaZ = stream.ReadSByte();
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(DeltaX);
            stream.Write(DeltaY);
            stream.Write(DeltaZ);
            stream.Write(Yaw);
            stream.Write(Pitch);
        }
    }

    public class EntityTeleportPacket : Packet
    {
        public int EntityId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            X = (double)stream.ReadInt() / 32.0d;
            Y = (double)stream.ReadInt() / 32.0d;
            Z = (double)stream.ReadInt() / 32.0d;
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write((int)(X * 32));
            stream.Write((int)(Y * 32));
            stream.Write((int)(Z * 32));
            stream.Write(Yaw);
            stream.Write(Pitch);
        }
    }

    public class EntityStatusPacket : Packet
    {
        public int EntityId { get; set; }
        public sbyte EntityStatus { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            EntityStatus = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(EntityStatus);
        }
    }

    public class AttachEntityPacket : Packet
    {
        public int EntityId { get; set; }
        public int VehicleId { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            VehicleId = stream.ReadInt();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(VehicleId);
        }
    }

    public class EntityMetadataPacket : Packet
    {
        public int EntityId { get; set; }
        public MetaData Data { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            Data = stream.ReadMetaData();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(Data);
        }
    }

    public class PreChunkPacket : Packet
    {
        public int X { get; set; }
        public int Z { get; set; }
        public bool Load { get; set; }

        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadInt();
            Z = stream.ReadInt();
            Load = stream.ReadBool();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Z);
            stream.Write(Load);
        }
    }

    public class MultiBlockChangePacket : Packet
    {
        public int X { get; set; }
        public int Z { get; set; }
        public short[] Coords { get; set; }
        public sbyte[] Types { get; set; }
        public sbyte[] Metadata { get; set; }

        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadInt();
            Z = stream.ReadInt();
            short length = stream.ReadShort();
            Coords = new short[length];
            Types = new sbyte[length];
            Metadata = new sbyte[length];
            for (int i = 0; i < Coords.Length; i++)
                Coords[i] = stream.ReadShort();
            for (int i = 0; i < Types.Length; i++)
                Types[i] = stream.ReadSByte();
            for (int i = 0; i < Metadata.Length; i++)
                Metadata[i] = stream.ReadSByte();

        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Z);
            stream.Write((short)Coords.Length);
            for (int i = 0; i < Coords.Length; i++)
                stream.Write(Coords[i]);
            for (int i = 0; i < Types.Length; i++)
                stream.Write(Types[i]);
            for (int i = 0; i < Metadata.Length; i++)
                stream.Write(Metadata[i]);
        }
    }

    public class BlockChangePacket : Packet
    {
        public int X { get; set; }
        public sbyte Y { get; set; }
        public int Z { get; set; }
        public byte Type { get; set; }
        public byte Data { get; set; }

        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();
            Type = stream.ReadByte();
            Data = stream.ReadByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
            stream.Write(Type);
            stream.Write(Data);
        }
    }

    public class BlockActionPacket : Packet
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public sbyte DataA { get; set; }
        public sbyte DataB { get; set; }

        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadInt();
            Z = stream.ReadInt();
            DataA = stream.ReadSByte();
            DataB = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
            stream.Write(DataA);
            stream.Write(DataB);
        }

        #region Note Block Action
        public void SetNoteBlockAction(int x, int y, int z, Instrument instrument, Pitch pitch)
        {
            X = x;
            Y = y;
            Z = z;
            DataA = (sbyte)instrument;
            DataB = (sbyte)pitch;
        }

        public enum Instrument : sbyte
        {
            Harp = 0,
            DoubleBass = 1,
            SnareDrum = 2,
            Sticks = 3,
            BassDrum = 4
        }
        public enum Pitch : sbyte
        {
            Octave1_00_Fsharp   = 0,
            Octave1_01_G        = 1,
            Octave1_02_Gsharp   = 2,
            Octave1_03_A        = 3,
            Octave1_04_Asharp   = 4,
            Octave1_05_B        = 5,
            Octave1_06_C        = 6,
            Octave1_07_Csharp   = 7,
            Octave1_08_D        = 8,
            Octave1_09_Dsharp   = 9,
            Octave1_10_E        = 10,
            Octave1_11_F        = 11,
            Octave2_00_Fsharp   = 12,
            Octave2_01_G        = 13,
            Octave2_02_Gsharp   = 14,
            Octave2_03_A        = 15,
            Octave2_04_Asharp   = 16,
            Octave2_05_B        = 17,
            Octave2_06_Bsharp   = 18,
            Octave2_07_C        = 19,
            Octave2_08_Csharp   = 20,
            Octave2_09_D        = 21,
            Octave2_10_Dsharp   = 22,
            Octave2_11_E        = 23,
            Octave2_12_F        = 24,
        }
        #endregion

        #region Piston Action

        public void SetPistonAction(int x, int y, int z, PistonState state, PistonDirection direction)
        {
            X = x;
            Y = y;
            Z = z;
            DataA = (sbyte)state;
            DataB = (sbyte)direction;
        }

        public enum PistonState : sbyte
        {
            Pushing = 0,
            Pulling = 1,
        }

        public enum PistonDirection : sbyte
        {
            Down = 0,
            Up = 1,
            East = 2,
            West = 3,
            North = 4,
            South = 5,
        }

        #endregion
    }

    public class ExplosionPacket : Packet
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public float Radius { get; set; }
        public sbyte[,] Offsets { get; set; }

        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadDouble();
            Y = stream.ReadDouble();
            Z = stream.ReadDouble();
            Radius = stream.ReadFloat();
            Offsets = new sbyte[stream.ReadInt(), 3];
            for (int i = 0; i < Offsets.GetLength(0); i++)
            {
                Offsets[i, 0] = stream.ReadSByte();
                Offsets[i, 1] = stream.ReadSByte();
                Offsets[i, 2] = stream.ReadSByte();
            }

        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
            stream.Write(Radius);
            stream.Write((int)Offsets.GetLength(0));
            for (int i = 0; i < Offsets.GetLength(0); i++)
            {
                stream.Write(Offsets[i, 0]);
                stream.Write(Offsets[i, 1]);
                stream.Write(Offsets[i, 2]);
            }
        }
    }

    public class SoundEffectPacket : Packet
    {
        /// <summary>
        /// The ID of the sound effect to play
        /// </summary>
        public SoundEffect EffectID { get; set; }
        /// <summary>
        /// The X location of the effect
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// The Y location of the effect
        /// </summary>
        public byte Y { get; set; }
        /// <summary>
        /// The Z location of the effect
        /// </summary>
        public int Z { get; set; }
        /// <summary>
        /// Extra data about RECORD_PLAY, SMOKE, and BLOCK_BREAK
        /// </summary>
        public int SoundData { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EffectID = (SoundEffect)stream.ReadInt();
            X = stream.ReadInt();
            Y = stream.ReadByte();
            Z = stream.ReadInt();
            SoundData = stream.ReadInt();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write((int)EffectID);
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
            stream.Write((int)SoundData);
        }

        public enum SoundEffect : int
        {
            CLICK2 = 1000,
            CLICK1 = 1001,
            BOW_FIRE = 1002,
            DOOR_TOGGLE = 1003,
            EXTINGUISH = 1004,
            RECORD_PLAY = 1005, // Has SoundData (probably record ID)
            SMOKE = 2000,       // Has SoundData (direction, see SmokeDirection)
            BLOCK_BREAK = 2001  // Has SoundData (Block ID broken)
        }

        public enum SmokeDirection : int
        {
            SouthEast = 0,
            South = 1,
            SouthWest = 2,
            East = 3,
            UpOrMiddle = 4, // ? not clear at http://mc.kev009.com/Protocol#Sound_effect_.280x3D.29
            West = 5,
            NorthEast = 6,
            North = 7,
            NorthWest = 8
        }
    }

    public class OpenWindowPacket : Packet
    {
        public sbyte WindowId { get; set; }
        internal InterfaceType InventoryType { get; set; }
        public string WindowTitle { get; set; }
        public sbyte SlotCount { get; set; }

        public override void Read(BigEndianStream stream)
        {
            WindowId = stream.ReadSByte();
            InventoryType = (InterfaceType)stream.ReadSByte();
            WindowTitle = stream.ReadString8(100);
            SlotCount = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(WindowId);
            stream.Write((sbyte)InventoryType);
            stream.Write8(WindowTitle);
            stream.Write(SlotCount);
        }
    }

    public class CloseWindowPacket : Packet
    {
        public sbyte WindowId { get; set; }

        public override void Read(BigEndianStream stream)
        {
            WindowId = stream.ReadSByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(WindowId);
        }
    }

    public class WindowClickPacket : Packet
    {
        public sbyte WindowId { get; set; }
        public short Slot { get; set; }
        public bool RightClick { get; set; }
        public short Transaction { get; set; }
        public bool Shift { get; set; }
        public ItemStack Item { get; set; }

        public override void Read(BigEndianStream stream)
        {
            WindowId = stream.ReadSByte();
            Slot = stream.ReadShort();
            RightClick = stream.ReadBool();
            Transaction = stream.ReadShort();
            Shift = stream.ReadBool();
            Item = ItemStack.Read(stream);
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(WindowId);
            stream.Write(Slot);
            stream.Write(RightClick);
            stream.Write(Transaction);
            stream.Write(Shift);
            (Item ?? ItemStack.Void).Write(stream);
        }
    }

    public class SetSlotPacket : Packet
    {
        public sbyte WindowId { get; set; }
        public short Slot { get; set; }
        public ItemStack Item { get; set; }

        public override void Read(BigEndianStream stream)
        {
            WindowId = stream.ReadSByte();
            Slot = stream.ReadShort();
            Item = ItemStack.Read(stream);
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(WindowId);
            stream.Write(Slot);
            (Item ?? ItemStack.Void).Write(stream);
        }
    }

    public class WindowItemsPacket : Packet
    {
        public sbyte WindowId { get; set; }
        public ItemStack[] Items { get; set; }

        public override void Read(BigEndianStream stream)
        {
            WindowId = stream.ReadSByte();
            Items = new ItemStack[stream.ReadShort()];
            for (int i = 0; i < Items.Length; i++)
                Items[i] = ItemStack.Read(stream);
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(WindowId);
            stream.Write((short)Items.Length);
            for (int i = 0; i < Items.Length; i++)
                (Items[i] ?? ItemStack.Void).Write(stream);
        }
    }

    public class UpdateProgressBarPacket : Packet
    {
        /// <summary>
        /// The id of the window that the progress bar is in.
        /// </summary>
        public sbyte WindowId { get; set; }
        /// <summary>
        /// Which of the progress bars that should be updated. (For furnaces, 0 = progress arrow, 1 = fire icon)
        /// </summary>
        public short ProgressBar { get; set; }
        /// <summary>
        /// <para>The value of the progress bar. </para>
        /// <para>
        /// The maximum values vary depending on the progress bar. Presumably the values are specified as in-game ticks. Some progress bar values increase, while others decrease. For furnaces, 0 is empty, full progress arrow = about 180, full fire icon = about 250)
        /// </para>
        /// </summary>
        public short Value { get; set; }

        public override void Read(BigEndianStream stream)
        {
            WindowId = stream.ReadSByte();
            ProgressBar = stream.ReadShort();
            Value = stream.ReadShort();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(WindowId);
            stream.Write(ProgressBar);
            stream.Write(Value);
        }
    }

    public class TransactionPacket : Packet
    {
        public sbyte WindowId { get; set; }
        public short Transaction { get; set; }
        public bool Accepted { get; set; }

        public override void Read(BigEndianStream stream)
        {
            WindowId = stream.ReadSByte();
            Transaction = stream.ReadShort();
            Accepted = stream.ReadBool();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(WindowId);
            stream.Write(Transaction);
            stream.Write(Accepted);
        }
    }

    public class UpdateSignPacket : Packet
    {
        public int X { get; set; }
        public short Y { get; set; }
        public int Z { get; set; }
        public string[] Lines { get; set; }

        public override void Read(BigEndianStream stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadShort();
            Z = stream.ReadInt();
            Lines = new string[4];
            for (int i = 0; i < Lines.Length; i++)
                Lines[i] = stream.ReadString16(25);
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
            for (int i = 0; i < Lines.Length; i++)
                stream.Write(Lines[i]);
        }
    }

    public class DisconnectPacket : Packet
    {
        public string Reason { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Reason = stream.ReadString16(100);
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(Reason);
        }
    }

    public class MapDataPacket : Packet
    {
        //Unknown fields
        public short UnknownConstantValue { get; set; }
        public short UnknownMapId { get; set; }
        //Text length of the Text Byte array
        public byte TextLength { get; set; }
        public byte[] Text { get; set; }

        public override void Read(BigEndianStream stream)
        {
            UnknownConstantValue = stream.ReadShort();
            UnknownMapId = stream.ReadShort();
            TextLength = stream.ReadByte();
            Text = stream.ReadBytes(TextLength);
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(UnknownConstantValue);
            stream.Write(UnknownMapId);
            stream.Write(TextLength);
            for (int i = 0; i < TextLength; i++)
                stream.Write(Text[i]);
        }
    }

    public class NewInvalidStatePacket : Packet
    {
        public NewInvalidReason Reason { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Reason = (NewInvalidReason)stream.ReadByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write((byte)Reason);
        }

        public enum NewInvalidReason : byte
        {
            InvalidBed = 0,
            BeginRaining = 1,
            EndRaining = 2
        }
    }

    public class IncrementStatisticPacket : Packet
    {
        public int Statistic { get; set; }
        public byte Amount { get; set; }

        public override void Read(BigEndianStream stream)
        {
            Statistic = stream.ReadInt();
            Amount = stream.ReadByte();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(Statistic);
            stream.Write(Amount);
        }

        public enum Statistics
        {
            StartGame = 1000,
            CreateWorld = 1001,
            LoadWorld = 1002,
            JoinMultiplayer = 1003,
            LeaveGame = 1004,
            PlayOneMinute = 1100,
            WalkOneCm = 2000,
            SwimOneCm = 2001,
            FallOneCm = 2002,
            ClimbOneCm = 2003,
            FlyOneCm = 2004,
            DiveOneCm = 2005,
            MinecartOneCm = 2006,
            BoatOneCm = 2007,
            PigOneCm = 2008,
            Jump = 2010,
            Drop = 2011,
            DamageDealt = 2020,
            DamageTaken = 2021,
            Deaths = 2022,
            MobKills = 2023,
            PlayerKills = 2024,
            FishCaught = 2025,
            MineBlock = 16777216,	// Note: Add an item ID to this value
            CraftItem = 16842752,	// Note: Add an item ID to this value
            UseItem = 16908288,		// Note: Add an item ID to this value
            BreakItem = 16973824	// Note: Add an item ID to this value
        }
    }
    public class ThunderBoltPacket : Packet
    {
        public int EntityId { get; set; }
        public bool Unknown { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public override void Read(BigEndianStream stream)
        {
            EntityId = stream.ReadInt();
            Unknown = stream.ReadBool();
            X = stream.ReadDoublePacked();
            Y = stream.ReadDoublePacked();
            Z = stream.ReadDoublePacked();
        }

        public override void Write(BigEndianStream stream)
        {
            stream.Write(EntityId);
            stream.Write(Unknown);
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
        }
    }
}
