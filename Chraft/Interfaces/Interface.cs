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
using System.Runtime.Serialization;
using Chraft.Net.Packets;
using Chraft.Persistence;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Item;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;
using Chraft.Entity;


namespace Chraft.Interfaces
{
	[Serializable]
	public abstract class Interface : IInterface
	{
		private static volatile sbyte NextHandle = 0;
		private bool IsTransactionInProgress = false;
        protected Player Owner { get; private set; }

		internal event EventHandler<InterfaceClickedEventArgs> Clicked;

		internal virtual ItemStack[] Slots { get; set; }
        public short SlotCount { get { return (short)Slots.Length; } }
		public string Title { get; set; }
        internal sbyte Handle { get; set; }
		internal InterfaceType Type { get; private set; }
		//protected internal PacketHandler PacketHandler { protected get; set; }
		internal ItemStack Cursor { get; set; }

		protected bool _IsOpen = false;
		public bool IsOpen { get { return _IsOpen; } }

        internal virtual ItemStack this[int slot]
		{
			get
			{
				return Slots[slot];
			}
			set
			{
                if (Slots[slot] != null)
					Slots[slot].Changed -= ItemStack_Changed;
				Slots[slot] = value ?? ItemStack.Void;
				Slots[slot].Slot = (short)slot;
				Slots[slot].Changed += ItemStack_Changed;
				SendUpdate((short)slot);
			}
		}

		/// <summary>
		/// Instantiates an empty Interface shell for use with serialization.
		/// </summary>
		public Interface()
		{
			Cursor = ItemStack.Void;
		}

		internal Interface(InterfaceType type, sbyte slotCount)
		{
			Type = type;
			Handle = NextHandle;
			Slots = new ItemStack[slotCount];
			Title = "C#raft Interface";
			NextHandle = NextHandle == 127 ? (sbyte)1 : (sbyte)(NextHandle + 1); // Handles between 1 and 127. 0 is reserved for Inventory
		}

        public IItemStack[] GetSlots()
        {
            return Slots;
        }

        public IItemStack GetItem(int slot)
        {
            return this[slot];
        }

        public void SetItem(int slot, IItemStack newItem)
        {
            this[slot] = newItem as ItemStack;
        }

        public IItemStack GetCursor()
        {
            return Cursor;
        }

		protected void StartTransaction()
		{
			IsTransactionInProgress = true;
		}

		protected void EndTransaction()
		{
			IsTransactionInProgress = false;
		}

		internal virtual void OnClicked(WindowClickPacket packet)
		{
			InterfaceClickedEventArgs e = new InterfaceClickedEventArgs(this, packet);
			if (Clicked != null)
				Clicked.Invoke(this, e);

			StartTransaction(); // Don't send update packets while we process this transaction, as that would be redundant.
			try
			{
				Interface target = null;
				switch (e.Location)
				{
				case ClickLocation.Void:
                    if (Cursor.IsVoid())
					{	// Empty click in void: ignore
						e.Cancel();
					}
					else if (e.RightClick)
					{	// Right-click in void: drop item
						Owner.Server.DropItem(Owner, new ItemStack(Cursor.Type, 1, Cursor.Durability));
						Cursor.Count--;
					}
					else
					{	// Left-click in void: drop stack
						Owner.Server.DropItem(Owner, Cursor);
						Cursor = ItemStack.Void;
					}
					return;

				case ClickLocation.Interface:
					target = e.Interface;
					break;

				case ClickLocation.Inventory:
					target = Owner.Inventory;
					break;
				}

				// Ensure a true void stack for our calculations
                if (Cursor.IsVoid())
					Cursor = ItemStack.Void;
                if (target.Slots[e.Slot].IsVoid())
					target[e.Slot] = ItemStack.Void;

				// The fun begins.
				if (target.Slots[e.Slot].StacksWith(Cursor))
				{
                    if (Cursor.IsVoid())
					{	// Useless click
						e.Cancel();
					}
					else if (e.RightClick)
					{	// Right-click on same item
						if (target.Slots[e.Slot].Count >= 64)
						{	// Stack is already full: ignore
							e.Cancel();
						}
						else
						{	// Increment stack
							target.Slots[e.Slot].Count++;
							Cursor.Count--;
						}
					}
					else
					{	// Left-click on same item
						int total = target.Slots[e.Slot].Count + Cursor.Count;
						if (total <= 64)
						{	// Move all items to stack
							target.Slots[e.Slot].Count = unchecked((sbyte)total);
							Cursor.Count = 0;
						}
						else
						{	// Make stack 64, and put remainder in cursor
							target.Slots[e.Slot].Count = 64;
							Cursor.Count = unchecked((sbyte)(total - 64));
						}
					}
				}
                else if (!Cursor.IsVoid() && e.RightClick && target.Slots[e.Slot].IsVoid())
                {
                    // Right-click on empty slot with items in cursor: drop one item from Cursor into slot
                    target.Slots[e.Slot].Type = Cursor.Type;
                    target.Slots[e.Slot].Durability = Cursor.Durability;
                    target.Slots[e.Slot].Count = 1;
                    Cursor.Count--;
                    if (Cursor.Count == 0)
                    	Cursor = ItemStack.Void;
                }
                else if (e.RightClick && Cursor.IsVoid())
                {	// Right-click with empty cursor: split stack in half
                    int count = target.Slots[e.Slot].Count;
                    target.Slots[e.Slot].Count /= 2;
                    count -= target.Slots[e.Slot].Count;
                    Cursor = new ItemStack(target.Slots[e.Slot].Type, (sbyte)count, target.Slots[e.Slot].Durability);
                }
                else if (e.RightClick)
                {	// Right-click on different type: ignored click
                    e.Cancel();
                }
                else
                {	// Left-click on different type: swap stacks
                    ItemStack swap = target[e.Slot];
                    target[e.Slot] = Cursor;
                    Cursor = swap;
                }
			}
			catch (Exception ex)
			{
				e.Cancel();
				Owner.Client.SendMessage("§cInventory Error: " + ex.Message);
				Owner.Client.Logger.Log(ex);
			}
			finally
			{
                Owner.Client.SendPacket(new TransactionPacket
				{
					Accepted = !e.Cancelled,
					Transaction = e.Transaction,
					WindowId = e.Interface.Handle
				});
				EndTransaction();
			}
		}

