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
using Chraft.Entity.Items;
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

        internal virtual ItemInventory[] Slots { get; set; }
        public short SlotCount { get { return (short)Slots.Length; } }
		public string Title { get; set; }
        internal sbyte Handle { get; set; }
		internal InterfaceType Type { get; private set; }
		//protected internal PacketHandler PacketHandler { protected get; set; }
        internal ItemInventory Cursor { get; set; }

		protected bool _IsOpen = false;
		public bool IsOpen { get { return _IsOpen; } }

        internal virtual ItemInventory this[short slot]
		{
			get
			{
				return Slots[slot];
			}
			set
			{
                if (Slots[slot] != null)
					Slots[slot].Changed -= ItemStack_Changed;
                Slots[slot] = value ?? ItemHelper.Void;
                //Do not keep a slot number in the item object itself, manage it on an interface level
				Slots[slot].Changed += ItemStack_Changed;
				SendUpdate(slot);
			}
		}

		/// <summary>
		/// Instantiates an empty Interface shell for use with serialization.
		/// </summary>
		public Interface()
		{
			Cursor = ItemHelper.Void;
		}

		internal Interface(InterfaceType type, sbyte slotCount) : this()
		{
			Type = type;
			Handle = NextHandle;
			Slots = new ItemInventory[slotCount];
			Title = "C#raft Interface";
			NextHandle = NextHandle == 127 ? (sbyte)1 : (sbyte)(NextHandle + 1); // Handles between 1 and 127. 0 is reserved for Inventory
		}

        public IItemInventory[] GetSlots()
        {
            return Slots;
        }

        public IItemInventory GetItem(short slot)
        {
            return this[slot];
        }

        public void SetItem(short slot, IItemInventory newItem)
        {
            this[slot] = newItem as ItemInventory;
        }

        public IItemInventory GetCursor()
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
                    if (ItemHelper.IsVoid(Cursor))
					{	// Empty click in void: ignore
						e.Cancel();
					}
					else if (e.RightClick)
					{	// Right-click in void: drop item
					    var item = ItemHelper.GetInstance(Cursor.Type);
					    item.Durability = Cursor.Durability;
					    item.Damage = Cursor.Damage;
					    item.Count = 1;
						Owner.Server.DropItem(Owner, item);
						Cursor.Count--;
					}
					else
					{	// Left-click in void: drop stack
						Owner.Server.DropItem(Owner, Cursor);
						Cursor = ItemHelper.Void;
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
                if (ItemHelper.IsVoid(Cursor))
					Cursor = ItemHelper.Void;
                if (ItemHelper.IsVoid(target.Slots[e.Slot]))
					target[e.Slot] = ItemHelper.Void;

				// The fun begins.
				if (target.Slots[e.Slot].StacksWith(Cursor))
				{
                    if (ItemHelper.IsVoid(Cursor))
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
							target[e.Slot].Count++;
							Cursor.Count--;
						}
					}
					else
					{
                        // Left-click on same item
						int total = target.Slots[e.Slot].Count + Cursor.Count;
						if (total <= 64)
						{	// Move all items to stack
							target[e.Slot].Count = unchecked((sbyte)total);
							Cursor.Count = 0;
						}
						else
						{	// Make stack 64, and put remainder in cursor
							target[e.Slot].Count = 64;
							Cursor.Count = unchecked((sbyte)(total - 64));
						}
					}
				}
                else if (!ItemHelper.IsVoid(Cursor) && e.RightClick && ItemHelper.IsVoid(target.Slots[e.Slot]))
                {
                    // Right-click on empty slot with items in cursor: drop one item from Cursor into slot

                    target[e.Slot] = Cursor.Clone();
                    target[e.Slot].Count = 1;
                    Cursor.Count--;
                    if (Cursor.Count == 0)
                    	Cursor = ItemHelper.Void;
                }
                else if (e.RightClick && ItemHelper.IsVoid(Cursor))
                {	// Right-click with empty cursor: split stack in half
                    sbyte count = target.Slots[e.Slot].Count;
                    target[e.Slot].Count /= 2;
                    count -= target.Slots[e.Slot].Count;
                    Cursor = target.Slots[e.Slot];
                    Cursor.Count = count;
                    //Cursor = new ItemStack(target.Slots[e.Slot].Type, (sbyte)count, target.Slots[e.Slot].Durability);
                }
                else if (e.RightClick)
                {	// Right-click on different type: ignored click
                    e.Cancel();
                }
                else
                {	// Left-click on different type: swap stacks
                    var swap = target[e.Slot];
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
				var item = slot < 0 ? Cursor : Slots[slot];
                Owner.Client.SendPacket(new SetSlotPacket
				{
                    Item = ItemHelper.IsVoid(item) ? ItemHelper.Void : item,
					Slot = slot,
					WindowId = Handle
				});
			}
		}

        protected virtual void OnItemStackChanged(short slot)
        {
            SendUpdate(slot);
        }

        private void ItemStack_Changed(object sender, EventArgs e)
        {
            OnItemStackChanged(((ItemInventory)sender).Slot);
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
                if (Slots[i] != null && !ItemHelper.IsVoid(Slots[i]))
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
            if (!ItemHelper.IsVoid(Cursor))
            {
                Owner.Server.DropItem(Owner, Cursor);
                Cursor = ItemHelper.Void;
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
                var stack = Slots[i];
                if (!ItemHelper.IsVoid(stack))
                {
                    Owner.Server.DropItem(Owner.World, coords, stack);
                    this[i] = ItemHelper.Void;
                }
            }
        }

        public bool IsEmpty()
        {
            bool empty = true;
            foreach (var item in this.Slots)
            {
                if (!ItemHelper.IsVoid(item))
                {
                    empty = false;
                    break;
                }
            }
            return empty;
        }
	}
}
