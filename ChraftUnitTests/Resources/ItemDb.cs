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
using System.IO;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Interfaces;
using Chraft.Utilities.Config;

namespace ChratUnitTests.Resources
{
	public class ItemDb
	{
		private Dictionary<string, short> Items = new Dictionary<string, short>();
		private Dictionary<string, short> Durabilities = new Dictionary<string, short>();

		public ItemInventory this[string item]
		{
			get
			{
                if (Contains(item))
                {
                    var i = ItemHelper.GetInstance(Items[item]);
                    i.Count = ChraftConfig.DefaultStackSize;
                    i.Durability = Durabilities[item];
                    return i;
                }
                else
                    return ItemHelper.Void;
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
