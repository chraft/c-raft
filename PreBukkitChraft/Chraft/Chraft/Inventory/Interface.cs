using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using System.Runtime.Serialization;
using Chraft.Persistence;

namespace Chraft.Inventory
{
	[Serializable]
	public abstract class Interface
	{
		private static volatile sbyte NextHandle = 0;
		private bool IsTransactionInProgress = false;
		private Client Client;

		public event EventHandler<InterfaceClickedEventArgs> Clicked;

		public ItemStack[] Slots { get; set; }
		public sbyte SlotCount { get { return (sbyte)Slots.Length; } }
		public string Title { get; set; }
		internal sbyte Handle { get; private set; }
		internal InterfaceType Type { get; private set; }
		protected internal PacketHandler PacketHandler { protected get; set; }
		public ItemStack Cursor { get; set; }

		protected bool _IsOpen = false;
		public bool IsOpen { get { return _IsOpen; } }

		public ItemStack this[short slot]
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
				Slots[slot].Slot = slot;
				Slots[slot].Changed += ItemStack_Changed;
				SendUpdate(slot);
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
			NextHandle = NextHandle == 127 ? (sbyte)0 : (sbyte)(NextHandle + 1);
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
						Client.Server.DropItem(Client, new ItemStack(Cursor.Type, 1, Cursor.Durability));
						Cursor.Count--;
					}
					else
					{	// Left-click in void: drop stack
						Client.Server.DropItem(Client, Cursor);
						Cursor = ItemStack.Void;
					}
					return;

				case ClickLocation.Interface:
					target = e.Interface;
					break;

				case ClickLocation.Inventory:
					target = Client.Inventory;
					break;
				}

				// Ensure a true void stack for our calculations
				if (ItemStack.IsVoid(Cursor))
					Cursor = ItemStack.Void;
				if (ItemStack.IsVoid(target.Slots[e.Slot]))
					target.Slots[e.Slot] = ItemStack.Void;

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
					ItemStack swap = target.Slots[e.Slot];
					target.Slots[e.Slot] = Cursor;
					Cursor = swap;
				}
			}
			catch (Exception ex)
			{
				e.Cancel();
				Client.SendMessage("§cInventory Error: " + ex.Message);
				Client.Logger.Log(ex);
			}
			finally
			{
				PacketHandler.SendPacket(new TransactionPacket
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
			if (IsOpen && !IsTransactionInProgress)
			{
				ItemStack item = slot < 0 ? Cursor : Slots[slot];
				PacketHandler.SendPacket(new SetSlotPacket
				{
					Item = ItemStack.IsVoid(item) ? ItemStack.Void : item,
					Slot = slot,
					WindowId = Handle
				});
			}
		}

		private void ItemStack_Changed(object sender, EventArgs e)
		{
			SendUpdate(((ItemStack)sender).Slot);
		}

		public virtual void Associate(Client client)
		{
			Client = client;
			client.AssociateInterface(this);
		}

		public void Open()
		{
			PacketHandler.SendPacket(new OpenWindowPacket
			{
				WindowId = Handle,
				WindowTitle = Title,
				InventoryType = Type,
				SlotCount = SlotCount
			});
			UpdateClient();
			_IsOpen = true;
		}

		public virtual void UpdateClient()
		{
			for (short i = 0; i < 45; i++)
			{
				if (!ItemStack.IsVoid(Slots[i]))
					SendUpdate(i);
			}
		}

		public void UpdateCursor()
		{
			SendUpdate(-1);
		}

		public void Close()
		{
			_IsOpen = false;
			PacketHandler.SendPacket(new CloseWindowPacket
			{
				WindowId = Handle
			});
		}
	}
}
