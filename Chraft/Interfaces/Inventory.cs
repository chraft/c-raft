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
using Chraft.Entity.Items;
using Chraft.Net.Packets;
using Chraft.Entity;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Item;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;

namespace Chraft.Interfaces
{
    [Serializable]
	public class Inventory : CraftingInterface, IInventory
    {

		private short _ActiveSlot;
		public short ActiveSlot { get { return _ActiveSlot; } }

        
        public IItemInventory ActiveItem { 
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

        public IItemInventory GetActiveItem()
        {
            return ActiveItem;
        }

		internal override void Associate(Player player)
		{
			base.Associate(player);
		}

		internal void OnActiveChanged(short slot)
		{
            if (slot >= (short)InventorySlots.QuickSlotFirst && slot <= (short)InventorySlots.QuickSlotLast)
			    _ActiveSlot = slot;
		}

		/// <summary>
		/// Gets an array of quick slots.
		/// </summary>
		/// <returns>Quick slots from left to right</returns>
        public IEnumerable<IItemInventory> GetQuickSlots()
		{
			for (short i = (short)InventorySlots.QuickSlotFirst; i <= (short)InventorySlots.QuickSlotLast; i++)
				yield return this[i];
		}

		public void AddItem(short id, sbyte count, short durability, bool isInGame =true)
		{
			// Quickslots, stacking
            for (short i = (short)InventorySlots.QuickSlotFirst; i <= (short)InventorySlots.QuickSlotLast; i++)
			{
                if (!ItemHelper.IsVoid(this[i]) && this[i].Type == id && this[i].Durability == durability)
				{
					if (this[i].Count + count <= 64)
					{
                        this[i].Count += count;
						return;
					}
                    count -= (sbyte)(64 - this[i].Count);
                    this[i].Count = 64;
				}
			}

			// Inventory, stacking
			for (short i = (short)InventorySlots.InventoryFirst; i <= (short)InventorySlots.InventoryLast; i++)
			{
                if (!ItemHelper.IsVoid(this[i]) && this[i].Type == id && this[i].Durability == durability)
				{
                    if (this[i].Count + count <= 64)
					{
                        this[i].Count += count;
						return;
					}
                    count -= (sbyte)(64 - this[i].Count);
                    this[i].Count = 64;
				}
			}

			// Quickslots, not stacking
            for (short i = (short)InventorySlots.QuickSlotFirst; i <= (short)InventorySlots.QuickSlotLast; i++)
			{
                if (ItemHelper.IsVoid(this[i]))
				{
                    if (isInGame)
                    {
                        Owner.Client.SendPacket(new ChatMessagePacket {Message = "Placing in slot " + i});
                    }
                    this[i] = ItemHelper.GetInstance(id);
                    this[i].Count = count;
                    this[i].Durability = durability;
                    //this[i] = new ItemStack(id, count, durability) { Slot = i };
					return;
				}
			}

			// Inventory, not stacking
            for (short i = (short)InventorySlots.InventoryFirst; i <= (short)InventorySlots.InventoryLast; i++)
			{
                if (ItemHelper.IsVoid(this[i]))
				{
                    this[i] = ItemHelper.GetInstance(id);
                    this[i].Count = count;
                    this[i].Durability = durability;
                    //this[i] = new ItemStack(id, count, durability) { Slot = i };
					return;
				}
			}

            Owner.MarkToSave();
		}

        public void RemoveItem(short slot)
        {
            if (this[slot].Type > 0)
            {
                if (this[slot].Count == 1)
                    this[slot] = ItemHelper.Void;
                else
                    this[slot].Count -= 1;
            }
            Owner.MarkToSave();
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
