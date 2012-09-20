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
using Chraft.PluginSystem.Item;

namespace Chraft.Entity.Items.Base
{
    public abstract class ItemBase
    {
        public IInterface Owner { get; internal set; }
        public event EventHandler Changed;

        public short Slot { get; internal set; }
        public short Type { get; protected set; }
        public string Name { get; protected set; }

        protected short _durability;
        public short Durability
        {
            get { return _durability; }
            set
            {
                _durability = value;
                OnChanged();
            }
        }

        protected short _damage;
        public short Damage
        {
            get { return _damage; }
            set
            {
                _damage = value;
                OnChanged();
            }
        }

        protected sbyte _count;
        public sbyte Count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnChanged();
            }
        }

        public bool IsStackable { get; protected set; }
        public short MaxStackSize { get; protected set; }
        public bool IsEnchantable { get; protected set; }

        protected ItemBase()
        {
            Type = 0;
            Name = "ItemBase";
            Durability = 0;
            Damage = 0;
            Count = 1;
            IsStackable = true;
            MaxStackSize = 64;
        }

        public ItemBase(short type) : this()
        {
            Type = type;
        }

        public ItemBase(short type, sbyte count) : this()
        {
            Type = type;
            Count = count;
        }

        public ItemBase(short type, sbyte count, short durability) : this()
        {
            Type = type;
            Count = count;
            Durability = durability;
        }

        internal virtual void Write(BigEndianStream stream)
        {
            stream.Write(Type > 0 ? Type : (short)-1);
            if (Type > 0)
            {
                stream.Write(Count);
                stream.Write(Durability);
            }
        }

        internal virtual void Write(PacketWriter stream)
        {
            stream.Write(Type > 0 ? Type : (short)-1);
            if (Type > 0)
            {
                stream.Write(Count);
                stream.Write(Durability);

                //if (Durability > 0 || IsEnchantable)
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

        /// <summary>
        /// Determines whether the durability and type of two stacks are identical.
        /// </summary>
        /// <param name="stack">The stack to be compared with the current object.</param>
        /// <returns>True if the objects stack; otherwise false.</returns>
        /// <remarks>
        /// <para>This method does not take into consideration whether or not the item type can be stacked (e.g. pork).</para>
        /// <para>This method is used under this assumption throughout the server.</para>
        /// </remarks>
        public virtual bool StacksWith(ItemBase stack)
        {
            return Type == stack.Type && stack.Durability == Durability;
        }

        // For recipes
        public ItemBase(string code)
        {
            string[] parts = code.Split(':', '#');
            string numeric = parts[0];
            string count = "1";
            string durability = "0";
            if (code.Contains(':'))
                durability = parts[1];
            if (code.Contains('#'))
                count = parts[parts.Length - 1];
            Type = short.Parse(numeric);
            Count = sbyte.Parse(count);
            Durability = (durability == "*" ? (short) -1 : short.Parse(durability));
        }

        public ItemBase(PacketReader stream)
        {
            Type = stream.ReadShort();
            if (Type >= 0)
            {
                Count = stream.ReadSByte();
                Durability = stream.ReadShort();

                // TODO: Implement extra data read (enchantment) and items

                //if (Durability > 0 || IsEnchantable)
                    stream.ReadShort();
            }
        }

        public virtual short GetDamage()
        {
            return Damage;
        }

        private void OnChanged()
        {
            if (Changed != null)
                Changed.Invoke(this, new EventArgs());
        }

        public ItemBase Clone()
        {
            var newItem = ItemHelper.GetInstance(Type);
            newItem.Count = Count;
            newItem.Durability = Durability;
            newItem.Damage = Damage;
            return newItem;
        }
    }
}
