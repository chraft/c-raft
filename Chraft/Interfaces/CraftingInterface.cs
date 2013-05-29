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
using Chraft.Entity.Items.Base;
using Chraft.Interfaces.Recipes;
using Chraft.Net.Packets;

namespace Chraft.Interfaces
{
    [Serializable]
    public abstract class CraftingInterface : Interface
    {
        protected sbyte CraftingSlotCount { get; set; }

        public CraftingInterface()
            : base()
        {
        }

        internal CraftingInterface(InterfaceType type, sbyte craftingSlots, sbyte slotCount)
            : base(type, slotCount)
        {
            CraftingSlotCount = craftingSlots;
        }

        protected Recipe GetRecipe()
        {
            var ingredients = new List<ItemInventory>();
            for (short i = 1; i <= this.CraftingSlotCount; i++)
                ingredients.Add(ItemHelper.IsVoid(this[i]) ? ItemHelper.Void : this[i]);
            return Recipe.GetRecipe(Server.GetRecipes(), ingredients.ToArray());
        }

        internal override void OnClicked(WindowClickPacket packet)
        {
            if (packet.Slot == 0 && !ItemHelper.IsVoid(this[0]))
            {
                if (!ItemHelper.IsVoid(Cursor))
                {
                    if (Cursor.Type != this[0].Type || Cursor.Durability != this[0].Durability || Cursor.Count + this[0].Count > 64)
                    {
                        Owner.Client.SendPacket(new TransactionPacket
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
                    var item = ItemHelper.GetInstance(this[0].Type);
                    item.Durability = this[0].Durability;
                    item.Damage = this[0].Damage;
                    Cursor = item;
                }

                // Add the newly crafted item to the Cursor
                this.Cursor.Count += this[0].Count;

                // Cook Ingredients, and update recipe output slot in case ingredients are now insufficient for another
                if (!ItemHelper.IsVoid(this[0]))
                {
                    Recipe recipe = GetRecipe();
                    if (recipe != null)
                    {
                        var ingredients = new List<ItemInventory>();
                        for (short i = 1; i <= this.CraftingSlotCount; i++)
                            ingredients.Add(ItemHelper.IsVoid(this[i]) ? ItemHelper.Void : this[i]);

                        // Use the ingredients
                        recipe.UseIngredients(ingredients.ToArray());

                        // Check if any now have a count of 0 then set the slot to void
                        foreach (var item in ingredients)
                        {
                            if (!ItemHelper.IsVoid(item) && item.Count <= 0) // should never be less than 0, just some defensive coding
                            {
                                this[item.Slot] = ItemHelper.Void;
                            }
                        }

                        // We have to try and get the recipe again to make sure there is enough ingredients left to do one more
                        recipe = GetRecipe();
                        if (recipe == null)
                        {
                            // Not enough ingredients, set recipe output slot to void item
                            this[0] = ItemHelper.Void;
                        }
                    }
                }
            }
            else
            {
                base.OnClicked(packet);

                // If one of the ingredient slots have just been updated, update the contents of the recipe output slot
                if (packet.Slot >= 1 && packet.Slot <= this.CraftingSlotCount)
                {
                    Recipe recipe = GetRecipe();
                    if (recipe == null)
                        this[0] = ItemHelper.Void;
                    else
                        this[0] = recipe.Result;
                }
            }
        }
    }
}
