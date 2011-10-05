using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockTorch : BlockBase
    {
        public BlockTorch()
        {
            Name = "Torch";
            Type = BlockData.Blocks.Torch;
            IsAir = true;
            IsSingleHit = true;
            Luminance = 0xe;
            BurnEfficiency = 300;
            LootTable.Add(new ItemStack((short)Type, 1));
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            Player player = (entity as Player);
            if (player == null)
                return;

            switch (face)
            {
                case BlockFace.Down: return;
                case BlockFace.Up: block.MetaData = (byte)MetaData.Torch.Standing;
                    break;
                case BlockFace.West: block.MetaData = (byte)MetaData.Torch.West;
                    break;
                case BlockFace.East: block.MetaData = (byte)MetaData.Torch.East;
                    break;
                case BlockFace.North: block.MetaData = (byte)MetaData.Torch.North;
                    break;
                case BlockFace.South: block.MetaData = (byte)MetaData.Torch.South;
                    break;
            }
            base.Place(entity, block, targetBlock, face);
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if (targetBlock.Y > sourceBlock.Y && targetBlock.MetaData == (byte)MetaData.Torch.Standing ||
                targetBlock.X > sourceBlock.X && targetBlock.MetaData == (byte)MetaData.Torch.South ||
                targetBlock.X < sourceBlock.X && targetBlock.MetaData == (byte)MetaData.Torch.North ||
                targetBlock.Z > sourceBlock.Z && targetBlock.MetaData == (byte)MetaData.Torch.West ||
                targetBlock.Z < sourceBlock.Z && targetBlock.MetaData == (byte)MetaData.Torch.East)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }
    }
}
