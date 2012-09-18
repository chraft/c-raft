using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemStone : ItemInventory
    {
        public ItemStone()
        {
            Type = (short)BlockData.Blocks.Stone;
            Name = "Stone";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = false;
            MaxStackSize = 1;
        }
    }
}
