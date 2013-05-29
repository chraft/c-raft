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
using System.IO;
using System.Xml.Linq;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;

namespace Chraft.Interfaces.Recipes
{
	public class Recipe
	{
        public ItemInventory[,] Ingredients3 { get; private set; }
        public ItemInventory[] Ingredients2 { get; set; }
        public ItemInventory[,] Products { get; private set; }
        public ItemInventory Result { get; private set; }
		public bool AnyOrder { get; private set; }

        private Recipe(ItemInventory result, ItemInventory[,] ingredients, ItemInventory[,] products, bool anyOrder)
		{
		    Ingredients3 = ingredients;
			Result = result;
			Products = products;
			AnyOrder = anyOrder;

            var ings = new List<ItemInventory>();
			for (int h = 0; h < ingredients.GetLength(0); h++)
				for (int w = 0; w < ingredients.GetLength(1); w++)
                    if (ingredients[h, w] != null && !ItemHelper.IsVoid(ingredients[h, w]))
						ings.Add(ingredients[h, w]);
			Ingredients2 = ings.ToArray();
		}

        private bool MatchesOrdered(ItemInventory[] ingredients)
		{
			int s = ingredients.Length == 4 ? 2 : 3;
			int dw = s - Ingredients3.GetLength(1);
			int dh = s - Ingredients3.GetLength(0);

			for (int w = 0; w <= dw; w++)
			{
				for (int h = 0; h <= dh; h++)
				{
					for (int x = w; x < w + Ingredients3.GetLength(1); x++)
					{
						for (int y = h; y < h + Ingredients3.GetLength(0); y++)
						{
							var ing1 = Ingredients3[y - h, x - w];
							var ing2 = ingredients[y * s + x];
                            if (ItemHelper.IsVoid(ing1) && ItemHelper.IsVoid(ing2))
                                continue;
                             if (ing1.Type == ing2.Type && (ing1.Durability < 0 || ing1.Durability == ing2.Durability) && ing2.Count >= ing1.Count)
								continue;
							goto continue1;
						}
					}
					return true;
				continue1:
					continue;
				}
			}
			return false;
		}

        private bool MatchesUnordered(ItemInventory[] ingredients)
		{
			foreach (var ing1 in Ingredients2)
			{
				foreach (var ing2 in ingredients)
				{
					if (ing1.Type == ing2.Type && (ing1.Durability < 0 || ing1.Durability == ing2.Durability) && ing2.Count >= ing1.Count)
						goto continue1;
					continue;
				}
				return false;
			continue1:
				continue;
			}

			foreach (var ing1 in ingredients)
			{
				foreach (var ing2 in Ingredients2)
				{
					if (ing1.Type == ing2.Type && (ing1.Durability < 0 || ing1.Durability == ing2.Durability) && ing2.Count >= ing1.Count)
						goto continue2;
					continue;
				}
				return false;
			continue2:
				continue;
			}
			return true;
		}

        private bool Matches(ItemInventory[] ingredients)
		{
            // 1. check that the correct number of ingredients exist (quickly excludes any recipes of differing number of ingredients)
            //    fixes #69: "Recipies do not evaluate full recipe, they select first found"
            if (MatchesIngredientsCount(ingredients))
            {
                // 2. now do the check for whether this recipe matches the provided ingredients
                return AnyOrder ? MatchesUnordered(ingredients) : MatchesOrdered(ingredients);
            }
            else
            {
                return false;
            }
		}

        /// <summary>
        /// This ensures that we are matching the correct number of ingredients (e.g. two separate blocks of wood, not three). Fixes #69 (https://www.assembla.com/spaces/chraft/tickets/69-recipies-do-not-evaluate-full-recipe--they-select-first-found).
        /// </summary>
        /// <param name="ingredients">ingredients to count</param>
        /// <returns>true if the number of individual ingredients match</returns>
        private bool MatchesIngredientsCount(ItemInventory[] ingredients)
        {
            return (Ingredients2.Length - CountVoidIngredients(Ingredients2)) == (ingredients.Length - CountVoidIngredients(ingredients));
        }

        /// <summary>
        /// Counts the number of ingredients that are Void items
        /// </summary>
        /// <param name="ingredients"></param>
        /// <returns>The number of ItemStack.IsVoid() ingredients</returns>
        private int CountVoidIngredients(ItemInventory[] ingredients)
        {
            int result = 0;
            foreach (var item in ingredients)
            {
                if (ItemHelper.IsVoid(item))
                    result++;
            }

            return result;
        }

