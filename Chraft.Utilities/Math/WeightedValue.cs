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
using System.Diagnostics;
using System.Linq;

namespace Chraft.Utilities.Math
{
    /// <summary>
    /// A weighted value based on an arbitrary integer. For a random selection based on weights using percentages see <see cref="WeightedPercentValue{T}"/>.
    /// The percent chance of this item being selected can be calculated as this.Weight / collection.Sum(weightedValue.Weight)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WeightedValue<T>
    {
        /// <summary>
        /// The weight to use.
        /// </summary>
        public int Weight { get; set; }
        public T Value { get; set; }
    }

    public static class WeightedValue
    {
        /// <summary>
        /// Create a WeightedValue with the provided weight and value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="weight">An arbitrary positive integer that forms the weight</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static WeightedValue<T> Create<T>(int weight, T value)
        {
            if (weight <= 0)
                throw new ArgumentOutOfRangeException("weight", "must be greater than 0");

            return new WeightedValue<T> { Weight = weight, Value = value };
        }

        /// <summary>
        /// Returns a random item from the WeightedValue collection. The percent chance of each item being selected can be calculated as item.Weight / collection.Sum(weightedValue.Weight)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static T SelectRandom<T>(this IEnumerable<WeightedValue<T>> collection, Random random)
        {
            Debug.Assert(collection != null, "collection != null");
            WeightedValue<T>[] collectionArray = collection.ToArray();
            int sum = collectionArray.Sum(item => item.Weight);

            int i = random.Next(sum);
            
            foreach(var item in collectionArray)
            {
                i -= item.Weight;
                if(i <= 0)
                {
                    return item.Value;
                }
            }

            return default(T);
        }
    }
}