		protected void SendUpdate(short slot)
		{
			if (IsOpen && !IsTransactionInProgress && Owner != null)
			{
				ItemStack item = slot < 0 ? Cursor : Slots[slot];
                Owner.Client.SendPacket(new SetSlotPacket
				{
                    Item = item.IsVoid() ? ItemStack.Void : item,
					Slot = slot,
					WindowId = Handle
				});
			}
		}

        protected virtual void OnItemStackChanged(ItemStack stack)
        {
            SendUpdate(stack.Slot);
        }

        private void ItemStack_Changed(object sender, EventArgs e)
        {
            OnItemStackChanged(((ItemStack)sender));
		}

		internal virtual void Associate(Player player)
		{
			Owner = player;
			//Owner.Client.AssociateInterface(this);
		}

        protected virtual void DoOpen()
        {
        }

        public void Open()
		{
            Owner.Client.SendPacket(new OpenWindowPacket
			{
				WindowId = Handle,
				WindowTitle = Title,
				InventoryType = Type,
				SlotCount = (sbyte)SlotCount
			});

            DoOpen();
            _IsOpen = true;

            UpdateClient();
		}

		public virtual void UpdateClient()
		{
			for (short i = 0; i < SlotCount; i++)
			{
                if (Slots[i] != null && !Slots[i].IsVoid())
					SendUpdate(i);
			}
		}

		public void UpdateCursor()
		{
			SendUpdate(-1);
		}

        protected virtual void DoClose()
        {
            
            // Drop whatever is in the cursor

            //todo - determine why cursor was null
            if (Cursor != null && !Cursor.IsVoid())
            {
                Owner.Server.DropItem(Owner, Cursor);
                Cursor = ItemStack.Void;
            }
        }

        public void Close(bool sendCloseToClient)
		{
            if (_IsOpen)
            {
                _IsOpen = false;

                DoClose();

                if (sendCloseToClient)
                {
                    Owner.Client.SendPacket(new CloseWindowPacket
                    {
                        WindowId = Handle
                    });
                }
            }
		}

        public void DropAll(UniversalCoords coords)
        {
            // Drop all items from the workbench
            for (short i = 0; i < SlotCount; i++)
            {
                ItemStack stack = Slots[i];
                if (!stack.IsVoid())
                {
                    Owner.Server.DropItem(Owner.World, coords, stack);
                    this[i] = ItemStack.Void;
                }
            }
        }

        public bool IsEmpty()
        {
            bool empty = true;
            foreach (var item in this.Slots)
            {
                if (!item.IsVoid())
                {
                    empty = false;
                    break;
                }
            }
            return empty;
        }
	}
}
