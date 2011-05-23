using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using org.bukkit.inventory;

namespace Chraft.Interfaces
{
	[Serializable]
	public class ItemStackChraft : ItemStackBukkit
	{        
		public event EventHandler Changed;

		internal short Slot 
        {
            get;
            set; 
        }

		public static ItemStackChraft Void { get { return new ItemStackChraft(-1, 0, 0); } } // Needs to send -1 via 0x05

		public short Type
		{
			get
			{
			    return (short) base.Type;
			}
			set
			{
				base.Type =value;
				OnChanged();
			}
		}

		public sbyte Count
		{
			get
			{
				return (sbyte)base.Amount;
			}
			set
			{
				base.Amount = value;
				OnChanged();
			}
		}

		public short Durability
		{
			get
			{
				return (short)base.Durability;
			}
			set
			{
				base.Durability = value;
				OnChanged();
			}
		}

        public ItemStackChraft()
            : base(-1, 0, 0)
        {
        }

		public ItemStackChraft(short type)
			: base(type, 1, 0)
		{
		}

		public ItemStackChraft(short type, sbyte count, short durability)
			: base(type, count, durability)
		{
		}

		public ItemStackChraft(org.bukkit.inventory.ItemStack @is, short slot)
			: base(@is.getTypeId(), @is.getAmount(), @is.getDurability())
		{
			Slot = slot;
		}

		internal static ItemStackChraft Read(BigEndianStream stream)
		{
			ItemStackChraft retval = new ItemStackChraft(stream.ReadShort());
			if (retval.Type >= 0)
			{
				retval.Count = stream.ReadSByte();
				retval.Durability = stream.ReadShort();
			}
			return retval;
		}

		public static bool IsVoid(ItemStackChraft stack)
		{
			return stack == null || stack.Count < 1 || stack.Type <= 0;
		}

		private void OnChanged()
		{
			if (Changed != null)
				Changed.Invoke(this, new EventArgs());
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
		public bool StacksWith(ItemStackChraft stack)
		{
			return Type == stack.Type && stack.Durability == Durability;
		}

		public static ItemStackChraft Parse(string code)
		{
			string[] parts = code.Split(':', '#');
			string numeric = parts[0];
			string count = "1";
			string durability = "0";
			if (code.Contains(':'))
				durability = parts[1];
			if (code.Contains('#'))
				count = parts[parts.Length - 1];
			return new ItemStackChraft(short.Parse(numeric), sbyte.Parse(count), durability == "*" ? (short)-1 : short.Parse(durability));
		}

		public bool Equals(ItemStack other)
		{
			return equals(other);
		}

	    public bool @equals(ItemStack other)
	    {
	        throw new NotImplementedException();
	    }
	}
}
