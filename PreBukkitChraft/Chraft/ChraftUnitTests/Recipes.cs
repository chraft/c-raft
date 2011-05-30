using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            List<ItemStack> ingredients = new List<ItemStack>();
            ingredients.Add(new ItemStack(5, 1, 0));
            ingredients.Add(new ItemStack(5, 1, 0));
            ingredients.Add(new ItemStack(5, 1, 0));
            ingredients.Add(new ItemStack(5, 1, 0));
            //Assert.IsTrue(Recipe.GetRecipe(recipes, ingredients.ToArray()).Result.Type.Equals(58));
        }
    }
}
