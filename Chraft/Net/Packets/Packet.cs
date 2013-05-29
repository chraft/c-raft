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
using System.Text;
using System.Threading;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Interfaces;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;

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
        public string LevelType { get; set; }
        public sbyte ServerMode { get; set; }
        public sbyte Dimension { get; set; }
        public sbyte Difficulty { get; set; }
        public byte NotUsedWorldHeight { get; set; }
        public byte MaxPlayers { get; set; }

        public override void Read(PacketReader reader)
        {
            ProtocolOrEntityId = reader.ReadInt();
            LevelType = reader.ReadString16(9);
            ServerMode = reader.ReadSByte();
            Dimension = reader.ReadSByte();
            Difficulty = reader.ReadSByte();
            NotUsedWorldHeight = reader.ReadByte();
            MaxPlayers = reader.ReadByte();
        }

        public override void Write()
        {
            
            SetCapacity(12, LevelType);
            Writer.Write(ProtocolOrEntityId);
            Writer.Write(LevelType);
            Writer.Write(ServerMode);
            Writer.Write(Dimension);
            Writer.Write(Difficulty);
            Writer.Write(0);
            Writer.Write(MaxPlayers);
        }

        public enum LevelTypeEnum
        {
            DEFAULT,
            SUPERFLAT,
            LARGEBIOMES
        }
    }

    public class HandshakePacket : Packet
    {
        public byte ProtocolVersion { get; set; }
        public string Username { get; set; }
        public string ServerHost { get; set; }
        public int ServerPort { get; set; }

        public override void Read(PacketReader stream)
        {
            ProtocolVersion = stream.ReadByte();
            Username = stream.ReadString16(64);
            ServerHost = stream.ReadString16(64);
            ServerPort = stream.ReadInt();

        }

        public override void Write()
        {
            SetCapacity(10, Username, ServerHost);
            Writer.Write(ProtocolVersion);
            Writer.Write(Username);
            Writer.Write(ServerHost);
            Writer.Write(ServerPort);
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
        public long AgeOfWorld { get; set; }
        public long Time { get; set; }
        protected override int Length { get { return 17; } }

        public override void Read(PacketReader stream)
        {
            AgeOfWorld = stream.ReadLong();
            Time = stream.ReadLong();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(AgeOfWorld);
            Writer.Write(Time);
        }
    }

    public class EntityEquipmentPacket : Packet
    {
        public int EntityId { get; set; }
        public short Slot { get; set; }
        public ItemInventory Item { get; set; }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Slot = stream.ReadShort();
            Item = ItemHelper.GetInstance(stream);
        }

        public override void Write()
        {
            SetCapacity(7);
            Writer.Write(EntityId);
            Writer.Write(Slot);
            (Item ?? ItemHelper.Void).Write(Writer);
            Length = (int)Writer.UnderlyingStream.Length;
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
        public sbyte GameMode { get; set; } // 0 for survival, 1 for creative, 2 for adventure
        public short WorldHeight { get; set; } // Default 256
        public string LevelType { get; set; } 

        public override void Read(PacketReader stream)
        {
            World = stream.ReadInt();
            Difficulty = stream.ReadSByte();
            GameMode = stream.ReadSByte();
            WorldHeight = stream.ReadShort();
            LevelType = stream.ReadString16(9);
        }

        public override void Write()
        {
            SetCapacity(11, LevelType);
            Writer.Write(World);
            Writer.Write(Difficulty);
            Writer.Write(GameMode);
            Writer.Write(WorldHeight);
            Writer.Write(LevelType);
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
            CancelledDigging = 1,
            FinishDigging = 2,
            DropItemStack = 3,
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
        public ItemInventory Item { get; set; }
        public byte CursorsX { get; set; }
        public byte CursorsY { get; set; }
        public byte CursorsZ { get; set; }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();

            Face = (BlockFace)stream.ReadSByte();
            Item = ItemHelper.GetInstance(stream);

            CursorsX = stream.ReadByte();
            CursorsY = stream.ReadByte();
            CursorsZ = stream.ReadByte();
            //amount in hand and durability are handled int ItemStack.Read
        }

        public override void Write()
        {
            SetCapacity(16);


            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);

            Writer.Write((sbyte)Face);
            (Item ?? ItemHelper.Void).Write(Writer);

            Writer.Write(CursorsX);
            Writer.Write(CursorsY);
            Writer.Write(CursorsZ);

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
        public MetaData Data { get; set; }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            PlayerName = stream.ReadString16(16);
            X = stream.ReadInt() / 32.0d;
            Y = stream.ReadInt() / 32.0d;
            Z = stream.ReadInt() / 32.0d;
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
            CurrentItem = stream.ReadShort();
            Data = stream.ReadMetaData();
        }

        public override void Write()
        {
            SetCapacity(24, PlayerName);
            Writer.Write(EntityId);
            Writer.Write(PlayerName);
            Writer.Write((int)Math.Floor(X * 32.0));
            Writer.Write((int)Math.Floor(Y * 32.0));
            Writer.Write((int)Math.Floor(Z * 32.0));
            Writer.Write(Yaw);
            Writer.Write(Pitch);
            Writer.Write(CurrentItem);
            Writer.Write(Data);
            Length = (int)Writer.UnderlyingStream.Length;
        }
    }

    public class SpawnItemPacket : Packet
    {
        public int EntityId { get; set; }
        public short Slot { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }
        public sbyte Roll { get; set; }

        protected override int Length { get { return 20; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Slot = stream.ReadShort();
            X = stream.ReadInt() / 32.0d;
            Y = stream.ReadInt() / 32.0d;
            Z = stream.ReadInt() / 32.0d;
            Yaw = stream.ReadSByte();
            Pitch = stream.ReadSByte();
            Roll = stream.ReadSByte();
        }

        public override void Write()
        {
            SetCapacity(20 + Slot);
            Writer.Write(EntityId);
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
        public byte Pitch { get; set; }
        public byte Yaw { get; set; }
        public int Data { get; set; }
        public short SpeedX { get; set; }
        public short SpeedY { get; set; }
        public short SpeedZ { get; set; }

        protected override int Length { get { return Data > 0 ? 29 : 23; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Type = (ObjectType)stream.ReadSByte();
            X = stream.ReadInt() / 32.0d; // ((double)intX / 32.0d) => representation of X as double
            Y = stream.ReadInt() / 32.0d;
            Z = stream.ReadInt() / 32.0d;
            Pitch = stream.ReadByte();
            Yaw = stream.ReadByte();

            Data = stream.ReadInt();
            if (Data != 0)
            {
                SpeedX = stream.ReadShort();
                SpeedY = stream.ReadShort();
                SpeedZ = stream.ReadShort();
            }
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write((sbyte)Type);
            Writer.Write((int)(X * 32)); //todo verify
            Writer.Write((int)(Y * 32));
            Writer.Write((int)(Z * 32));
            Writer.Write(Data);
            if (Data != 0)
            {
                Writer.Write(SpeedX);
                Writer.Write(SpeedY);
                Writer.Write(SpeedZ);
            }
        }

        public enum ObjectType : sbyte
        {
            Boat = 1,
            ItemStack = 2,
            Minecart = 10,
            StorageCart = 11,
            PoweredCart = 12,
            ActivatedTNT = 50,
            EnderCrystal = 51,
            Arrow = 60,
            ThrownSnowball = 61,
            ThrownEgg = 62,
            ThrownEnderPearl = 65,
            WitherSkull = 66,
            FallingObjects = 70,
            ItemFrames = 71,
            EyeOfEnder = 72,
            ThrownPotion = 73,
            FallingDragonEgg = 74,
            ThrownExpBottle = 75,
            FishingFloat = 90

            //todo metadata
        }
    }

    public class MobSpawnPacket : Packet
    {
        public int EntityId { get; set; }
        public MobType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public sbyte Pitch { get; set; }
        public sbyte HeadPitch { get; set; }
        public sbyte Yaw { get; set; }
        public short VelocityX { get; set; }
        public short VelocityY { get; set; }
        public short VelocityZ { get; set; }

        public MetaData Data { get; set; }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            Type = (MobType)stream.ReadByte();
            X = stream.ReadInt() / 32.0d;
            Y = stream.ReadInt() / 32.0d;
            Z = stream.ReadInt() / 32.0d;
           
            Pitch = stream.ReadSByte();
            HeadPitch = stream.ReadSByte();
            Yaw = stream.ReadSByte();
            VelocityX = stream.ReadShort();
            VelocityY = stream.ReadShort();
            VelocityZ = stream.ReadShort();
            Data = stream.ReadMetaData();
        }

        public override void Write()
        {
            SetCapacity(28);
            Writer.Write(EntityId);
            Writer.Write((byte)Type);
            Writer.Write((int)(X * 32));
            Writer.Write((int)(Y * 32));
            Writer.Write((int)(Z * 32));
            Writer.Write(Pitch);
            Writer.Write(HeadPitch);
            Writer.Write(Yaw);
            Writer.Write(VelocityX);
            Writer.Write(VelocityY);
            Writer.Write(VelocityZ);
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
        public byte EntitiesCount { get; set; }
        public int[] EntitiesId { get; set; }

        public override void Read(PacketReader stream)
        {
            EntitiesCount = stream.ReadByte();
            EntitiesId = stream.ReadIntArray(EntitiesCount);
        }

        public override void Write()
        {
            SetCapacity(2 + (EntitiesCount * sizeof(int)));

            Writer.Write(EntitiesCount);
            Writer.Write(EntitiesId);
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
            X = stream.ReadInt() / 32.0d;
            Y = stream.ReadInt() / 32.0d;
            Z = stream.ReadInt() / 32.0d;
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
        public short Type { get; set; }
        public byte Data { get; set; }

        protected override int Length { get { return 13; } }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadSByte();
            Z = stream.ReadInt();
            Type = stream.ReadShort();
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
        public short BlockId { get; set; }

        protected override int Length { get { return 15; } }

        public override void Read(PacketReader stream)
        {
            X = stream.ReadInt();
            Y = stream.ReadShort();
            Z = stream.ReadInt();
            DataA = stream.ReadSByte();
            DataB = stream.ReadSByte();
            BlockId = stream.ReadShort();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(DataA);
            Writer.Write(DataB);
            Writer.Write(BlockId);
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
        public float UnknownA { get; set; }
        public float UnknownB { get; set; }
        public float UnknownC { get; set; }

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

            UnknownA = stream.ReadFloat();
            UnknownB = stream.ReadFloat();
            UnknownC = stream.ReadFloat();

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

            Writer.Write(UnknownA);
            Writer.Write(UnknownB);
            Writer.Write(UnknownC);
        }
    }

    public class SoundOrParticleEffectPacket : Packet
    {
        /// <summary>
        /// The ID of the sound effect to play
        /// </summary>
        public SoundOrParticleEffect EffectID { get; set; }
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

        public bool DisableRelativeVolume { get; set; }

        protected override int Length { get { return 19; } }

        public override void Read(PacketReader stream)
        {
            EffectID = (SoundOrParticleEffect)stream.ReadInt();
            X = stream.ReadInt();
            Y = stream.ReadByte();
            Z = stream.ReadInt();
            SoundData = stream.ReadInt();
            DisableRelativeVolume = stream.ReadBool();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write((int)EffectID);
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(SoundData);
            Writer.Write(DisableRelativeVolume);
        }

        public enum SoundOrParticleEffect : int
        {
            SOUND_CLICK2 = 1000,
            SOUND_CLICK1 = 1001,
            SOUND_BOW_FIRE = 1002,
            SOUND_DOOR_TOGGLE = 1003,
            SOUND_EXTINGUISH = 1004,
            SOUND_RECORD_PLAY = 1005, // Has SoundData (probably record ID)
            SOUND_GHAST_CHARGE = 1007,
            SOUND_GHAST_FIREBALL = 1008,
            SOUND_ZOMBIE_WOOD = 1010,
            SOUND_ZOMBIE_METAL = 1011,
            SOUND_ZOMBIE_WOOD_BREAK = 1012,
            SOUND_WITHER_SPAWN = 1013,

            //Particles
            PARTICLE_SMOKE = 2000,       // Has SoundData (direction, see SmokeDirection)
            PARTICLE_BLOCK_BREAK = 2001,  // Has SoundData (Block ID broken)
            PARTICLE_SPLASH_POTION = 2002, // Has Data (Potion ID)
            PARTICLE_EYEOFENDER = 2003, // Unknown
            PARTICLE_MOB_SPAWN = 2004,
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

    public class NamedSoundEffectPacket : Packet
    {
        public string SoundName { get; set; }
        public int EffectX { get; set; }
        public int EffectY { get; set; }
        public int EffectZ { get; set; }
        public float Volume { get; set; }
        public byte Pitch { get; set; }

        public override void Read(PacketReader stream)
        {
            SoundName = stream.ReadString16(20);
            EffectX = stream.ReadInt();
            EffectY = stream.ReadInt();
            EffectZ = stream.ReadInt();
            Volume = stream.ReadFloat();
            Pitch = stream.ReadByte();
        }

        public override void Write()
        {
            SetCapacity(20, SoundName);

            Writer.Write(EffectX);
            Writer.Write(EffectY);
            Writer.Write(EffectZ);
            Writer.Write(Volume);
            Writer.Write(Pitch);
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

    public class GlobalEntityPacket : Packet
    {
        public int EntityId { get; set; }
        //The global entity, currently always 1 for thunderbolt.
        public byte ID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        protected override int Length { get { return 18; } }

        public override void Read(PacketReader stream)
        {
            EntityId = stream.ReadInt();
            ID = stream.ReadByte();
            X = stream.ReadDoublePacked();
            Y = stream.ReadDoublePacked();
            Z = stream.ReadDoublePacked();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(EntityId);
            Writer.Write(ID);
            Writer.Write((int)X);
            Writer.Write((int)Y);
            Writer.Write((int)Z);
        }
    }

    public class OpenWindowPacket : Packet
    {
        public sbyte WindowId { get; set; }
        internal InterfaceType InventoryType { get; set; }
        public string WindowTitle { get; set; }
        public sbyte SlotCount { get; set; }
        public bool UseProvidedWindowTitle { get; set; }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            InventoryType = (InterfaceType)stream.ReadSByte();
            WindowTitle = stream.ReadString16(100);
            SlotCount = stream.ReadSByte();
            UseProvidedWindowTitle = stream.ReadBool();
        }

        public override void Write()
        {
            SetCapacity(7, WindowTitle);
            Writer.Write(WindowId);
            Writer.Write((sbyte)InventoryType);
            Writer.Write(WindowTitle);
            Writer.Write(SlotCount);
            Writer.Write(UseProvidedWindowTitle);
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
        public MouseButtonClicked MouseButton { get; set; }
        public short Transaction { get; set; }
        public ClickWindowMode Mode { get; set; }
        public ItemInventory Item { get; set; }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            Slot = stream.ReadShort();
            MouseButton = (MouseButtonClicked)stream.ReadByte();
            Transaction = stream.ReadShort();
            Mode = (ClickWindowMode)stream.ReadByte();
            Item = ItemHelper.GetInstance(stream);
        }

        public override void Write()
        {
            if (Item == null || Item.Type == -1)
                SetCapacity(8);
            else
                SetCapacity(10);

            Writer.Write(WindowId);
            Writer.Write(Slot);
            Writer.Write((byte)MouseButton);
            Writer.Write(Transaction);
            if (MouseButton == MouseButtonClicked.Middle)
            {
                Writer.Write((byte)ClickWindowMode.MiddleClick);
            }
            else
            {
                Writer.Write((byte) Mode);
            }
            (Item ?? ItemHelper.Void).Write(Writer);
            Length = (int)Writer.UnderlyingStream.Length;
        }

        public enum MouseButtonClicked
        {
            Left = 0,
            Right = 1,
            Middle = 3
        }

        public enum ClickWindowMode : byte
        {
            Click = 0,
            ClickAndShift = 1,
            MiddleClick = 3,
            PaintingMode = 5,
            DoubleClick = 6
        }
    }

    public class SetSlotPacket : Packet
    {
        public sbyte WindowId { get; set; }
        public short Slot { get; set; }
        public ItemInventory Item { get; set; }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            Slot = stream.ReadShort();
            Item = ItemHelper.GetInstance(stream);
        }

        public override void Write()
        {
            SetCapacity(4);

            Writer.Write(WindowId);
            Writer.Write(Slot);
            //(Item ?? ItemHelper.Void).Write(Writer);
            Item.Write(Writer);
            Length = (int)Writer.UnderlyingStream.Length;
        }
    }

    public class WindowItemsPacket : Packet
    {
        public sbyte WindowId { get; set; }
        public ItemInventory[] Items { get; set; }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            Items = new ItemInventory[stream.ReadShort()];
            for (int i = 0; i < Items.Length; i++)
                Items[i] = ItemHelper.GetInstance(stream);
        }

        public override void Write()
        {
            SetCapacity(4);
            Writer.Write(WindowId);
            Writer.Write((short)Items.Length);
            for (int i = 0; i < Items.Length; i++)
                (Items[i] ?? ItemHelper.Void).Write(Writer);

            Length = (int)Writer.UnderlyingStream.Length;
        }
    }

    public class UpdateWindowPropertyPacket : Packet
    {
        /// <summary>
        /// The id of the window that the progress bar is in.
        /// </summary>
        public sbyte WindowId { get; set; }
        /// <summary>
        /// Which property should be updated. (For furnaces, 0 = progress arrow, 1 = fire icon)
        /// For enchantement table  0, 1 or 2 depending on the "enchantment slot" being given.
        /// </summary>
        public short Property { get; set; }
        /// <summary>
        /// <para>The value of the property </para>
        /// <para>
        /// The maximum values vary depending on the property. Presumably the values are specified as in-game ticks. Some progress bar values increase, while others decrease. For furnaces, 0 is empty, full progress arrow = about 180, full fire icon = about 250)
        /// </para>
        /// </summary>
        public short Value { get; set; }

        protected override int Length { get { return 6; } }

        public override void Read(PacketReader stream)
        {
            WindowId = stream.ReadSByte();
            Property = stream.ReadShort();
            Value = stream.ReadShort();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(WindowId);
            Writer.Write(Property);
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

    public class CreativeInventoryActionPacket : Packet
    {
        public short Slot { get; set; }
        public ItemInventory Item { get; set; }

        public override void Read(PacketReader stream)
        {
            Slot = stream.ReadShort();
            Item = ItemHelper.GetInstance(stream);
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

    public class UpdateScorePacket : Packet
    {
        public string ItemName { get; set; }
        public byte UpdateOrRemove { get; set; }
        public string ScoreName { get; set; }
        public int Value { get; set; }

        public override void Read(PacketReader reader)
        {
            ItemName = reader.ReadString16(240);
            UpdateOrRemove = reader.ReadByte();
            ScoreName = reader.ReadString16(240);
            Value = reader.ReadInt();
        }

        public override void Write()
        {
            SetCapacity(9, ItemName, ScoreName);
            Writer.Write(ItemName);
            Writer.Write(UpdateOrRemove);
            Writer.Write(ScoreName);
            Writer.Write(Value);
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

    public class ItemDataPacket : Packet
    {
        //Unknown fields
        public short ItemType { get; set; }
        public short ItemId { get; set; }
        //Text length of the Text Byte array
        public short TextLength { get; set; }
        public byte[] Text { get; set; }

        protected override int Length { get { return 7 + Text.Length; } }

        public override void Read(PacketReader stream)
        {
            ItemType = stream.ReadShort();
            ItemId = stream.ReadShort();
            TextLength = stream.ReadShort();
            Text = stream.ReadBytes(TextLength);
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(ItemType);
            Writer.Write(ItemId);
            Writer.Write(TextLength);
            for (int i = 0; i < TextLength; i++)
                Writer.Write(Text[i]);
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

    public class ParticlePacket : Packet
    {
        public string ParticleName { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float OffsetZ { get; set; }
        public float ParticleSpeed { get; set; }
        public int NumberOfParticles { get; set; }

        public override void Read(PacketReader stream)
        {
            ParticleName = stream.ReadString16(240);
            X = stream.ReadFloat();
            Y = stream.ReadFloat();
            Z = stream.ReadFloat();
            OffsetX = stream.ReadFloat();
            OffsetY = stream.ReadFloat();
            OffsetZ = stream.ReadFloat();
            ParticleSpeed = stream.ReadFloat();
            NumberOfParticles = stream.ReadInt();
        }

        public override void Write()
        {
            SetCapacity(34, ParticleName);
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(OffsetX);
            Writer.Write(OffsetY);
            Writer.Write(OffsetZ);
            Writer.Write(ParticleSpeed);
            Writer.Write(NumberOfParticles);
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

    public class PlayerAbilitiesPacket : Packet
    {
        public byte Abilities { get; set; }
        public bool Invulnerability { get; set; }
        public bool IsFlying { get; set; }
        public bool CanFly { get; set; }
        public byte WalkingSpeed { get; set; }
        public byte FlyingSpeed { get; set; }

        public override void Read(PacketReader reader)
        {
            Abilities = reader.ReadByte();
            Invulnerability = (Abilities & 1) == 1;
            IsFlying = (Abilities & 2) == 2;
            CanFly = (Abilities & 4) == 4;
        }

        public override void Write()
        {
            SetCapacity(4);
            Writer.Write((byte)(Invulnerability ? 1 : 0) | (IsFlying ? 2 : 0) | (CanFly ? 4 : 0));
            Writer.Write(WalkingSpeed);
            Writer.Write(FlyingSpeed);
        }
    }

    public class ScoreBoardObjectivePacket : Packet
    {
        public string ObjectiveName { get; set; }
        public string ObjectiveValue { get; set; }
        public byte CreateOrRemove { get; set; }

        public override void Read(PacketReader reader)
        {
            ObjectiveName = reader.ReadString16(240);
            ObjectiveValue = reader.ReadString16(240);
            CreateOrRemove = reader.ReadByte();
        }

        public override void Write()
        {
            SetCapacity(6, ObjectiveName, ObjectiveValue);
            Writer.Write(ObjectiveName);
            Writer.Write(ObjectiveValue);
            Writer.Write(CreateOrRemove);
        }
    }

    public class TabCompletePacket : Packet
    {
        public string Text { get; set; }

        public override void Read(PacketReader reader)
        {
            Text = reader.ReadString16(100);
        }

        public override void Write()
        {
            SetCapacity(3, Text);
            Writer.Write(Text);
        }
    }

    public class ClientSettingsPacket : Packet
    {
        public string Locale { get; set; }
        public byte ViewDistance { get; set; }
        public byte ChatFlags { get; set; }
        public byte Difficulty { get; set; }
        public bool ShowCape { get; set; }

        public override void Read(PacketReader reader)
        {
            Locale = reader.ReadString16(10);
            ViewDistance = reader.ReadByte();
            ChatFlags = reader.ReadByte();
            Difficulty = reader.ReadByte();
            ShowCape = reader.ReadBool();
        }

        public override void Write()
        {
            SetCapacity(7, Locale);
            Writer.Write(Locale);
            Writer.Write(ViewDistance);
            Writer.Write(ChatFlags);
            Writer.Write(Difficulty);
            Writer.Write(ShowCape);
        }
    }

    public class ClientStatusPacket : Packet
    {
        public byte Status { get; set; }

        protected override int Length { get { return 2; } }

        public override void Read(PacketReader reader)
        {
            Status = reader.ReadByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(Status);
        }
    }

    public class PluginMessagePacket : Packet
    {
        public string Channel { get; set; }
        public short ByteLength { get; internal set; }
        public byte[] Message { get; set; }

        public override void Read(PacketReader reader)
        {
            Channel = reader.ReadString16(260);
            ByteLength = reader.ReadShort();
            Message = reader.ReadBytes(ByteLength);
        }

        public override void Write()
        {
            SetCapacity(5 + Message.Length, Channel);
            Writer.Write(Channel);
            Writer.Write((short)Message.Length);
            Writer.Write(Message, 0, Message.Length);
        }
    }

    public class EncryptionKeyResponse : Packet
    {
        public short SharedSecretLength { get; set; }
        public byte[] SharedSecret { get; set; }
        public short VerifyTokenLength { get; set; }
        public byte[] VerifyToken { get; set; }

        protected override int Length { get { return 5 + SharedSecretLength + VerifyTokenLength; } }

        public override void Read(PacketReader reader)
        {
            SharedSecretLength = reader.ReadShort();
            SharedSecret = reader.ReadBytes(SharedSecretLength);
            VerifyTokenLength = reader.ReadShort();
            VerifyToken = reader.ReadBytes(VerifyTokenLength);

        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(SharedSecretLength);
            if (SharedSecretLength > 0)
                Writer.Write(SharedSecret, 0, SharedSecretLength);

            Writer.Write(VerifyTokenLength);
            if (VerifyTokenLength > 0)
                Writer.Write(VerifyToken, 0, VerifyTokenLength);
        }
    }

    public class EncryptionKeyRequest : Packet
    {
        public string ServerId { get; set; }
        public short PublicKeyLength { get; set; }
        public byte[] PublicKey { get; set; }
        public short VerifyTokenLength { get; set; }
        public byte[] VerifyToken { get; set; }

        public override void Read(PacketReader reader)
        {
            ServerId = reader.ReadString16(20);
            PublicKeyLength = reader.ReadShort();
            PublicKey = reader.ReadBytes(PublicKeyLength);
            VerifyTokenLength = reader.ReadShort();
            VerifyToken = reader.ReadBytes(VerifyTokenLength);

        }

        public override void Write()
        {
            SetCapacity(7 + PublicKeyLength + VerifyTokenLength, ServerId);
            Writer.Write(ServerId);
            Writer.Write(PublicKeyLength);
            Writer.Write(PublicKey, 0, PublicKeyLength);
            Writer.Write(VerifyTokenLength);
            Writer.Write(VerifyToken, 0, VerifyTokenLength);
        }
    }

    /// <summary>
    /// To load server info in the multiplayer menu, the notchian client connects to each known server and sends an 0xFE.
    /// In return, the server sends a kick (0xFF), with its string containing data (server description, number of users, number of slots), delimited by a §
    /// </summary>
    public class ServerListPingPacket : Packet
    {
        protected override int Length { get { return 2; } }

        public byte Magic { get; set; }

        public override void Read(PacketReader stream)
        {
            Magic = stream.ReadByte();
        }

        public override void Write()
        {
            SetCapacity();
            Writer.Write(Magic);
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

    public class DisplayScoreboardPacket : Packet
    {
        public BoardPosition Position { get; set; }
        public string ScoreName { get; set; }

        public override void Read(PacketReader reader)
        {
            Position = (BoardPosition)reader.ReadByte();
            ScoreName = reader.ReadString16(240);
        }

        public override void Write()
        {
            SetCapacity(4, ScoreName);
            Writer.Write((byte)Position);
            Writer.Write(ScoreName);
        }

        public enum BoardPosition : byte
        {
            List = 0,
            SideBar = 1,
            BelowName = 2
        }
    }

    public class TeamsPacket : Packet
    {
        public string TeamName { get; set; }
        public TeamMode Mode { get; set; }
        public string TeamDisplayName { get; set; }
        public string TeamPrefix { get; set; }
        public string TeamSuffix { get; set; }
        public byte FriendlyFire { get; set; }
        public short PlayerCount { get; set; }
        public string[] Players { get; set; }

        public override void Read(PacketReader reader)
        {
            TeamName = reader.ReadString16(240);
            Mode = (TeamMode) reader.ReadByte();

            if (Mode == TeamMode.Created || Mode == TeamMode.InfoUpdated)
            {
                TeamDisplayName = reader.ReadString16(240);
                TeamPrefix = reader.ReadString16(240);
                TeamSuffix = reader.ReadString16(240);
                FriendlyFire = reader.ReadByte();
            }
            if (Mode == TeamMode.Created || Mode == TeamMode.PlayerAdded || Mode == TeamMode.PlayerRemoved)
            {
                var count = reader.ReadInt();
                Players = new string[count];
                for (int i = 0; i < count; i++)
                {
                    Players[i] = reader.ReadString16(140);
                }
            }
        }

        public override void Write()
        {
            Writer.Write(TeamName);
            Writer.Write((byte) Mode);

            if (Mode == TeamMode.Created || Mode == TeamMode.InfoUpdated)
            {
                Writer.Write(TeamDisplayName);
                Writer.Write(TeamPrefix);
                Writer.Write(TeamSuffix);
                Writer.Write(FriendlyFire);
            }
            if (Mode == TeamMode.Created || Mode == TeamMode.PlayerAdded || Mode == TeamMode.PlayerRemoved)
            {
                Writer.Write((short) Players.Length);
                for (int i = 0; i < Players.Length; i++)
                {
                    Writer.Write(Players[i]);
                }
            }
        }

        public enum TeamMode : byte
        {
            Created = 0,
            Removed = 1,
            InfoUpdated = 2,
            PlayerAdded = 3,
            PlayerRemoved =4
        }
    }
}
