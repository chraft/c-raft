using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.bukkit.inventory;
using java.util;

namespace Chraft.Interfaces
{
	public partial class Inventory : PlayerInventory
	{
		public org.bukkit.inventory.ItemStack[] getArmorContents()
		{
            return null;
			//return new ItemStackChraft[] { Slots[5], Slots[6], Slots[7], Slots[8] };
		}

		public org.bukkit.inventory.ItemStack getBoots()
		{
            return null;
			//return Slots[8];
		}
        
		public org.bukkit.inventory.ItemStack getChestplate()
		{
            return null;
			//return Slots[6];
		}

		public int getHeldItemSlot()
		{
			return ActiveSlot;
		}

		public org.bukkit.inventory.ItemStack getHelmet()
		{
            return null;
			//return Slots[5];
		}

		public org.bukkit.inventory.ItemStack getItemInHand()
		{
            return null;
			//eturn ActiveItem;
		}

		public org.bukkit.inventory.ItemStack getLeggings()
		{
            return null;
			//return Slots[7];
		}

		public void setBoots(org.bukkit.inventory.ItemStack @is)
		{
			Slots[8] = new ItemStackChraft(@is, 8);
		}

		public void setChestplate(org.bukkit.inventory.ItemStack @is)
		{
			Slots[6] = new ItemStackChraft(@is, 6);
		}

		public void setHelmet(org.bukkit.inventory.ItemStack @is)
		{
			Slots[5] = new ItemStackChraft(@is, 5);
		}

		public void setItemInHand(org.bukkit.inventory.ItemStack @is)
		{
			Slots[ActiveSlot] = new ItemStackChraft(@is, ActiveSlot);
		}

		public void setLeggings(org.bukkit.inventory.ItemStack @is)
		{
			Slots[7] = new ItemStackChraft(@is, 7);
		}

		public HashMap addItem(params org.bukkit.inventory.ItemStack[] isarr)
		{
			throw new NotImplementedException();
		}

		public HashMap all(org.bukkit.inventory.ItemStack @is)
		{
			HashMap slots = new HashMap();
			if (@is != null)
				for (short i = 0; i < Slots.Length; i++)
					if (@is.equals(Slots[i]))
						slots.put(i, Slots[i]);
			return slots;
		}

		public HashMap all(org.bukkit.Material m)
		{
			return all(m.getId());
		}

		public HashMap all(int materialId)
		{
			HashMap slots = new HashMap();
			for (int i = 0; i < Slots.Length; i++)
			{
				ItemStackChraft item = Slots[i];
				if (item != null && item.Type == materialId)
					slots.put(i, item);
			}
			return slots;
		}

		public void clear()
		{
			for (short i = 0; i < Slots.Length; i++)
				clear(i);
		}

		public void clear(int i)
		{
			Slots[i] = ItemStackChraft.Void;
		}

		public bool contains(org.bukkit.inventory.ItemStack @is, int amount)
		{
			int amt = 0;
			foreach (ItemStackChraft item in Slots)
			{
				if (item != null && item.Equals(@is))
				{
					amt += item.Amount;
				}
			}
			return amt >= amount;
		}

		public bool contains(org.bukkit.Material material, int amount)
		{
			return contains(material.getId(), amount);
		}

		public bool contains(int materialId, int amount)
		{
			int amt = 0;
			foreach (ItemStackChraft item in Slots)
				if (item != null && item.Type == materialId)
					amt += item.Amount;
			return amt >= amount;
		}

		public bool contains(org.bukkit.inventory.ItemStack @is)
		{
			if (@is == null)
				return false;
			for (short i = 0; i < Slots.Length; i++)
				if (Slots[i].Equals(@is))
					return true;
			return false;
		}

		public bool contains(org.bukkit.Material m)
		{
			return contains(m.getId());
		}

		public bool contains(int materialId)
		{
			foreach (ItemStackChraft item in Slots)
				if (item != null && item.Type == materialId)
					return true;
			return false;
		}

		public int first(org.bukkit.inventory.ItemStack item)
		{
			if (item == null)
				return -1;
			for (int i = 0; i < Slots.Length; i++)
				if (item.equals(Slots[i]))
					return i;
			return -1;
		}

		public int first(org.bukkit.Material m)
		{
			return first(m.getId());
		}

		public int first(int s)
		{
			for (int i = 0; i < Slots.Length; i++)
				if (Slots[i].Type == s)
					return i;
			return -1;
		}

		public int firstEmpty()
		{
			for (int i = 0; i < Slots.Length; i++)
				if (Slots[i] == null)
					return i;
			return -1;
		}

		public org.bukkit.inventory.ItemStack[] getContents()
		{
            return null;
			//return Slots;
		}

		public org.bukkit.inventory.ItemStack getItem(int i)
		{
            return null;
			//return Slots[i];
		}

		public string getName()
		{
			return "Inventory";
		}

		public int getSize()
		{
			return Slots.Length;
		}

		public void remove(org.bukkit.inventory.ItemStack @is)
		{
			for (short i = 0; i < Slots.Length; i++)
				if (Slots[i].equals(@is))
					clear(i);
		}

		public void remove(org.bukkit.Material m)
		{
			remove(m.getId());
		}

		public void remove(int m)
		{
			for (int i = 0; i < Slots.Length; i++)
				if (Slots[i].Type == m)
					clear(i);
		}

		public HashMap removeItem(params org.bukkit.inventory.ItemStack[] items)
		{
			HashMap leftover = new HashMap();

			// TODO: optimization

			for (int i = 0; i < items.Length; i++)
			{
				org.bukkit.inventory.ItemStack item = items[i];
				int toDelete = item.getAmount();

				while (toDelete>0)
				{
					int f = first(item.getType());

					// Drat! we don't have this type in the inventory
					if (f == -1)
					{
						item.setAmount(toDelete);
						leftover.put(i, item);
						break;
					}
					else
					{
						org.bukkit.inventory.ItemStack itemStack = getItem(f);
						int amount = itemStack.getAmount();

						if (amount <= toDelete)
						{
							toDelete -= amount;
							// clear the slot, all used up
							clear(f);
						}
						else
						{
							// split the stack and store
							itemStack.setAmount(amount - toDelete);
							setItem(f, itemStack);
							toDelete = 0;
						}
					}
				}
			}
			return leftover;
		}

		public void setContents(org.bukkit.inventory.ItemStack[] isarr)
		{
			for (short i = 0; i < isarr.Length; i++)
				Slots[i] = new ItemStackChraft(isarr[i], i);
		}

		public void setItem(int i, org.bukkit.inventory.ItemStack @is)
		{
			Slots[i] = new ItemStackChraft(@is, (short)i);
		}
	}
}
