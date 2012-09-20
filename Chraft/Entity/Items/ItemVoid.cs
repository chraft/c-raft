using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity.Items.Base;

namespace Chraft.Entity.Items
{
    class ItemVoid : ItemInventory
    {
        public ItemVoid()
        {
            Type = -1;
            Durability = 0;
            Count = 0;
            Damage = 0;
            IsStackable = false;
            MaxStackSize = 0;
        }
    }
}
