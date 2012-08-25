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
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Item;
using Chraft.World;
using System.Runtime.Serialization;

namespace Chraft.Interfaces
{
    [Serializable]
    public class ItemStack : IItemStack
    {
        public event EventHandler Changed;

        public short Slot { get; internal set; }
        public static ItemStack Void { get { return new ItemStack(-1, 0, 0); } } // Needs to send -1 via 0x05

        public bool IsEnchantable()
        {
            return (Type >= 256 && Type <= 258) || Type == 261 ||
                   (Type >= 267 && Type <= 279) ||
                   (Type >= 283 && Type <= 286) ||
                   (Type >= 290 && Type <= 294) ||
                   (Type >= 298 && Type <= 317);
        }

        private short _Type;
        public short Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
                OnChanged();
            }
        }

        private sbyte _Count;
        public sbyte Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
                OnChanged();
            }
        }

        private short _Durability;
        public short Durability
        {
            get
            {
                return _Durability;
            }
            set
            {
                _Durability = value;
                OnChanged();
            }
        }

        public ItemStack()
        {
        }

        internal ItemStack(PacketReader stream)
        {
            Type = stream.ReadShort();
            if (Type >= 0)
            {
                Count = stream.ReadSByte();
                Durability = stream.ReadShort();

                // TODO: Implement extra data read (enchantment) and items
                if (Durability > 0 || IsEnchantable())
                    stream.ReadShort();
            }
        }

        internal ItemStack(BigEndianStream stream)
        {
            Type = stream.ReadShort();
            if (Type >= 0)
            {
                Count = stream.ReadSByte();
                Durability = stream.ReadShort();

                // TODO: Implement extra data read (enchantment) and items
                if (Durability > 0 || IsEnchantable())
                    stream.ReadShort();
            }
        }

        public bool IsVoid()
        {
            return Count < 1 || Type <= 0;
        }

        private void OnChanged()
        {
            if (Changed != null)
                Changed.Invoke(this, new EventArgs());
        }

        public ItemStack(short type)
        {
            Type = type;
            Count = 1;
            Durability = 0;
        }

        public ItemStack(short type, sbyte count) : this(type, count, 0) { }

        public ItemStack(short type, sbyte count, short durability)
        {
            Type = type;
            Count = count;
            Durability = durability;
        }

        internal void Write(PacketWriter stream)
        {
            stream.Write(Type > 0 ? Type : (short)-1);
            if (Type > 0)
            {
                stream.Write(Count);
                stream.Write(Durability);

                //if (Durability > 0 || IsEnchantable())
                stream.Write((short)-1);
                // TODO: Remove the two lines above and implement items and enchantments write
                /* 
                 * if (Item.CanBeDamaged())
                 * {
                 *      if(_enchantments != null)
                 *          WriteEnchantmentsToNBT(stream);
                 *      else
                 *          stream.Write(-1);
                 * }
                 */
            }
        }

        internal void Write(BigEndianStream stream)
        {
            stream.Write(Type > 0 ? Type : (short)-1);
            if (Type > 0)
            {
                stream.Write(Count);
                stream.Write(Durability);

                //if (Durability > 0 || IsEnchantable())
                stream.Write((short)-1);
                // TODO: Remove the two lines above and implement items and enchantments write
                /* 
                 * if (Item.CanBeDamaged())
                 * {
                 *      if(_enchantments != null)
                 *          WriteEnchantmentsToNBT(stream);
                 *      else
                 *          stream.Write(-1);
                 * }
                 */
            }
        }

        internal void ReadEnchantmentsFromNBT(PacketWriter stream)
        {
            // TODO: Implement this and choose return value
        }

        internal void ReadEnchantmentsFromNBT(BigEndianStream stream)
        {
            // TODO: Implement this and choose return value
        }

        internal void WriteEnchantmentsToNBT(PacketWriter stream)
        {
            // TODO: Implement this
            /*      Gzip the nbt representing enchantments
             *      write the gzipped output length
             *      write output 
             */
        }

        internal void WriteEnchantmentsToNBT(BigEndianStream stream)
        {
            // TODO: Implement this
            /*      Gzip the nbt representing enchantments
             *      write the gzipped output length
             *      write output 
             */
        }

        public byte ToBlock()
        {
            switch (Type)
            {
                default:
                    return (byte)Type;
            }
        }

        /// <summary>
        /// Determines whether the durability and type of two stacks are identical.
        /// </summary>
        /// <param name="stack">The stack to be compared with the current object.</param>
        /// <returns>True if the objects stack; otherwise false.</returns>
        /// <remarks>
        /// <para>This method does not take into consideration whether or not the item type can be stacked (e.g. pork).</para>
        /// <para>This method is used under this assumption throughout the server.</para>
        /// </remarks>
        public bool StacksWith(ItemStack stack)
        {
            return Type == stack.Type && stack.Durability == Durability;
        }

        public static ItemStack Parse(string code)
        {
            string[] parts = code.Split(':', '#');
            string numeric = parts[0];
            string count = "1";
            string durability = "0";
            if (code.Contains(':'))
                durability = parts[1];
            if (code.Contains('#'))
                count = parts[parts.Length - 1];
            return new ItemStack(short.Parse(numeric), sbyte.Parse(count), durability == "*" ? (short)-1 : short.Parse(durability));
        }
        internal static ItemStack Read(PacketReader stream)
        {
            ItemStack retval = new ItemStack(stream.ReadShort());
            if (retval.Type >= 0)
            {
                retval.Count = stream.ReadSByte();
                retval.Durability = stream.ReadShort();

                // TODO: Implement extra data read (enchantment) and items

                //if (retval.Durability > 0 || retval.IsEnchantable())
                    stream.ReadShort();
            }
            return retval;

        }
    }
}