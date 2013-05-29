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
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;

namespace Chraft.Interfaces.Recipes
{
    public class SmeltingRecipe
    {
        public ItemInventory Result { get; private set; }
        public ItemInventory Ingredient { get; private set; }
        private SmeltingRecipe(ItemInventory result, ItemInventory ingredient)
		{
		    Ingredient = ingredient;
			Result = result;
		}

        public static SmeltingRecipe GetRecipe(SmeltingRecipe[] recipes, ItemInventory ingredient)
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
            var lines = File.ReadAllLines(file);
            var recs = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                if (l.StartsWith("#") || !l.Contains(','))
                    continue;
                recs.Add(lines[i]);
            }

            var recipes = new List<SmeltingRecipe>();
            foreach (string r in recs)
            {
                string[] rec = r.Split(',');
                var result = ItemHelper.Parse(rec[1]);
                var ingredient = ItemHelper.Parse(rec[0]);
                recipes.Add(new SmeltingRecipe(result, ingredient));
            }

            return recipes.ToArray();
        }
    }
}
