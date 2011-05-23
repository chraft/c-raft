using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Chraft.Interfaces.Recipes
{
	public class Recipe
	{
		public ItemStackChraft[,] Ingredients3 { get; private set; }
		public ItemStackChraft[] Ingredients2 { get; set; }
		public ItemStackChraft[,] Products { get; private set; }
		public ItemStackChraft Result { get; private set; }
		public bool AnyOrder { get; private set; }

		private Recipe(ItemStackChraft result, ItemStackChraft[,] ingredients, ItemStackChraft[,] products, bool anyOrder)
		{
			Ingredients3 = ingredients;
			Result = result;
			Products = products;
			AnyOrder = anyOrder;

			List<ItemStackChraft> ings = new List<ItemStackChraft>();
			for (int h = 0; h < ingredients.GetLength(0); h++)
				for (int w = 0; w < ingredients.GetLength(1); w++)
					if (!ItemStackChraft.IsVoid(ingredients[h, w]))
						ings.Add(ingredients[h, w]);
			Ingredients2 = ings.ToArray();
		}

		private bool MatchesOrdered(ItemStackChraft[] ingredients)
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
							ItemStackChraft ing1 = Ingredients3[y - h, x - w];
							ItemStackChraft ing2 = ingredients[y * s + x];
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

		private bool MatchesUnordered(ItemStackChraft[] ingredients)
		{
			foreach (ItemStackChraft ing1 in Ingredients2)
			{
				foreach (ItemStackChraft ing2 in ingredients)
				{
					if (ing1.Type == ing2.Type && (ing1.Durability < 0 || ing1.Durability == ing2.Durability) && ing2.Count >= ing1.Count)
						goto continue1;
					continue;
				}
				return false;
			continue1:
				continue;
			}

			foreach (ItemStackChraft ing1 in ingredients)
			{
				foreach (ItemStackChraft ing2 in Ingredients2)
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

		private bool Matches(ItemStackChraft[] ingredients)
		{
			return AnyOrder ? MatchesUnordered(ingredients) : MatchesOrdered(ingredients);
		}

		public static Recipe GetRecipe(Recipe[] recipes, ItemStackChraft[] ingredients)
		{
			foreach (Recipe r in recipes)
			{
				if (r.Matches(ingredients))
				{
					Console.WriteLine("Matches recipe: " + r.Result.Type + " " + r.Result.Count);
					return r;
				}
			}
			return null;
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
				ItemStackChraft result = ItemStackChraft.Parse(r[0].Trim('[', ']'));
				r.RemoveAt(0);

				int height = r.Count;
				int width = r[0].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
				ItemStackChraft[,] ing = new ItemStackChraft[height, width];
				for (int h = 0; h < height; h++)
				{
					string[] items = r[0].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					for (int w = 0; w < width; w++)
						ing[h, w] = ItemStackChraft.Parse(items[w]);
				}

				recipes.Add(new Recipe(result, ing, new ItemStackChraft[3, 3], anyOrder));
			}

			return recipes.ToArray();
		}
	}
}
