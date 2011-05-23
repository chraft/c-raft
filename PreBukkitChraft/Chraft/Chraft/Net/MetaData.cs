using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;

namespace Chraft.Net
{
    public class MetaData
    {
        private Dictionary<int, object> Data = new Dictionary<int, object>();

        public bool Sheared
        {
            get { return ((byte)Data[16] & 0x10) != 0; }
            set { Data[16] = Data.ContainsKey(16) ? ((byte)Data[16] & 0xef) | (value ? 0x10 : 0) : (value ? 0x10 : 0); }
        }

        public WoolColor WoolColor
        {
            get { return (WoolColor)((byte)Data[16] & 0xf); }
            set { Data[16] = Data.ContainsKey(16) ? ((byte)Data[16] & 0xf) | (byte)value : (byte)value; }
        }

        public bool IsOnFire
        {
            get { return ((byte)Data[0] & 0x1) != 0; }
            set { Data[0] = (byte)((byte)Data[0] & 0xfe | (value ? 0x1 : 0)); }
        }
        public bool IsCrouched
        {
            get { return ((byte)Data[0] & 0x2) != 0; }
            set { Data[0] = (byte)((byte)Data[0] & 0xfd | (value ? 0x2 : 0)); }
        }
        public bool IsRiding
        {
            get { return ((byte)Data[0] & 0x4) != 0; }
            set { Data[0] = (byte)((byte)Data[0] & 0xfb | (value ? 0x4 : 0)); }
        }

        public MetaData()
        {
            if (!Data.ContainsKey(0))
                Data.Add(0, (byte)0);
        }

        internal MetaData(BigEndianStream rx)
        {
            byte x;
            while ((x = rx.ReadByte()) != 0x7f)
            {
                switch (x >> 5)
                {
                    case 0: Data[x & 0x1f] = rx.ReadByte(); break;
                    case 1: Data[x & 0x1f] = rx.ReadShort(); break;
                    case 2: Data[x & 0x1f] = rx.ReadInt(); break;
                    case 3: Data[x & 0x1f] = rx.ReadFloat(); break;
                    case 4: Data[x & 0x1f] = rx.ReadString16(64); break;
                    default: Data[x & 0x1f] = null; break;
                }
            }
        }

        internal void Write(BigEndianStream tx)
        {
            try // I can't work out how it set this from SpawnAnimal.
            {
                foreach (int k in Data.Values)
                {
                    Type type = Data[k].GetType();
                    if (type == typeof(byte))
                    {
                        tx.WriteByte((byte)k);
                        tx.Write((byte)Data[k]);
                    }
                    else if (type == typeof(short))
                    {
                        tx.WriteByte((byte)(0x20 | k));
                        tx.Write((short)Data[k]);
                    }
                    else if (type == typeof(int))
                    {
                        tx.WriteByte((byte)(0x40 | k));
                        tx.Write((int)Data[k]);
                    }
                    else if (type == typeof(float))
                    {
                        tx.WriteByte((byte)(0x60 | k));
                        tx.Write((float)Data[k]);
                    }
                    else if (type == typeof(string))
                    {
                        tx.WriteByte((byte)(0x80 | k));
                        tx.Write((string)Data[k]);
                    }
                }
            }
            catch { }
            finally
            {
                tx.WriteByte(0x7f);
            }
        }


        public enum Wood : byte
        {
            Normal, Redwood, Birch
        }
        public enum Slab : byte
        {
            Stone, Sandstone, Wooden, Cobblestone
        }
        public enum Liquid : byte
        {
            Full = 0x0, LavaMax = 0x3, WaterMax = 0x7, Falling = 0x8
        }
        public enum Wool : byte
        {
            White, Orange, Magenta, LightBlue, Yellow, LightGreen, Pink, Gray,
            LightGray, Cyan, Purple, Blue, Brown, DarkGreen, Red, Black
        }
        public enum Dyes : byte
        {
            InkSack, RoseRed, CactusGreen, CocoBeans, LapisLazuli, Purple, Cyan, LightGray,
            Gray, Pink, Lime, DandelionYellow, LightBlue, Magenta, Orange, BoneMeal
        }
        public enum Torch : byte
        {
            South = 0x1, North, East, West, Standing
        }
        public enum Bed : byte
        {
            West, North, East, South,
            BedFoot
        }
        public enum Tracks : byte
        {
            EastWest, NorthSouth, RiseSouth, RiseNorth, RiseEast, RiseWest,
            NECorner, SECorner, SWCorner, NWCorner
        }
        public enum Ladders : byte
        {
            East = 2, West, North, South
        }
        public enum Stairs : byte
        {
            South, North, West, East
        }
        public enum Lever : byte
        {
            SouthWall = 1, NorthWall, WestWall, EastWall, EWGround, NSGround,
            IsFlipped = 8
        }
        public enum Door : byte
        {
            Northeast, Southeast, Southwest, Northwest,
            IsOpen = 4, IsTopHalf = 8
        }
        public enum Button : byte
        {
            SouthWall = 0x1, NorthWall, WestWall, EastWall,
            IsPressed = 0x8
        }
        public enum SignPost : byte
        {
            West = 0x0,
            WestNorthwest,
            Northwest,
            NorthNorthwest,
            North,
            NorthNortheast,
            Northeast,
            EastNortheast,
            East,
            EastSoutheast,
            Southeast,
            SouthSoutheast,
            South,
            SouthSouthwest,
            Southwest,
            WestSouthwest
        }
        public enum SignWall : byte
        {
            East = 0x2, West, North, South
        }
        public enum Dispenser : byte
        {
            East = 0x2, West, North, South
        }
        public enum Furnace : byte
        {
            East = 0x2, West, North, South
        }
        public enum Pumpkin : byte
        {
            East, South, West, North
        }
        public enum RedstoneRepeater : byte
        {
            East, South, West, North,
            Tick1, Tick2, Tick3, Tick4
        }
    }
}
