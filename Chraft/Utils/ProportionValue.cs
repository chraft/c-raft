using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Utils
{
    public class ProportionValue<T>
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
    /// </summary>
    public static class ProportionValue
    {
        public static ProportionValue<T> Create<T>(double proportion, T value)
        {
            return new ProportionValue<T> { Proportion = proportion, Value = value };
        }

        static Random random = new Random();
        public static T ChooseByRandom<T>(
            this IEnumerable<ProportionValue<T>> collection)
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
