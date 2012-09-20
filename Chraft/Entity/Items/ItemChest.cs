using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemChest : ItemPlaceable
    {
        public ItemChest()
        {
            Type = (short)BlockData.Blocks.Chest;
            Name = "Chest";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = false;
            MaxStackSize = 64;
        }
    }
}
