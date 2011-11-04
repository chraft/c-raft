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
using System.Runtime.Serialization;
using Chraft.Net;
using Chraft.Interfaces.Recipes;
using Chraft.Net.Packets;
using Chraft.Entity;

namespace Chraft.Interfaces
{
	[Serializable]
	public partial class Inventory : CraftingInterface
	{

		private short _ActiveSlot;
		public short ActiveSlot { get { return _ActiveSlot; } }

        
        public Chraft.Interfaces.ItemStack ActiveItem { 
            get { return this[ActiveSlot]; }
        }

		/// <summary>
		/// Used for serialization only.  Do not use.
		/// </summary>
		public Inventory()
            : base(InterfaceType.Inventory, 4, 45)
		{
			_ActiveSlot = 36;
			_IsOpen = true;
            // Inventory is always WindowId 0
            Handle = 0;
		}

		internal Inventory(Player player)
			: base(InterfaceType.Inventory, 4, 45)
		{
			_ActiveSlot = 36;
			Associate(player);
			_IsOpen = true;
            // Inventory is always WindowId 0
            Handle = 0;
			UpdateClient();
		}

		public override void Associate(Player player)
		{
			base.Associate(player);
		}

		internal void OnActiveChanged(short slot)
		{
			_ActiveSlot = slot;
		}

		/// <summary>
		/// Gets an array of quick slots.
		/// </summary>
		/// <returns>Quick slots from left to right</returns>
		public IEnumerable<ItemStack> GetQuickSlots()
		{
			for (short i = (short)InventorySlots.QuickSlotFirst; i <= (short)InventorySlots.QuickSlotLast; i++)
				yield return this[i];
		}

		internal void AddItem(short id, sbyte count, short durability)
		{
			// Quickslots, stacking
            for (short i = (short)InventorySlots.QuickSlotFirst; i <= (short)InventorySlots.QuickSlotLast; i++)
			{
				if (!ItemStack.IsVoid(Slots[i]) && Slots[i].Type == id && Slots[i].Durability == durability)
				{
					if (Slots[i].Count + count <= 64)
					{
						Slots[i].Count += count;
						return;
					}
					count -= (sbyte)(64 - Slots[i].Count);
					Slots[i].Count = 64;
				}
			}

			// Inventory, stacking
			for (short i = (short)InventorySlots.InventoryFirst; i <= (short)InventorySlots.InventoryLast; i++)
			{
				if (!ItemStack.IsVoid(Slots[i]) && Slots[i].Type == id && Slots[i].Durability == durability)
				{
					if (Slots[i].Count + count <= 64)
					{
						Slots[i].Count += count;
						return;
					}
					count -= (sbyte)(64 - Slots[i].Count);
					Slots[i].Count = 64;
				}
			}

			// Quickslots, not stacking
            for (short i = (short)InventorySlots.QuickSlotFirst; i <= (short)InventorySlots.QuickSlotLast; i++)
			{
				if (ItemStack.IsVoid(Slots[i]))
				{
					Owner.Client.SendPacket(new ChatMessagePacket { Message = "Placing in slot " + i });
					this[i] = new ItemStack(id, count, durability) { Slot = i };
					return;
				}
			}

			// Inventory, not stacking
            for (short i = (short)InventorySlots.InventoryFirst; i <= (short)InventorySlots.InventoryLast; i++)
			{
				if (ItemStack.IsVoid(Slots[i]))
				{
					this[i] = new ItemStack(id, count, durability) { Slot = i };
					return;
				}
			}
		}

        internal void RemoveItem(short slot)
        {
            if (this[slot].Type > 0)
            {
                if (this[slot].Count == 1)
                {
                    this[slot] = ItemStack.Void;
                }
                else
                {
                    this[slot].Count -= 1;
                }
            }
        }

        internal bool DamageItem(short slot, short damageAmount = 1)
        {
            short durability = 0;

            World.BlockData.ToolDuarability.TryGetValue((World.BlockData.Items)this[slot].Type, out durability);

            if (durability > 0)
            {
                if (this[slot].Durability >= durability)
                {
                    if (this[slot].Count == 1)
                    {
                        this[slot] = ItemStack.Void;
                        return true;
                    }
                    else // This will allow stacked tools to work properly.
                    {
                        this[slot].Durability = 0;
                        this[slot].Count--;
                        return true;
                    }
                }
                else
                {
                    this[slot].Durability += damageAmount;
                    return true;
                }
            }

            return false;
        }

        protected override void DoClose()
        {
            base.DoClose();

            // Always leave the inventory open
            _IsOpen = true;
        }

        public enum InventorySlots : short
        {
            CraftingOutput = 0,
            CraftingInputFirst = 1,
            CraftingInputLast = 4,
            Head = 5,
            Chest = 6,
            Legs = 7,
            Feets = 8,
            InventoryFirst = 9,
            InventoryLast = 35,
            QuickSlotFirst = 36,
            QuickSlotLast = 44
        }
	}
}
