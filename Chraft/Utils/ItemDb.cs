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
using System.IO;
using Chraft.Entity.Items;
using Chraft.PluginSystem.Item;
using Chraft.Utilities.Config;

namespace Chraft.Utils
{
    public class ItemDb : IItemDb
    {
        private Dictionary<string, short> Items = new Dictionary<string, short>();
        private Dictionary<string, short> Durabilities = new Dictionary<string, short>();

        internal IItemInventory this[string item]
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

                    if (Contains(item))
                    {
                        var res = ItemHelper.GetInstance(Items[item]);
                        res.Count = ChraftConfig.DefaultStackSize;
                        res.Durability = Durabilities[item];
                        return res;
                    }
                    else
                    {
                        return ItemHelper.Void;
                    }
                }
                catch (Exception)
                {
                    return ItemHelper.Void;
                }
            }
        }

        public IItemInventory GetItem(string item)
        {
            return this[item];
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
