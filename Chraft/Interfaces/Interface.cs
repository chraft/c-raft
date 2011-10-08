using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using System.Runtime.Serialization;
using Chraft.Net.Packets;
using Chraft.Persistence;
using Chraft.World;


namespace Chraft.Interfaces
{
	[Serializable]
	public abstract class Interface 
	{
		private static volatile sbyte NextHandle = 0;
		private bool IsTransactionInProgress = false;
        protected Player Owner { get; private set; }

		public event EventHandler<InterfaceClickedEventArgs> Clicked;

		public virtual ItemStack[] Slots { get; set; }
        public short SlotCount { get { return (short)Slots.Length; } }
		public string Title { get; set; }
        internal sbyte Handle { get; set; }
		internal InterfaceType Type { get; private set; }
		//protected internal PacketHandler PacketHandler { protected get; set; }
		public ItemStack Cursor { get; set; }

		protected bool _IsOpen = false;
		public bool IsOpen { get { return _IsOpen; } }

        public virtual ItemStack this[int slot]
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
					if (ItemStack.IsVoid(Cursor))
					{	// Empty click in void: ignore
						e.Cancel();
					}
					else if (e.RightClick)
					{	// Right-click in void: drop item
						Owner.Server.DropItem(Owner.Client, new ItemStack(Cursor.Type, 1, Cursor.Durability));
						Cursor.Count--;
					}
					else
					{	// Left-click in void: drop stack
						Owner.Server.DropItem(Owner.Client, Cursor);
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
				if (ItemStack.IsVoid(Cursor))
					Cursor = ItemStack.Void;
				if (ItemStack.IsVoid(target.Slots[e.Slot]))
					target[e.Slot] = ItemStack.Void;

				// The fun begins.
				if (target.Slots[e.Slot].StacksWith(Cursor))
				{
					if (ItemStack.IsVoid(Cursor))
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
                else if (!ItemStack.IsVoid(Cursor) && e.RightClick && ItemStack.IsVoid(target.Slots[e.Slot]))
                {
                    // Right-click on empty slot with items in cursor: drop one item from Cursor into slot
                    target.Slots[e.Slot].Type = Cursor.Type;
                    target.Slots[e.Slot].Durability = Cursor.Durability;
                    target.Slots[e.Slot].Count = 1;
                    Cursor.Count--;
                    if (Cursor.Count == 0)
                    	Cursor = ItemStack.Void;
                }
                else if (e.RightClick && ItemStack.IsVoid(Cursor))
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
					Item = ItemStack.IsVoid(item) ? ItemStack.Void : item,
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

		public virtual void Associate(Player player)
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
				if (!ItemStack.IsVoid(Slots[i]))
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
            if (!ItemStack.IsVoid(Cursor))
            {
                Owner.Server.DropItem(Owner.Client, Cursor);
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
                if (!ItemStack.IsVoid(stack))
                {
                    Owner.Server.DropItem(Owner.World, coords, stack);
                    this[i] = ItemStack.Void;
                }
            }
        }

        protected bool IsEmpty()
        {
            bool empty = true;
            foreach (var item in this.Slots)
            {
                if (!ItemStack.IsVoid(item))
                {
                    empty = false;
                    break;
                }
            }
            return empty;
        }
	}
}
