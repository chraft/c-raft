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
                try
                {
                    short numeric = -1;
                    if(short.TryParse(item, out numeric))
                    {
                        item = ItemName(numeric);
                    }
                    return Contains(item) ? new ItemStack(Items[item], Settings.Default.DefaultStackSize, Durabilities[item]) : ItemStack.Void;
                }
                catch (Exception)
                {
                    return ItemStack.Void;
                }
            }
        }

        public ItemDb(string file)
        {
            if (!File.Exists(file))
                return;

            foreach (string[] parts in File.ReadAllLines(file).Where(l => !l.StartsWith("#")).Select(l => l.Split(',')).Where(parts => parts.Length >= 2))
            {
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
            short numeric = -1;
            if (Items.ContainsKey(item))
            {
                return true;
            }
            return (short.TryParse(item, out numeric) && Items.ContainsValue(numeric));
        }

        public string ItemName(short item) // Returns top item name (...or Use Enum.Parse instead?)
        {
            foreach (var kvp in Items.Where(kvp => kvp.Value == item))
            {
                return kvp.Key;
            }

            return "Not Found";
        }
    }
}
