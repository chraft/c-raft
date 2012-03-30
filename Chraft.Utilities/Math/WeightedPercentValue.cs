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

namespace Chraft.Utilities.Math
{
    /// <summary>
    /// A proportion value based on a percent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WeightedPercentValue<T>
    {
        public double Proportion { get; set; }
        public T Value { get; set; }
    }

    /// <summary>
    /// var list = new[] {
    /// ProportionValue.Create(0.7, "a"),
    /// ProportionValue.Create(0.2, "b"),
    /// ProportionValue.Create(0.1, "c")
    /// };
    /// // Outputs "a" with probability 0.7, etc.
    /// Console.WriteLine(list.ChooseByRandom());
    /// 
    /// var list = new List&gt;int&lt;();
    /// list.Add(1);
    /// list.Add(2);
    /// list.ChooseByRandom(); // outputs 1 with a probability of 0.5
    /// </summary>
    public static class WeightedPercentValue
    {
        public static WeightedPercentValue<T> Create<T>(double proportion, T value)
        {
            return new WeightedPercentValue<T> { Proportion = proportion, Value = value };
        }

        /// <summary>
        /// Returns a random item from the <see cref="WeightedValue{T}"/> collection taking into consideration the percent chance for that item. The sum of the proportions must equal 100% (1.0)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static T SelectRandom<T>(this IEnumerable<WeightedPercentValue<T>> collection, Random random)
        {
            var rnd = random.NextDouble();
            foreach (var item in collection)
            {
                if (rnd < item.Proportion)
                    return item.Value;
                rnd -= item.Proportion;
            }
            throw new InvalidOperationException(
                "The proportions in the collection do not add up to 1.");
        }
    }
}
