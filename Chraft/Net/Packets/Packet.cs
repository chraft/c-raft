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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Chraft.Interfaces;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;
using Chraft.World;
using Chraft.Entity;
using Chraft.PluginSystem;

namespace Chraft.Net.Packets
{
    /// <summary>
    /// Contains all the packet read / write methods.
    /// Propeties must be read in order as specified by the protocol http://mc.kev009.com/Protocol
    /// use sbytes for handling bytes of negative value (-128 to 127) otherwise normal bytes (0-255) are fine
    /// </summary>

    public abstract class Packet : IPacket
    {
        public static StreamRole Role;
        public abstract void Read(PacketReader reader);
        public abstract void Write();

        protected PacketWriter Writer;

        private int _Length;
        public bool Shared { get; private set; }
        public int Written;
        public Logger Logger;

        private byte[] _buffer;

        private int _sharesNum;
        protected virtual int Length { get { return _Length; } set { _Length = value; } }

        public PacketType GetPacketType()
        {
            return PacketMap.GetPacketType(GetType());
        }

        protected Packet()
        {
        }

        public void SetCapacity()
        {
            Writer = PacketWriter.CreateInstance(Length);
            Writer.Write((byte)GetPacketType());
        }

        public void SetCapacity(int fixedLength)
        {
            _Length = fixedLength;
            SetCapacity();
        }

        public void SetCapacity(int fixedLength, params string[] args)
        {
            byte[] bytes;

            _Length = fixedLength;
            Queue<byte[]> strings = new Queue<byte[]>();
            for (int i = 0; i < args.Length; ++i)
            {
                bytes = ASCIIEncoding.BigEndianUnicode.GetBytes(args[i]);
                _Length += bytes.Length;
                strings.Enqueue(bytes);
            }

            Writer = PacketWriter.CreateInstance(Length, strings);
            Writer.Write((byte)GetPacketType());
        }

        public void SetShared(ILogger logger, int num)
        {
            Logger = (Logger)logger;
            if (num == 0)
            {
                _sharesNum = 1;
                Logger.Log(LogLevel.Error, "Packet {0}, shares must be > 0!! Setting to 1", ToString());
            }
            Shared = true;
            _sharesNum = num;
            Write();

            _buffer = new byte[Length];
            byte[] underlyingBuffer = Writer.UnderlyingStream.GetBuffer();
            try
            {
                Buffer.BlockCopy(underlyingBuffer, 0, _buffer, 0, Length);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Writer {0}, Request {1} \r\n{2}", underlyingBuffer.Length, Length, e));
            }
        }

        public void Release()
        {
            if (!Shared)
            {
                PacketWriter.ReleaseInstance(Writer);
                _buffer = null;
            }
            else
            {
                int shares = Interlocked.Decrement(ref _sharesNum);

                if (shares == 0)
                {
                    PacketWriter.ReleaseInstance(Writer);
                    _buffer = null;
                }
            }
        }

        public byte[] GetBuffer()
        {
            if (!Shared)
            {
                _buffer = new byte[Length];
                byte[] underlyingBuffer = Writer.UnderlyingStream.GetBuffer();
                try
                {
                    Buffer.BlockCopy(underlyingBuffer, 0, _buffer, 0, Length);
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("Writer {0}, Request {1} \r\n{2}", underlyingBuffer.Length, Length, e));
                }
            }

