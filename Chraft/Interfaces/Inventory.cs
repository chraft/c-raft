using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Chraft.Net;
using Chraft.Interfaces.Recipes;
using Chraft.Net.Packets;

namespace Chraft.Interfaces
{
	[Serializable]
	public partial class Inventory : Interface
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
		{
			_ActiveSlot = 36;
			_IsOpen = true;
		}

		internal Inventory(Client client)
			: base(InterfaceType.Inventory, 45)
		{
			_ActiveSlot = 36;
			Associate(client);
			_IsOpen = true;
			UpdateClient();
		}

		public override void Associate(Client client)
		{
			base.Associate(client);
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
			for (short i = 36; i < 43; i++)
				yield return this[i];
		}

		internal void AddItem(short id, sbyte count, short durability)
		{
			// Quickslots, stacking
			for (short i = 36; i < 43; i++)
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
			for (short i = 9; i < 36; i++)
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
			for (short i = 36; i < 43; i++)
			{
				if (ItemStack.IsVoid(Slots[i]))
				{
					PacketHandler.SendPacket(new ChatMessagePacket { Message = "Placing in slot " + i });
					this[i] = new ItemStack(id, count, durability) { Slot = i };
					return;
				}
			}

			// Inventory, not stacking
			for (short i = 9; i < 36; i++)
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

        internal bool DamageItem(short slot)
        {
            short durability = 0;

            World.BlockData.ToolDuarability.TryGetValue((World.BlockData.Items)this[slot].Type, out durability);

            if (durability > 0)
            {
                if (this[slot].Durability == durability)
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
                    this[slot].Durability++;
                    return true;
                }
            }

            return false;
        }

		private Recipe GetRecipe()
		{
			List<ItemStack> ingredients = new List<ItemStack>();
			for (short i = 1; i <= 4; i++)
				ingredients.Add(ItemStack.IsVoid(Slots[i]) ? ItemStack.Void : this[i]);
            return Recipe.GetRecipe(Server.GetRecipes(), ingredients.ToArray());
		}

		internal override void OnClicked(WindowClickPacket packet)
		{
            if (packet.Slot == 0 && !ItemStack.IsVoid(this[0]))
			{
				if (!ItemStack.IsVoid(Cursor))
				{
                    if (Cursor.Type != this[0].Type || Cursor.Durability != this[0].Durability || Cursor.Count + this[0].Count > 64)
					{
						PacketHandler.SendPacket(new TransactionPacket
						{
							Accepted = false,
							Transaction = packet.Transaction,
							WindowId = packet.WindowId                    
						});
						return;
					}
					// TODO: Why was this here? We can't modify the this[0] item as this will change the result of the recipe (it is currently a reference to recipe.Result)
                    //this[0].Count += Cursor.Count;
					//Cursor = ItemStack.Void;
				}
				else
				{
					this.Cursor = ItemStack.Void;
					this.Cursor.Slot = -1;
					this.Cursor.Type = this[0].Type;
					this.Cursor.Durability = this[0].Durability;
				}

				// Add the newly crafted item to the Cursor
                this.Cursor.Count += this[0].Count;

				// Cook Ingredients, and update recipe output slot in case ingredients are now insufficient for another
                if (!ItemStack.IsVoid(this[0]))
                {
                    Recipe recipe = GetRecipe();
                    if (recipe != null)
                    {
                        List<ItemStack> ingredients = new List<ItemStack>();
                        for (short i = 1; i <= 4; i++)
                            ingredients.Add(ItemStack.IsVoid(Slots[i]) ? ItemStack.Void : this[i]);
                        
                        // Use the ingredients
                        recipe.UseIngredients(ingredients.ToArray());

                        // Check if any now have a count of 0 then set the slot to void
                        foreach (var item in ingredients)
                        {
                            if (!ItemStack.IsVoid(item) && item.Count <= 0) // should never be less than 0, just some defensive coding
                            {
                                this[item.Slot] = ItemStack.Void;
                            }
                        }

                        // We have to try and get the recipe again to make sure there is enough ingredients left to do one more
                        recipe = GetRecipe();
                        if (recipe == null)
                        {
                            // Not enough ingredients, set recipe output slot to void item
                            this[0] = ItemStack.Void;
                        }
                    }
                }
			}
			else
			{
				base.OnClicked(packet);

                // If one of the ingredient slots have just been updated, update the contents of the recipe output slot
                if (packet.Slot >= 1 && packet.Slot <= 4)
                {
                    Recipe recipe = GetRecipe();
                    if (recipe == null)
                    {
                        this[0] = ItemStack.Void;
                    }
                    else
                    {
                        this[0] = recipe.Result;
                    }
                }
			}
		}
	}
}
