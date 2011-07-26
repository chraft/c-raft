using System;
using Chraft.Interfaces;
using Chraft.World;
using Chraft.Entity;

namespace Chraft.Net.Packets
{
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
			return PacketMap.GetPacketType(this.GetType());
		}

		public static Packet Read(PacketType type, BigEndianStream stream)
		{
			try
			{
				Type ptype = PacketMap.Map[type];
				Packet packet = (Packet)ptype.GetConstructor(new Type[0]).Invoke(new object[0]);
				packet.Read(stream);
				return packet;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error processing packet of type {0}: {1}", type, ex);
				return null;
			}
		}
	}

	public class KeepAlivePacket : Packet
	{
		public override void Read(BigEndianStream stream)
		{
		}

		public override void Write(BigEndianStream stream)
		{
		}
	}

	public class LoginRequestPacket : Packet
	{
		public int ProtocolOrEntityId { get; set; }
		public string Username { get; set; }
		public long MapSeed { get; set; }
		public sbyte Dimension { get; set; }

		public override void Read(BigEndianStream stream)
		{
			ProtocolOrEntityId = stream.ReadInt();
			Username = stream.ReadString16(16);
			MapSeed = stream.ReadLong();
			Dimension = stream.ReadSByte();
		}

		public override void Write(BigEndianStream stream)
		{
			stream.Write(ProtocolOrEntityId);
			stream.Write(Username);
			stream.Write(MapSeed);
			stream.Write(Dimension);
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
        public byte World { get; set; }

		public override void Read(BigEndianStream stream)
		{
		    World = stream.ReadByte();
		}

		public override void Write(BigEndianStream stream)
		{
            stream.Write(World);
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
		public sbyte Type { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }

		public override void Read(BigEndianStream stream)
		{
			EntityId = stream.ReadInt();
			Type = stream.ReadSByte();
			X = (double)stream.ReadInt() / 32.0d;
			Y = (double)stream.ReadInt() / 32.0d;
			Z = (double)stream.ReadInt() / 32.0d;
		}

		public override void Write(BigEndianStream stream)
		{
			stream.Write(EntityId);
			stream.Write(Type);
			stream.Write((int)(X * 32));
			stream.Write((int)(Y * 32));
			stream.Write((int)(Z * 32));
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

	public class PlayNoteBlockPacket : Packet
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public sbyte Instrument { get; set; }
		public sbyte Pitch { get; set; }

		public override void Read(BigEndianStream stream)
		{
			X = stream.ReadInt();
			Y = stream.ReadInt();
			Z = stream.ReadInt();
			Instrument = stream.ReadSByte();
			Pitch = stream.ReadSByte();
		}

		public override void Write(BigEndianStream stream)
		{
			stream.Write(X);
			stream.Write(Y);
			stream.Write(Z);
			stream.Write(Instrument);
			stream.Write(Pitch);
		}
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
		public sbyte WindowId { get; set; }
		public short ProgressBar { get; set; }
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
}
