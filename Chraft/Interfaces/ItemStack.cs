using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;
using System.Runtime.Serialization;

namespace Chraft.Interfaces
{
    [Serializable]
    public class ItemStack
    {
        public event EventHandler Changed;

        internal short Slot { get; set; }
        public static ItemStack Void { get { return new ItemStack(-1, 0, 0); } } // Needs to send -1 via 0x05

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

        internal ItemStack(BigEndianStream stream)
        {
            Type = stream.ReadShort();
            if (Type >= 0)
            {
                Count = stream.ReadSByte();
                Durability = stream.ReadShort();
            }
        }

        public static bool IsVoid(ItemStack stack)
        {
            return stack == null || stack.Count < 1 || stack.Type <= 0;
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

        public ItemStack(short type, sbyte count) : this(type, count, 0) {}

        public ItemStack(short type, sbyte count, short durability)
        {
            Type = type;
            Count = count;
            Durability = durability;
        }

        internal void Write(BigEndianStream stream)
        {
            stream.Write(Type > 0 ? Type : (short)-1);
            if (Type > 0)
            {
                stream.Write(Count);
                stream.Write(Durability);
            }
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
        internal static ItemStack Read(BigEndianStream stream)
        {
            ItemStack retval = new ItemStack(stream.ReadShort());
            if (retval.Type >= 0)
            {
                retval.Count = stream.ReadSByte();
                retval.Durability = stream.ReadShort();
            }
            return retval;

        }
    }
}
