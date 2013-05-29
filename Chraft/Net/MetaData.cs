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
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Entity;
using Chraft.Utilities;
using Chraft.Utilities.Misc;
using System.Collections.Concurrent;

namespace Chraft.Net
{
    public class MetaData : IMetaData
    {
        private ConcurrentDictionary<int, object> Data = new ConcurrentDictionary<int, object>();

        public bool Sheared
        {
            get { return ((byte)Data[16] & 0x10) != 0; }
            set { Data[16] = (byte)(Data.ContainsKey(16) ? ((byte)Data[16] & 0xef) | (value ? 0x10 : 0) : (value ? 0x10 : 0)); }
            //TODO - find out the significance of 0xef
        }

        public WoolColor WoolColor
        {
            get { return (WoolColor)((byte)Data[16] & 0xf); }
            set { Data[16] = (byte)(Data.ContainsKey(16) ? ((byte)Data[16] & 0xf) | (byte)value : (byte)value); }
            //TODO - find out the significance of 0xf
        }

        public bool IsOnFire
        {
            get { return ((byte)Data[0] & 0x1) != 0; }
            set { Data[0] = (byte)((byte)Data[0] & 0xfe | (value ? 0x1 : 0)); }
            //TODO - find out the significance of 0xfe
        }
        public bool IsCrouched
        {
            get { return ((byte)Data[0] & 0x2) != 0; }
            set { Data[0] = (byte)((byte)Data[0] & 0xfd | (value ? 0x2 : 0)); }
            //TODO - find out the significance of 0xfd
        }
        public bool IsRiding
        {
            get { return ((byte)Data[0] & 0x4) != 0; }
            set { Data[0] = (byte)((byte)Data[0] & 0xfb | (value ? 0x4 : 0)); }
            //TODO - find out the significance of 0xfb
        }

        public bool IsSprinting
        {
            get { return ((byte)Data[0] & 0x8) != 0; }
            set { Data[0] = (byte)((byte)Data[0] & 0xf7 | (value ? 0x8 : 0)); }
            //TODO - find out the significance of 0xf7
        }

        #region Wolf Meta Data
        /// <summary>
        /// Gets / Sets wolfs sitting state
        /// </summary>
        public bool IsSitting
        {
            get { return (Data.ContainsKey(16) ? ((byte)Data[16] & 0x1) != 0 : false); }
            set { Data[16] = (byte)(Data.ContainsKey(16) ? ((byte)Data[16] & 0xfb | (value ? 0x1 : 0)) : (value ? 0x1 : 0)); }
            //TODO - find out the significance of 0xfb
        }

        /// <summary>
        /// Gets / Sets wolfs aggressive state
        /// </summary>
        public bool IsAggressive
        {
            get { return (Data.ContainsKey(16) ? ((byte)Data[16] & 0x2) != 0 : false); }
            set { Data[16] = (byte)(Data.ContainsKey(16) ? ((byte)Data[16] & 0xfb | (value ? 0x2 : 0)) : (value ? 0x1 : 0)); }
            //TODO - find out the significance of 0xfb
        }

        /// <summary>
        /// Gets / Sets wolfs tamed state
        /// </summary>
        public bool IsTamed
        {
            get { return (Data.ContainsKey(16) ? ((byte)Data[16] & 0x4) != 0 : false); }
            set { Data[16] = (byte)(Data.ContainsKey(16) ? ((byte)Data[16] & 0xfb | (value ? 0x4 : 0)) : (value ? 0x4 : 0)); }
            //TODO - find out the significance of 0xfb
        }

        public string TamedBy
        {
            get { return (Data.ContainsKey(17) ? (string)Data[17] : String.Empty); }
            set { Data[17] = value; }
        }

        public int Health
        {
            get { return (Data.ContainsKey(18) ? (int)Data[18] : 8); }
            set { Data[18] = value; }
        }
#endregion


        public MetaData()
        {
            //if (!Data.ContainsKey(0))
            Data.TryAdd(0, (byte)0);
        }

        internal MetaData(PacketReader rx)
        {
            byte x;
            while ((x = rx.ReadByte()) != 0x7f)
            {
                if (rx.Failed)
                    return;

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

        internal void Write(PacketWriter tx)
        {
            try // I can't work out how it set this from SpawnAnimal.
            {
                foreach (int k in Data.Keys)
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
            South = 0x1, North, West, East, Standing
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
        public enum Container : byte
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

        public enum Cake : byte
        {
            Full = 0x0,
            FiveLeft,
            FourLeft,
            ThreeLeft,
            TwoLeft,
            OneLeft
        }

        public enum HugeMushroom : byte
        {
            Porous = 0x0,
            TopNorthWest,
            TopNorth,
            TopNorthEast,
            TopWest,
            Top,
            TopEast,
            TopSouthWest,
            TopSouth,
            TopSouthEast,
            NorthWeastSouthEast
        }
    }
}
