using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemStone : ItemPlaceable
    {
        public ItemStone()
        {
            Type = (short)BlockData.Blocks.Stone;
            Name = "Stone";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = true;
            MaxStackSize = 64;
        }
    }
}
