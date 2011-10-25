using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Chraft.Interfaces.Recipes
{
    public class SmeltingRecipe
    {
        public ItemStack Result { get; private set; }
        public ItemStack Ingredient { get; private set; }
        private SmeltingRecipe(ItemStack result, ItemStack ingredient)
		{
		    Ingredient = ingredient;
			Result = result;
		}

        public static SmeltingRecipe GetRecipe(SmeltingRecipe[] recipes, ItemStack ingredient)
        {
            foreach (SmeltingRecipe r in recipes)
            {
                if (r.Ingredient.StacksWith(ingredient))
                    return r;
            }
            return null;
        }

        public static SmeltingRecipe[] FromFile(string file)
        {
            string[] lines = File.ReadAllLines(file);
            List<string> recs = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                if (l.StartsWith("#") || !l.Contains(','))
                    continue;
                recs.Add(lines[i]);
            }

            List<SmeltingRecipe> recipes = new List<SmeltingRecipe>();
            foreach (string r in recs)
            {
                string[] rec = r.Split(',');
                ItemStack result = ItemStack.Parse(rec[1]);
                ItemStack ingredient = ItemStack.Parse(rec[0]);
                recipes.Add(new SmeltingRecipe(result, ingredient));
            }

            return recipes.ToArray();
        }
    }
}
