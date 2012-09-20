using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemDiamondPickaxe : ItemInventory
    {
        public ItemDiamondPickaxe()
        {
            Type = (short)BlockData.Items.Diamond_Pickaxe;
            Name = "DiamondPickaxe";
            Durability = 1562;
            Damage = 1;
            Count = 1;
            IsStackable = false;
            MaxStackSize = 1;
        }
    }
}
