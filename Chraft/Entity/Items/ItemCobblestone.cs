using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemCobblestone : ItemPlaceable
    {
        public ItemCobblestone()
        {
            Type = (short)BlockData.Blocks.Cobblestone;
            Name = "CobbleStone";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = false;
            MaxStackSize = 64;
        }
    }
}
