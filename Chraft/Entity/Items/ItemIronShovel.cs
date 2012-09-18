using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemIronShovel : ItemInventory
    {
        public ItemIronShovel()
        {
            Type = (short)BlockData.Items.Iron_Spade;
            Name = "IronShovel";
            Durability = 251;
            Damage = 1;
            Count = 1;
            IsStackable = false;
            MaxStackSize = 1;
        }
    }
}
