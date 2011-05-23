using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces.Recipes;
using Chraft.Net;

namespace Chraft.Interfaces
{
	public class WorkbenchInterface : Interface
	{
		private Recipe[] Recipes;

		public WorkbenchInterface()
			: base(InterfaceType.Workbench, 10)
		{
		}

		public override void Associate(Client client)
		{
			Recipes = client.Server.Recipes;
			base.Associate(client);
		}

		private Recipe GetRecipe()
		{
			List<ItemStackChraft> ingredients = new List<ItemStackChraft>();
			for (short i = 1; i <= 9; i++)
				ingredients.Add(ItemStackChraft.IsVoid(Slots[i]) ? ItemStackChraft.Void : this[i]);
			return Recipe.GetRecipe(Recipes, ingredients.ToArray());
		}

		internal override void OnClicked(WindowClickPacket packet)
		{
			if (packet.Slot >= 1 && packet.Slot <= 4)
			{
				Recipe recipe = GetRecipe();
				if (recipe == null)
				{
					this[0].Type = 0;
					this[0].Durability = 0;
				}
				else
				{
					this[0] = recipe.Result;
				}
			}

			if (packet.Slot == 0)
			{
				if (!ItemStackChraft.IsVoid(Cursor))
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
					this[0].Count += Cursor.Count;
					Cursor = ItemStackChraft.Void;
				}
				else
				{
					this.Cursor = ItemStackChraft.Void;
					this.Cursor.Slot = -1;
					this.Cursor.Type = this[0].Type;
					this.Cursor.Durability = this[0].Durability;
				}

				this.Cursor.Count += this[0].Count;
				this[0] = ItemStackChraft.Void;

				List<ItemStackChraft> ingredients = new List<ItemStackChraft>();
				for (short i = 1; i <= 9; i++)
					ingredients.Add(ItemStackChraft.IsVoid(Slots[i]) ? ItemStackChraft.Void : this[i]);
				Recipe recipe = GetRecipe();
				for (int i = 0; i < ingredients.Count; i++)
				{
					if (ItemStackChraft.IsVoid(ingredients[i]))
						continue;
					for (int i2 = 0; i2 < recipe.Ingredients2.Length; i2++)
					{
						if (ingredients[i].Type == recipe.Ingredients2[i2].Type)
						{
							ingredients[i].Count -= recipe.Ingredients2[i2].Count;
							break;
						}
					}
				}
			}
			else
			{
				base.OnClicked(packet);
			}
		}
	}
}
