using Chraft.Entity.Items.Base;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.Entity.Items
{
    class ItemInkSack : ItemUsable
    {
        public ItemInkSack()
        {
            Type = (short)BlockData.Items.Ink_Sack;
            Name = "InkSack";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = true;
            MaxStackSize = 64;
        }

        public override void Use(IStructBlock baseBlock, BlockFace face)
        {
            var player = Owner.GetPlayer() as Player;
            var newBlockCoords = UniversalCoords.FromFace(baseBlock.Coords, face);

            if (Durability != 15)
                return;

            if (baseBlock.Type == (byte)BlockData.Blocks.Red_Mushroom || baseBlock.Type == (byte)BlockData.Blocks.Brown_Mushroom)
            {
                var baseMushroom = (BlockBaseMushroom)BlockHelper.Instance.CreateBlockInstance(baseBlock.Type);
                baseMushroom.Fertilize(player, (StructBlock)baseBlock);
            }

            base.Use(baseBlock, face);
        }
    }
}
