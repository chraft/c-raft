using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockDiamondBlock : BlockBase
    {
        public BlockDiamondBlock()
        {
            Name = "DiamondBlock";
            Type = BlockData.Blocks.Diamond_Block;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Diamond_Block;
            DropBlockAmount = 1;
        }
    }
}
