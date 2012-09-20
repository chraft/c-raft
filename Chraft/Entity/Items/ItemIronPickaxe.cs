using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemIronPickaxe : ItemInventory
    {
        public ItemIronPickaxe()
        {
            Type = (short)BlockData.Items.Iron_Pickaxe;
            Name = "IronPickaxe";
            Durability = 251;
            Damage = 1;
            Count = 1;
            IsStackable = false;
            MaxStackSize = 1;
        }
    }
}