        /// <summary>
        /// Subtracts the ingredients that are required to craft this recipe, by updating the ingredient.Count property
        /// </summary>
        /// <param name="ingredients">The ingredients to be used</param>
        public void UseIngredients(ItemInventory[] ingredients)
        {
            // Assumption: 
            // Recipes ingredients are loaded left to right, top to bottom, which is the same order of workbench/inventory recipe slots
            // e.g. 1  2 <- inventory
            //      3  4
            //-------------
            //      1  2  3 <- workbench
            //      4  5  6
            //      7  8  9
            int indx = 0;
            foreach (var item in this.Ingredients2)
            {
                // Based on assumption about load order: simply getting the next non-void ingredient will match the current recipe ingredient, then subtract the number required.
                for (int i = indx; i < ingredients.Length; i++)
                {
                    if (!ItemHelper.IsVoid(ingredients[i]))
                    {
                        ingredients[i].Count -= item.Count;
                        indx = i + 1;
                        break;
                    }
                }
            }
        }

        public static Recipe GetRecipe(Recipe[] recipes, ItemInventory[] ingredients)
		{
			foreach (Recipe r in recipes)
			{
				if (r.Matches(ingredients))
					return r;
			}
			return null;
		}

        public static Recipe[] FromXmlFile(string file)
        {
            // Define variables
            List<Recipe> loadedRecipes = new List<Recipe>();
            XDocument document;

            // Load the recipes file
            document = XDocument.Load(file);

            // Get all of the recipe elements
            var recipes = document.Descendants("Recipes").Descendants("Recipe");

            // Loop through the recipe elements
            foreach (XElement recipe in recipes)
            {
                // Define variables
                ItemInventory result;
                ItemInventory[,] ingredients;
                bool freeformRecipe = false;
                int rowCount = 0, row = 0;

                // Determine the resulting item
                string amount = recipe.Descendants("Amount").First().Value;
                string id = recipe.Attribute("Id").Value;

                // - Create the stack
                result = ItemHelper.Parse(string.Format("{0}#{1}", id, amount));

                // Determine whether or not this is a free-from recipe
                string match = recipe.Attribute("Match").Value;

                // - Check the value
                if (match.ToLower() == "any")
                    freeformRecipe = true;

                // Load the rows
                var rows = recipe.Descendants("Rows").Descendants("Row");
                rowCount = rows.Count<XElement>();

                // Initialize the ingredients array
                ingredients = new ItemInventory[rowCount, 3];

                // Loop through the row elements
                foreach (XElement r in rows)
                {
                    // Define variables
                    string value = r.Value;
                    string[] items = value.Split(',');

                    // Loop through the items
                    for (int i = 0; i < items.Length; i++)
                    {
                        // Define variables
                        string item = items[i];

                        // Add the item stack to the ingredients list
                        ingredients[row, i] = ItemHelper.Parse(item);
                    }

                    // Increment the row variable
                    row++;
                }

                // Add the recipe to the list
                loadedRecipes.Add(new Recipe(result, ingredients, new ItemInventory[3, 3], freeformRecipe));
            }

            // Return the loaded recipes
            return loadedRecipes.ToArray();
        }

		public static Recipe[] FromFile(string file)
		{
			string[] lines = File.ReadAllLines(file);
			List<List<string>> recs = new List<List<string>>();

			for (int i = 0; i < lines.Length; i++)
			{
				string l = lines[i];
				if (!l.StartsWith("[") || !l.EndsWith("]"))
					continue;
				List<string> rec = new List<string>();
				while (i < lines.Length && lines[i] != "" && lines[i] != "=")
					rec.Add(lines[i++]);
				recs.Add(rec);
			}

			List<Recipe> recipes = new List<Recipe>();
			foreach (List<string> r in recs)
			{
				bool anyOrder = r[0].StartsWith("[[") && r[0].EndsWith("]]");
				var result = ItemHelper.Parse(r[0].Trim('[', ']'));
				r.RemoveAt(0);

				int height = r.Count;
				int width = r[0].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                var ing = new ItemInventory[height, width];
				for (int h = 0; h < height; h++)
				{
					string[] items = r[h].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					for (int w = 0; w < width; w++)
						ing[h, w] = ItemHelper.Parse(items[w]);
				}

                recipes.Add(new Recipe(result, ing, new ItemInventory[3, 3], anyOrder));
			}

			return recipes.ToArray();
		}
	}
}
