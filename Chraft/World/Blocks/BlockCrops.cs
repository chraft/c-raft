using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockCrops : BlockBase, IBlockGrowable
    {
        public BlockCrops()
        {
            Name = "Crops";
            Type = BlockData.Blocks.Crops;
            IsAir = true;
            IsSingleHit = true;
            DropItem = BlockData.Items.Seeds;
            DropItemAmount = 0;
            Opacity = 0x0;
        }

        protected override void DropItems(EntityBase who, StructBlock block)
        {
            // TODO: Fully grown drops 1 Wheat & 0-3 Seeds. 0 seeds - very rarely
            if (block.MetaData < 5)
                DropItemAmount = 0;
            else
                DropItemAmount = (sbyte)(1 + block.World.Server.Rand.Next(2));
            base.DropItems(who, block);
        }

        public void Grow(StructBlock block)
        {
            // Crops grow from 0x0 to 0x7
            if (block.MetaData == 0x07)
                return;
            // TODO: Check if the water within 4 blocks on the same horizontal level and grow faster?
            if (block.World.Server.Rand.Next(10) == 0)
            {
                block.World.SetBlockData(block.X, block.Y, block.Z, block.MetaData++);
            }
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            if (!targetBlock.World.BlockHelper.Instance(targetBlock.Type).IsPlowed || targetSide != BlockFace.Up)
                return false;
            return base.CanBePlacedOn(who, targetBlock, targetBlock, targetSide);
        }
    }
}
