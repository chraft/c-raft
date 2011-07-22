using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Chraft.Interfaces;
using Chraft.Properties;

namespace Chraft.Resources
{
	public class ItemDb
	{
		private Dictionary<string, short> Items = new Dictionary<string, short>();
		private Dictionary<string, short> Durabilities = new Dictionary<string, short>();

		public ItemStack this[string item]
		{
			get
			{
				if (Contains(item))
					return new ItemStack(Items[item], Settings.Default.DefaultStackSize, Durabilities[item]);
				else
					return ItemStack.Void;
			}
		}

		public ItemDb(string file)
		{
			if (!File.Exists(file))
				return;

			foreach (String l in File.ReadAllLines(file))
			{
				if (l.StartsWith("#"))
					continue;

				string[] parts = l.Split(',');
				if (parts.Length < 2)
					continue;

				short numeric;
				if (!short.TryParse(parts[1], out numeric))
					continue;

				short durability;
				if (parts.Length < 3 || !short.TryParse(parts[2], out durability))
					durability = 0;

				string item = parts[0].ToLower();
				Items.Add(item, numeric);
				Durabilities.Add(item, durability);
			}
		}

		public bool Contains(string item)
		{
			short numeric;
			return Items.ContainsKey(item) || (short.TryParse(item, out numeric) && Items.ContainsValue(numeric));
		}

        public string ItemName(short item) // Returns top item name (...or Use Enum.Parse instead?)
        {
            foreach (KeyValuePair<string, short> kvp in Items)
            {
                if (kvp.Value == item)
                {
                    return kvp.Key;
                }
            }

            return "Not Found";

        }
	}
}
