using Chraft.Entity.Items.Base;
using Chraft.PluginSystem.Item;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Math;

namespace Chraft.Entity.Items
{
    class ItemPort : ItemConsumable
    {
        public ItemPort()
        {
            Type = (short)BlockData.Items.Pork;
            Name = "Pork";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = true;
            MaxStackSize = 64;
        }

        protected override void OnConsumed()
        {
            base.OnConsumed();
        }
    }
}
