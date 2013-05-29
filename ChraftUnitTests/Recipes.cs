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
using Chraft.Interfaces;
using Chraft.Interfaces.Recipes;
//using NUnit.Framework;

namespace ChratUnitTests
{
    public class Recipies
    {
        //[Test]
        public void MakeWorkBench()
        {
            Recipe[] recipes = Recipe.FromFile("Resources/Recipes.dat");
            var ingredients = new List<ItemInventory>();
            /*ingredients.Add(new ItemStack(5, 1, 0));
            ingredients.Add(new ItemStack(5, 1, 0));
            ingredients.Add(new ItemStack(5, 1, 0));
            ingredients.Add(new ItemStack(5, 1, 0));*/
            //Assert.IsTrue(Recipe.GetRecipe(recipes, ingredients.ToArray()).Result.Type.Equals(58));
        }
    }
}
