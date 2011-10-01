using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockTallGrass : BlockBase
    {
        public BlockTallGrass()
        {
            Name = "TallGrass";
            Type = BlockData.Blocks.TallGrass;
            IsAir = true;
            IsSolid = true;
            DropBlock = BlockData.Blocks.TallGrass;
            DropBlockAmount = 1;
            Opacity = 0x0;
        }

    }
}
