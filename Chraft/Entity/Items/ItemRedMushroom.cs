using Chraft.Entity.Items.Base;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.Entity.Items
{
    class ItemRedMushroom : ItemPlaceable
    {
        public ItemRedMushroom()
        {
            Type = (short)BlockData.Blocks.Red_Mushroom;
            Name = "RedMushroom";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = true;
            MaxStackSize = 64;
        }
    }
}
