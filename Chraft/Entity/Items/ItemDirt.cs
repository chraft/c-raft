using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemDirt : ItemPlaceable
    {
        public ItemDirt()
        {
            Type = (short)BlockData.Blocks.Dirt;
            Name = "Dirt";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = true;
            MaxStackSize = 64;
        }
    }
}
