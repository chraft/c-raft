using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;

namespace Chraft.Entity.Items
{
    class ItemGrilledPork : ItemConsumable
    {
        public ItemGrilledPork()
        {
            Type = (short)BlockData.Items.Grilled_Pork;
            Name = "GrilledPork";
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
