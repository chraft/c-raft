using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemMycelium : ItemPlaceable
    {
        public ItemMycelium()
        {
            Type = (short)BlockData.Blocks.Mycelium;
            Name = "Mycelium";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = true;
            MaxStackSize = 64;
        }
    }
}