            return _buffer;
        }
    }

    public class KeepAlivePacket : Packet
    {
        public int KeepAliveID { get; set; }
        protected override int Length { get { return 5; } }

        public override void Read(PacketReader stream)
        {
            KeepAliveID = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(KeepAliveID);
        }
    }

    public class LoginRequestPacket : Packet
    {
        public int ProtocolOrEntityId { get; set; }
        public string Username { get; set; }
        public string Level_Type { get; set; }
        public int ServerMode { get; set; }
        public int Dimension { get; set; }
        public sbyte Difficulty { get; set; }
        public byte WorldHeight { get; set; }
        public byte MaxPlayers { get; set; }

        public override void Read(PacketReader reader)
        {
            ProtocolOrEntityId = reader.ReadInt();
            Username = reader.ReadString16(16);
            Level_Type = reader.ReadString16(9);
            ServerMode = reader.ReadInt();
            Dimension = reader.ReadInt();
            Difficulty = reader.ReadSByte();
            WorldHeight = reader.ReadByte();
            MaxPlayers = reader.ReadByte();
        }

        public override void Write()
        {
            string levelType = "DEFAULT"; // TODO: get from config
            SetCapacity(20, Username, levelType);
            Writer.Write(ProtocolOrEntityId);
            Writer.Write(Username);
            Writer.Write(levelType);
            Writer.Write(ServerMode);
            Writer.Write(Dimension);
            Writer.Write(Difficulty);
            Writer.Write(WorldHeight);
            Writer.Write(MaxPlayers);
        }

        public enum LevelType
        {
            DEFAULT,
            SUPERFLAT
        }
    }

    public class HandshakePacket : Packet
    {
        public string UsernameAndIpOrHash { get; set; }

        public override void Read(PacketReader stream)
        {
            UsernameAndIpOrHash = stream.ReadString16(64);
        }

        public override void Write()
        {
            SetCapacity(3, UsernameAndIpOrHash);
            Writer.Write(UsernameAndIpOrHash);
        }
    }

    public class ChatMessagePacket : Packet
    {
        public string Message { get; set; }

        public override void Read(PacketReader stream)
        {
            Message = stream.ReadString16(100);
        }

        public override void Write()
        {
            SetCapacity(3, Message);
            Writer.Write(Message);
        }
    }

    public class TimeUpdatePacket : Packet
    {
        public long Time { get; set; }
        protected override int Length { get { return 9; } }

        public override void Read(PacketReader stream)
        {
            Time = stream.ReadLong();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(Time);
        }
    }

    public class EntityEquipmentPacket : Packet
    {
        public int EntityId { get; set; }
        public short Slot { get; set; }
        public short ItemId { get; set; }
        public short Durability { get; set; }

        protected override int Length { get { return 11; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Slot = stream.ReadShort();
            ItemId = stream.ReadShort();
            Durability = stream.ReadShort();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(Slot);
            Writer.Write(ItemId);
            Writer.Write(Durability);
        }
    }

    public class SpawnPositionPacket : Packet
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        protected override int Length { get { return 13; } }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadInt();
            Z = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
        }
    }

    public class UseEntityPacket : Packet
    {
        public int User { get; set; }
        public int Target { get; set; }
        public bool LeftClick { get; set; }

        public override void Read(PacketReader stream)
        {
            User = stream.ReadInt();
            Target = stream.ReadInt();
            LeftClick = stream.ReadBool();
        }

        public override void Write()
        {
            Writer.Write(User);
            Writer.Write(Target);
            Writer.Write(LeftClick);
        }
    }

    public class UpdateHealthPacket : Packet
    {
        public short Health { get; set; }
        public short Food { get; set; }
        public float FoodSaturation { get; set; }

        protected override int Length { get { return 9; } }

        public override void Read(PacketReader stream)
        {
            Health = stream.ReadShort();
            Food = stream.ReadShort();
            FoodSaturation = stream.ReadFloat();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(Health);
            Writer.Write(Food);
            Writer.Write(FoodSaturation);
        }
    }

    public class RespawnPacket : Packet
    {
        public int World { get; set; }
        public sbyte Difficulty { get; set; }
        public sbyte CreativeMode { get; set; } // 0 for survival, 1 for creative.
        public short WorldHeight { get; set; } // Default 128
        public string LevelType { get; set; } // TODO: read from config

        public override void Read(PacketReader stream)
        {
            World = stream.ReadInt();
            Difficulty = stream.ReadSByte();
            CreativeMode = stream.ReadSByte();
            WorldHeight = stream.ReadShort();
            LevelType = stream.ReadString16(9);
        }

        public override void Write()
        {
            string levelType = "DEFAULT";
            SetCapacity(11, levelType);
            Writer.Write(World);
            Writer.Write(Difficulty);
            Writer.Write(CreativeMode);
            Writer.Write(WorldHeight);
            Writer.Write(levelType);
        }
    }

    public class PlayerPacket : Packet
    {
        public bool OnGround { get; set; }

        protected override int Length { get { return 2; } }

        public override void Read(PacketReader stream)
        {
            OnGround = stream.ReadBool();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(OnGround);
        }
    }

    /// <summary>
    /// Sent by the notchian server to update the user list (<tab> in the client). The server sends one packet per user per tick, amounting to 20 packets/s for 1 online user, 40 for 2, and so forth.
    /// </summary>
    public class PlayerListItemPacket : Packet
    {
        public string PlayerName { get; set; }
        public bool Online { get; set; }
        public short Ping { get; set; }

        public override void Read(PacketReader stream)
        {
            PlayerName = stream.ReadString16(16);
            Online = stream.ReadBool();
            Ping = stream.ReadShort();
        }

        public override void Write()
        {
            SetCapacity(6, PlayerName);
            Writer.Write(PlayerName);
            Writer.Write(Online);
            Writer.Write(Ping);
        }
    }

    public class PlayerPositionPacket : Packet
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Stance { get; set; }
        public double Z { get; set; }
        public bool OnGround { get; set; }

        protected override int Length { get { return 34; } }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadDouble();
            Y = stream.ReadDouble();
            Stance = stream.ReadDouble();
            Z = stream.ReadDouble();
            OnGround = stream.ReadBool();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Stance);
            Writer.Write(Z);
            Writer.Write(OnGround);
        }
    }

    public class PlayerRotationPacket : Packet
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public bool OnGround { get; set; }

        public override void Read(PacketReader stream)
        {
            Yaw = stream.ReadFloat();
            Pitch = stream.ReadFloat();
            OnGround = stream.ReadBool();
        }

        public override void Write()
        {
            Writer.Write(Yaw);
            Writer.Write(Pitch);
            Writer.Write(OnGround);
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

        protected override int Length { get { return 42; } }

        public override void Read(PacketReader stream)
        {
            //X,Y,Stance are in different order for Client->Server vs. Server->Client
            if (Packet.Role == StreamRole.Server)
            {
                X = stream.ReadDouble();
                Stance = stream.ReadDouble();
                Y = stream.ReadDouble();
            }
            else
            {
                X = stream.ReadDouble();
                Y = stream.ReadDouble();
                Stance = stream.ReadDouble();
            }
            Z = stream.ReadDouble();
            Yaw = stream.ReadFloat();
            Pitch = stream.ReadFloat();
            OnGround = stream.ReadBool();
        }

        public override void Write()
        {
            SetCapacity();
            //X,Y,Stance are in different order for Client->Server vs. Server->Client
            if (Packet.Role == StreamRole.Server)
            {
                Writer.Write(X);
                Writer.Write(Y);
                Writer.Write(Stance);
            }
            else
            {
                Writer.Write(X);
                Writer.Write(Stance);
                Writer.Write(Y);
            }
            Writer.Write(Z);
            Writer.Write(Yaw);
            Writer.Write(Pitch);
            Writer.Write(OnGround);
        }
    }

    public class PlayerDiggingPacket : Packet
    {
        public DigAction Action { get; set; }
        public int X { get; set; }
        public sbyte Y { get; set; }
        public int Z { get; set; }
        public sbyte Face { get; set; }

        public override void Read(PacketReader stream)
        {
            Action = (DigAction)stream.ReadByte();
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();
            Face = stream.ReadSByte();
        }

        public override void Write()
        {
            Writer.Write((byte)Action);
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(Face);
        }
        public enum DigAction : byte
        {
            StartDigging = 0,
            FinishDigging = 2,
            DropItem = 4,
            ShootArrow = 5
        }
    }

    public class PlayerBlockPlacementPacket : Packet
    {
        public int X { get; set; }
        public sbyte Y { get; set; }
        public int Z { get; set; }
        public BlockFace Face { get; set; }
        public ItemStack Item { get; set; }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();
            Face = (BlockFace)stream.ReadSByte();
            Item = ItemStack.Read(stream);
            //amount in hand and durability are handled int ItemStack.Read
        }

        public override void Write()
        {
            if (Item == null || Item.Type == -1)
                SetCapacity(13);
            else
                SetCapacity(16);

            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write((sbyte)Face);
            (Item ?? ItemStack.Void).Write(Writer);
            Length = (int)Writer.UnderlyingStream.Length;
        }
    }

    public class HoldingChangePacket : Packet
    {
        public short Slot { get; set; }

        public override void Read(PacketReader stream)
        {
            Slot = stream.ReadShort();
        }

        public override void Write()
        {
            Writer.Write(Slot);
        }
    }

    public class UseBedPacket : Packet
    {
        public int PlayerId { get; set; }
        public sbyte InBed { get; set; }
        public int X { get; set; }
        public sbyte Y { get; set; }
        public int Z { get; set; }

        protected override int Length { get { return 15; } }

        public override void Read(PacketReader stream)
        {
            PlayerId = stream.ReadInt();
            InBed = stream.ReadSByte();
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(PlayerId);
            Writer.Write(InBed);
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
        }
    }

    public class AnimationPacket : Packet
    {
        public int PlayerId { get; set; }
        public sbyte Animation { get; set; }

        protected override int Length { get { return 6; } }

        public override void Read(PacketReader stream)
        {
            PlayerId = stream.ReadInt();
            Animation = stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(PlayerId);
            Writer.Write(Animation);
        }
    }

    public class EntityActionPacket : Packet
    {
        public enum ActionType : sbyte
        {
            Crouch = 1,
            Uncrouch = 2,
            LeaveBed = 3,
            StartSprinting = 4,
            StopSprinting = 5,
        }

        public int PlayerId { get; set; }
        public ActionType Action { get; set; }

        protected override int Length { get { return 6; } }

        public override void Read(PacketReader stream)
        {
            PlayerId = stream.ReadInt();
            Action = (ActionType)stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(PlayerId);
            Writer.Write((sbyte)Action);
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

        public override void Read(PacketReader stream)
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

        public override void Write()
        {
            SetCapacity(23, PlayerName);
            Writer.Write(EntityId);
            Writer.Write(PlayerName);
            Writer.Write((int)Math.Floor(X * 32.0));
            Writer.Write((int)Math.Floor(Y * 32.0));
            Writer.Write((int)Math.Floor(Z * 32.0));
            Writer.Write(Yaw);
            Writer.Write(Pitch);
            Writer.Write(CurrentItem);
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

        protected override int Length { get { return 25; } }

        public override void Read(PacketReader stream)
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

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(ItemId);
            Writer.Write(Count);
            Writer.Write(Durability);
            Writer.Write((int)(X * 32));
            Writer.Write((int)(Y * 32));
            Writer.Write((int)(Z * 32));
            Writer.Write(Yaw);
            Writer.Write(Pitch);
            Writer.Write(Roll);
        }
    }

    public class CollectItemPacket : Packet
    {
        public int EntityId { get; set; }
        public int PlayerId { get; set; }

        protected override int Length { get { return 9; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            PlayerId = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(PlayerId);
        }
    }

    public class AddObjectVehiclePacket : Packet
    {
        public int EntityId { get; set; }
        public ObjectType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int FireBallThrowerEid { get; set; }
        public short FireBallX { get; set; }
        public short FireBallY { get; set; }
        public short FireBallZ { get; set; }

        protected override int Length { get { return FireBallThrowerEid > 0 ? 28 : 22; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Type = (ObjectType)stream.ReadSByte();
            X = stream.ReadInt() / 32.0d; // ((double)intX / 32.0d) => representation of X as double
            Y = stream.ReadInt() / 32.0d;
            Z = stream.ReadInt() / 32.0d;
            FireBallThrowerEid = stream.ReadInt();
            if (FireBallThrowerEid != 0)
            {
                FireBallX = stream.ReadShort();
                FireBallY = stream.ReadShort();
                FireBallZ = stream.ReadShort();
            }
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write((sbyte)Type);
            Writer.Write((int)(X * 32));
            Writer.Write((int)(Y * 32));
            Writer.Write((int)(Z * 32));
            Writer.Write(FireBallThrowerEid);
            if (FireBallThrowerEid != 0)
            {
                Writer.Write(FireBallX);
                Writer.Write(FireBallY);
                Writer.Write(FireBallZ);
            }
        }

        public enum ObjectType : sbyte
        {
            Boat = 1,
            Minecart = 10,
            StorageCart = 11,
            PoweredCart = 12,
            ActivatedTNT = 50,
            EnderCrystal = 51,
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
        public sbyte HeadYaw { get; set; }
        public MetaData Data { get; set; }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Type = (MobType)stream.ReadByte();
            X = stream.ReadInt() / 32.0d;
            Y = stream.ReadInt() / 32.0d;
            Z = stream.ReadInt() / 32.0d;
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
            HeadYaw = stream.ReadSByte();
            Data = stream.ReadMetaData();
        }

        public override void Write()
        {
            SetCapacity(22);
            Writer.Write(EntityId);
            Writer.Write((byte)Type);
            Writer.Write((int)(X * 32));
            Writer.Write((int)(Y * 32));
            Writer.Write((int)(Z * 32));
            Writer.Write(Yaw);
            Writer.Write(Pitch);
            Writer.Write(HeadYaw);
            Writer.Write(Data);
            // This is because we don't know the dimension of Data in advance
            Length = (int)Writer.UnderlyingStream.Length;
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

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Title = stream.ReadString16(13);
            X = stream.ReadInt();
            Y = stream.ReadInt();
            Z = stream.ReadInt();
            GraphicId = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity(23, Title);
            Writer.Write(EntityId);
            Writer.Write(Title);
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(GraphicId);
        }
    }

    public class EntityVelocityPacket : Packet
    {
        public int EntityId { get; set; }
        public short VelocityX { get; set; }
        public short VelocityY { get; set; }
        public short VelocityZ { get; set; }

        protected override int Length { get { return 11; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            VelocityX = stream.ReadShort();
            VelocityY = stream.ReadShort();
            VelocityZ = stream.ReadShort();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(VelocityX);
            Writer.Write(VelocityY);
            Writer.Write(VelocityZ);
        }
    }

    public class DestroyEntityPacket : Packet
    {
        public int EntityId { get; set; }

        protected override int Length { get { return 5; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
        }
    }

    public class CreateEntityPacket : Packet
    {
        public int EntityId { get; set; }

        protected override int Length { get { return 5; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
        }
    }

    public class EntityRelativeMovePacket : Packet
    {
        public int EntityId { get; set; }
        public sbyte DeltaX { get; set; }
        public sbyte DeltaY { get; set; }
        public sbyte DeltaZ { get; set; }

        protected override int Length { get { return 8; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            DeltaX = stream.ReadSByte();
            DeltaY = stream.ReadSByte();
            DeltaZ = stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(DeltaX);
            Writer.Write(DeltaY);
            Writer.Write(DeltaZ);
        }
    }

    public class EntityLookPacket : Packet
    {
        public int EntityId { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }

        protected override int Length { get { return 7; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(Yaw);
            Writer.Write(Pitch);
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

        protected override int Length { get { return 10; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            DeltaX = stream.ReadSByte();
            DeltaY = stream.ReadSByte();
            DeltaZ = stream.ReadSByte();
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(DeltaX);
            Writer.Write(DeltaY);
            Writer.Write(DeltaZ);
            Writer.Write(Yaw);
            Writer.Write(Pitch);
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

        protected override int Length { get { return 19; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            X = (double)stream.ReadInt() / 32.0d;
            Y = (double)stream.ReadInt() / 32.0d;
            Z = (double)stream.ReadInt() / 32.0d;
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write((int)Math.Floor(X * 32.0));
            Writer.Write((int)Math.Floor(Y * 32.0));
            Writer.Write((int)Math.Floor(Z * 32.0));
            Writer.Write(Yaw);
            Writer.Write(Pitch);
        }
    }

    public class EntityStatusPacket : Packet
    {
        public int EntityId { get; set; }
        public sbyte EntityStatus { get; set; }

        protected override int Length { get { return 6; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            EntityStatus = stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(EntityStatus);
        }
    }

    public class AttachEntityPacket : Packet
    {
        public int EntityId { get; set; }
        public int VehicleId { get; set; }

        protected override int Length { get { return 9; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            VehicleId = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(VehicleId);
        }
    }

    public class EntityMetadataPacket : Packet
    {
        public int EntityId { get; set; }
        public MetaData Data { get; set; }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Data = stream.ReadMetaData();
        }

        public override void Write()
        {
            SetCapacity(6);
            Writer.Write(EntityId);
            Writer.Write(Data);

            Length = (int)Writer.UnderlyingStream.Length;
        }
    }

    public class PreChunkPacket : Packet
    {
        public int X { get; set; }
        public int Z { get; set; }
        public bool Load { get; set; }

        protected override int Length { get { return 10; } }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadInt();
            Z = stream.ReadInt();
            Load = stream.ReadBool();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(X);
            Writer.Write(Z);
            Writer.Write(Load);
        }
    }

    public class MultiBlockChangePacket : Packet
    {
        public UniversalCoords ChunkCoords { get; set; }
        public short[] CoordsArray { get; set; }
        public sbyte[] Types { get; set; }
        public sbyte[] Metadata { get; set; }

        public override void Read(PacketReader stream)
        {
            ChunkCoords = UniversalCoords.FromChunk(stream.ReadInt(), stream.ReadInt());
            short length = stream.ReadShort();
            CoordsArray = new short[length];
            Types = new sbyte[length];
            Metadata = new sbyte[length];
            for (int i = 0; i < CoordsArray.Length; i++)
                CoordsArray[i] = stream.ReadShort();
            for (int i = 0; i < Types.Length; i++)
                Types[i] = stream.ReadSByte();
            for (int i = 0; i < Metadata.Length; i++)
                Metadata[i] = stream.ReadSByte();

        }

        public override void Write()
        {
            SetCapacity(15 + (CoordsArray.Length * 4));
            Writer.Write(ChunkCoords.ChunkX);
            Writer.Write(ChunkCoords.ChunkZ);

            Writer.Write((short)CoordsArray.Length);
            Writer.Write(CoordsArray.Length * 4);
            for (int i = 0; i < CoordsArray.Length; i++)
            {
                Writer.Write(CoordsArray[i]);

                Writer.Write((short)((Types[i] & 0xFFF) << 4 | (Metadata[i] & 0xF)));
            }
        }
    }

    public class BlockChangePacket : Packet
    {
        public int X { get; set; }
        public sbyte Y { get; set; }
        public int Z { get; set; }
        public byte Type { get; set; }
        public byte Data { get; set; }

        protected override int Length { get { return 12; } }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();
            Type = stream.ReadByte();
            Data = stream.ReadByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(Type);
            Writer.Write(Data);
        }
    }

    public class BlockActionPacket : Packet
    {
        public int X { get; set; }
        public short Y { get; set; }
        public int Z { get; set; }
        public sbyte DataA { get; set; }
        public sbyte DataB { get; set; }

        protected override int Length { get { return 13; } }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadShort();
            Z = stream.ReadInt();
            DataA = stream.ReadSByte();
            DataB = stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(DataA);
            Writer.Write(DataB);
        }

        #region Note Block Action
        public void SetNoteBlockAction(int x, int y, int z, Instrument instrument, Pitch pitch)
        {
            X = x;
            Y = (short)y;
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
            Octave1_00_Fsharp = 0,
            Octave1_01_G = 1,
            Octave1_02_Gsharp = 2,
            Octave1_03_A = 3,
            Octave1_04_Asharp = 4,
            Octave1_05_B = 5,
            Octave1_06_C = 6,
            Octave1_07_Csharp = 7,
            Octave1_08_D = 8,
            Octave1_09_Dsharp = 9,
            Octave1_10_E = 10,
            Octave1_11_F = 11,
            Octave2_00_Fsharp = 12,
            Octave2_01_G = 13,
            Octave2_02_Gsharp = 14,
            Octave2_03_A = 15,
            Octave2_04_Asharp = 16,
            Octave2_05_B = 17,
            Octave2_06_Bsharp = 18,
            Octave2_07_C = 19,
            Octave2_08_Csharp = 20,
            Octave2_09_D = 21,
            Octave2_10_Dsharp = 22,
            Octave2_11_E = 23,
            Octave2_12_F = 24,
        }
        #endregion

        #region Piston Action

        public void SetPistonAction(int x, int y, int z, PistonState state, PistonDirection direction)
        {
            X = x;
            Y = (short)y;
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

        public override void Read(PacketReader stream)
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

        public override void Write()
        {
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(Radius);
            Writer.Write((int)Offsets.GetLength(0));
            for (int i = 0; i < Offsets.GetLength(0); i++)
            {
                Writer.Write(Offsets[i, 0]);
                Writer.Write(Offsets[i, 1]);
                Writer.Write(Offsets[i, 2]);
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

        protected override int Length { get { return 18; } }

        public override void Read(PacketReader stream)
        {
            EffectID = (SoundEffect)stream.ReadInt();
            X = stream.ReadInt();
            Y = stream.ReadByte();
            Z = stream.ReadInt();
            SoundData = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write((int)EffectID);
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(SoundData);
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

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            InventoryType = (InterfaceType)stream.ReadSByte();
            WindowTitle = stream.ReadString16(100);
            SlotCount = stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity(6, WindowTitle);
            Writer.Write(WindowId);
            Writer.Write((sbyte)InventoryType);
            Writer.Write(WindowTitle);
            Writer.Write(SlotCount);
        }
    }

    public class CreativeInventoryActionPacket : Packet
    {
        public short Slot { get; set; }
        public ItemStack Item { get; set; }

        public override void Read(PacketReader stream)
        {
            Slot = stream.ReadShort();
            Item = ItemStack.Read(stream);
        }

        public override void Write()
        {
            Writer.Write(Slot);
            if (Item == null || Item.Type == -1)
                SetCapacity(5);
            else
                SetCapacity(8);

            Length = (int)Writer.UnderlyingStream.Length;
        }
    }

    public class CloseWindowPacket : Packet
    {
        public sbyte WindowId { get; set; }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
        }

        public override void Write()
        {
            Writer.Write(WindowId);
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

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            Slot = stream.ReadShort();
            RightClick = stream.ReadBool();
            Transaction = stream.ReadShort();
            Shift = stream.ReadBool();
            Item = ItemStack.Read(stream);
        }

        public override void Write()
        {
            if (Item == null || Item.Type == -1)
                SetCapacity(10);
            else
                SetCapacity(13);

            Writer.Write(WindowId);
            Writer.Write(Slot);
            Writer.Write(RightClick);
            Writer.Write(Transaction);
            Writer.Write(Shift);
            (Item ?? ItemStack.Void).Write(Writer);
            Length = (int)Writer.UnderlyingStream.Length;
        }
    }

    public class SetSlotPacket : Packet
    {
        public sbyte WindowId { get; set; }
        public short Slot { get; set; }
        public ItemStack Item { get; set; }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            Slot = stream.ReadShort();
            Item = ItemStack.Read(stream);
        }

        public override void Write()
        {
            if (Item == null || Item.Type == -1)
                SetCapacity(6);
            else
                SetCapacity(9);

            Writer.Write(WindowId);
            Writer.Write(Slot);
            (Item ?? ItemStack.Void).Write(Writer);
            Length = (int)Writer.UnderlyingStream.Length;
        }
    }

    public class WindowItemsPacket : Packet
    {
        public sbyte WindowId { get; set; }
        public ItemStack[] Items { get; set; }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            Items = new ItemStack[stream.ReadShort()];
            for (int i = 0; i < Items.Length; i++)
                Items[i] = ItemStack.Read(stream);
        }

        public override void Write()
        {
            SetCapacity(4);
            Writer.Write(WindowId);
            Writer.Write((short)Items.Length);
            for (int i = 0; i < Items.Length; i++)
                (Items[i] ?? ItemStack.Void).Write(Writer);

            Length = (int)Writer.UnderlyingStream.Length;
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

        protected override int Length { get { return 6; } }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            ProgressBar = stream.ReadShort();
            Value = stream.ReadShort();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(WindowId);
            Writer.Write(ProgressBar);
            Writer.Write(Value);
        }
    }

    public class TransactionPacket : Packet
    {
        public sbyte WindowId { get; set; }
        public short Transaction { get; set; }
        public bool Accepted { get; set; }

        protected override int Length { get { return 5; } }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            Transaction = stream.ReadShort();
            Accepted = stream.ReadBool();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(WindowId);
            Writer.Write(Transaction);
            Writer.Write(Accepted);
        }
    }

    public class UpdateSignPacket : Packet
    {
        public int X { get; set; }
        public short Y { get; set; }
        public int Z { get; set; }
        public string[] Lines { get; set; }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadShort();
            Z = stream.ReadInt();
            Lines = new string[4];
            for (int i = 0; i < Lines.Length; i++)
                Lines[i] = stream.ReadString16(15);
        }

        public override void Write()
        {
            SetCapacity(19, Lines[0], Lines[1], Lines[2], Lines[3]);
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            for (int i = 0; i < Lines.Length; i++)
                Writer.Write(Lines[i]);
        }
    }

    /// <summary>
    /// To load server info in the multiplayer menu, the notchian client connects to each known server and sends an 0xFE.
    /// In return, the server sends a kick (0xFF), with its string containing data (server description, number of users, number of slots), delimited by a §
    /// </summary>
    public class ServerListPingPacket : Packet
    {
        protected override int Length { get { return 1; } }

        public override void Read(PacketReader stream)
        {
        }

        public override void Write()
        {
            SetCapacity();
        }
    }

    public class DisconnectPacket : Packet
    {
        public string Reason { get; set; }

        public override void Read(PacketReader stream)
        {
            Reason = stream.ReadString16(100);
        }

        public override void Write()
        {
            SetCapacity(3, Reason);
            Writer.Write(Reason);
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

        protected override int Length { get { return 6 + Text.Length; } }

        public override void Read(PacketReader stream)
        {
            UnknownConstantValue = stream.ReadShort();
            UnknownMapId = stream.ReadShort();
            TextLength = stream.ReadByte();
            Text = stream.ReadBytes(TextLength);
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(UnknownConstantValue);
            Writer.Write(UnknownMapId);
            Writer.Write(TextLength);
            for (int i = 0; i < TextLength; i++)
                Writer.Write(Text[i]);
        }
    }

    public class NewInvalidStatePacket : Packet
    {
        public NewInvalidReason Reason { get; set; }
        public byte GameMode { get; set; }

        protected override int Length { get { return Reason == NewInvalidReason.ChangeGameMode ? 3 : 2; } }

        public override void Read(PacketReader stream)
        {
            Reason = (NewInvalidReason)stream.ReadByte();
            if (Reason == NewInvalidReason.ChangeGameMode)
            {
                GameMode = stream.ReadByte();
            }
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write((byte)Reason);
            if (Reason == NewInvalidReason.ChangeGameMode)
            {
                Writer.Write(GameMode);
            }
        }

        public enum NewInvalidReason : byte
        {
            InvalidBed = 0,
            BeginRaining = 1,
            EndRaining = 2,
            ChangeGameMode = 3,
            EndGameCredits = 4
        }
    }

    public class IncrementStatisticPacket : Packet
    {
        public Statistics Statistic { get; set; }
        public byte Amount { get; set; }

        protected override int Length { get { return 6; } }

        public override void Read(PacketReader stream)
        {
            Statistic = (Statistics)stream.ReadInt();
            Amount = stream.ReadByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write((int)Statistic);
            Writer.Write(Amount);
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

        protected override int Length { get { return 18; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Unknown = stream.ReadBool();
            X = stream.ReadDoublePacked();
            Y = stream.ReadDoublePacked();
            Z = stream.ReadDoublePacked();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(Unknown);
            Writer.Write((int)X);
            Writer.Write((int)Y);
            Writer.Write((int)Z);
        }
    }

    public class ExperienceOrbPacket : Packet
    {

        public int EntityId { get; set; }
        public short Count { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        protected override int Length { get { return 19; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            X = stream.ReadInt();
            Y = stream.ReadInt();
            Z = stream.ReadInt();
            Count = stream.ReadShort();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(Count);
        }
    }

    public class EntityEffectPacket : Packet
    {
        public int EntityId { get; set; }
        public EntityEffects Effect { get; set; }
        public byte Amplifier { get; set; }
        public short Duration { get; set; }

        protected override int Length { get { return 9; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Effect = (EntityEffects)stream.ReadByte();
            Amplifier = stream.ReadByte();
            Duration = stream.ReadShort();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write((byte)Effect);
            Writer.Write(Amplifier);
            Writer.Write(Duration);
        }
    }

    public class RemoveEntityEffectPacket : Packet
    {
        public int EntityId { get; set; }
        public EntityEffects Effect { get; set; }

        protected override int Length { get { return 6; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Effect = (EntityEffects)stream.ReadByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write((byte)Effect);
        }
    }

    public enum EntityEffects
    {
        MoveSpeed = 1, // Increases player speed and FOV.
        MoveSlowDown = 2, // Decreases player speed and FOV.
        DigSpeed = 3, // Increases player dig speed
        DigSlowDown = 4, // Decreases player dig speed
        DamageBoost = 5,
        Heal = 6,
        Harm = 7,
        Jump = 8,
        Confusion = 9, //Portal-like effect
        Regeneration = 10, //Hearts pulse one-by-one - Caused by golden apple. Health regenerates over 600-tick (30s) period.
        Resistance = 11,
        FireResistance = 12,
        WaterResistance = 13,
        Invisibility = 14,
        Blindness = 15,
        NightVision = 16,
        Hunger = 17, //Food bar turns green - Caused by poisoning from Rotten Flesh or Raw Chicken
        Weakness = 18,
        Poison = 19 //Hearts turn yellow - Caused by poisoning from cave (blue) spider
    }

    public class EnchantItemPacket : Packet
    {
        public byte WindowId { get; set; }
        public byte Enchantment { get; set; }

        protected override int Length { get { return 3; } }

        public override void Read(PacketReader reader)
        {
            WindowId = reader.ReadByte();
            Enchantment = reader.ReadByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.WriteByte(WindowId);
            Writer.WriteByte(Enchantment);
        }
    }

    public class ExperiencePacket : Packet
    {
        public float Experience { get; set; }
        public short Level { get; set; }
        public short TotExperience { get; set; }

        protected override int Length { get { return 9; } }

        public override void Read(PacketReader reader)
        {
            Experience = reader.ReadFloat();
            Level = reader.ReadShort();
            TotExperience = reader.ReadShort();
        }

        public override void Write()
        {
            SetCapacity();

            Writer.Write(Experience);
            Writer.Write(Level);
            Writer.Write(TotExperience);
        }
    }

    public class PluginMessagePacket : Packet
    {
        public string Channel { get; set; }
        public short ByteLength { get; set; }
        public byte[] Data { get; set; }

        public override void Read(PacketReader reader)
        {
            Channel = reader.ReadString16(260);
            ByteLength = reader.ReadShort();
            Data = reader.ReadBytes(ByteLength);
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(Channel);
            Writer.Write(ByteLength);
            for (int i = 0; i < ByteLength; i++)
            {
                Writer.WriteByte(Data[i]);
            }

        }
    }

    public class EntityHeadLook : Packet
    {
        public int EntityId { get; set; }
        public sbyte HeadYaw { get; set; }

        public override void Read(PacketReader reader)
        {
            EntityId = reader.ReadInt();
            HeadYaw = reader.ReadSByte();
        }

        public override void Write()
        {
            Writer.Write(EntityId);
            Writer.Write(HeadYaw);
        }
    }

    public class UpdateTileEntity : Packet
    {
        public int X { get; set; }
        public short Y { get; set; }
        public int Z { get; set; }
        public sbyte Action { get; set; }
        public int Custom1 { get; set; }
        public int Custom2 { get; set; }
        public int Custom3 { get; set; }

        public override void Read(PacketReader reader)
        {
            X = reader.ReadInt();
            Y = reader.ReadShort();
            Z = reader.ReadInt();
            Action = reader.ReadSByte();
            Custom1 = reader.ReadInt();
            Custom2 = reader.ReadInt();
            Custom3 = reader.ReadInt();
        }

        public override void Write()
        {
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(Action);
            Writer.Write(Custom1);
            Writer.Write(Custom2);
            Writer.Write(Custom3);
        }
    }

    public class PlayerAbilitiesPacket : Packet
    {
        //Only in creative mode.

        //Invulnerabitity not used yet
        public bool Invulnerability { get; set; }
        public bool IsFlying { get; set; }
        public bool CanFly { get; set; }
        //InstantDestroy not used yet
        public bool InstantDestroy { get; set; }

        public override void Read(PacketReader reader)
        {
            Invulnerability = reader.ReadBool();
            IsFlying = reader.ReadBool();
            CanFly = reader.ReadBool();
            InstantDestroy = reader.ReadBool();
        }

        public override void Write()
        {
            SetCapacity(5);
            Writer.Write(Invulnerability);
            Writer.Write(IsFlying);
            Writer.Write(CanFly);
            Writer.Write(InstantDestroy);
        }
    }
}
