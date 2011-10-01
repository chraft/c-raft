using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Net;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockBurningFurnace : BlockBase
    {
        public BlockBurningFurnace()
        {
            Name = "BurningFurnace";
            Type = BlockData.Blocks.Burning_Furnace;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Furnace;
            DropBlockAmount = 1;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            // You can not place the furnace that is already burning.
        }

    }
}
