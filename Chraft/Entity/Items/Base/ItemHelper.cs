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
using System.Collections.Concurrent;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity.Items.Base;
using Chraft.Net;
using Chraft.PluginSystem.Item;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    public sealed class ItemHelper
    {
        private static ConcurrentDictionary<short, Type> _itemClasses;

        private static readonly ItemHelper _instance = new ItemHelper();

        public static ItemHelper Instance { get { return _instance; } }

        private ItemHelper()
        {
            Init();
        }

        private static void Init()
        {
            _itemClasses = new ConcurrentDictionary<short, Type>();

            ItemBase item;
            Type itemClass;
            short itemId;
            foreach (Type t in from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.GetInterfaces().Contains(typeof(IItemInventory)) && !t.IsAbstract
                               select t)
            {
                itemClass = t;
                item = (ItemInventory)itemClass.GetConstructor(Type.EmptyTypes).Invoke(null);
                itemId = item.Type;
                _itemClasses.TryAdd(itemId, itemClass);
            }
        }

        public static bool IsVoid(ItemInventory item)
        {
            return (item == null || item.Type == -1 || item.Count < 1);
        }

        public static bool IsVoid(IItemInventory item)
        {
            return (item == null | (item as ItemInventory).Type == -1 || (item as ItemInventory).Count < 1);
        }

        public static ItemInventory Void
        {
            get { return new ItemVoid(); }
        }

        public static ItemInventory GetInstance(BlockData.Blocks blockType)
        {
            return GetInstance((short)blockType);
        }

        public static ItemInventory GetInstance(BlockData.Items itemType)
        {
            return GetInstance((short)itemType);
        }

        public static ItemInventory GetInstance(short type)
        {
            ItemInventory item;
            Type itemClass;
            if (!_itemClasses.TryGetValue(type, out itemClass))
                return Void;
            item = (ItemInventory)itemClass.GetConstructor(Type.EmptyTypes).Invoke(null);
            return item;
        }

        public static ItemInventory GetInstance(PacketReader stream)
        {
            ItemInventory item = null;
            Type itemClass;
            short type = stream.ReadShort();
            sbyte count;
            short durability;
            if (type >= 0)
            {
                count = stream.ReadSByte();
                durability = stream.ReadShort();
                if (_itemClasses.TryGetValue(type, out itemClass))
                {
                    item = (ItemInventory)itemClass.GetConstructor(Type.EmptyTypes).Invoke(null);
                    item.Count = count;
                    item.Durability = durability;
                    // TODO: Implement extra data read (enchantment) and items

                    //if (durability > 0 || item.IsEnchantable)
                        stream.ReadShort();
                }
            }
            return item;
        }

        public static ItemInventory GetInstance(BigEndianStream stream)
        {
            ItemInventory item = Void;
            Type itemClass;
            short type = stream.ReadShort();
            if (type >= 0)
            {
                if (_itemClasses.TryGetValue(type, out itemClass))
                {
                    item = (ItemInventory) itemClass.GetConstructor(Type.EmptyTypes).Invoke(null);
                    item.Count = stream.ReadSByte();
                    item.Durability = stream.ReadShort();
                    // TODO: Implement extra data read (enchantment) and items
                    //if (item.Durability > 0 || item.IsEnchantable)
                        stream.ReadShort();
                }
            }
            return item;
        }

        public static ItemInventory Parse(string code)
        {
            string[] parts = code.Split(':', '#');
            string numeric = parts[0];
            string count = "1";
            string durability = "0";
            if (code.Contains(':'))
                durability = parts[1];
            if (code.Contains('#'))
                count = parts[parts.Length - 1];
            //TODO:Dennis make item classes and load them on server startup, before recipe loading
            short itemId;
            short.TryParse(numeric, out itemId);
            var item = GetInstance(itemId);
            if (IsVoid(item))
                return Void;
            item.Count = sbyte.Parse(count);
            item.Durability = durability == "*" ? (short) -1 : short.Parse(durability);
            return item;
        }
    }
}
