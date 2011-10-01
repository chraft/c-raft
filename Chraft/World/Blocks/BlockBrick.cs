using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockBrick : BlockBase
    {
        public BlockBrick()
        {
            Name = "Brick";
            Type = BlockData.Blocks.Brick;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Brick;
            DropBlockAmount = 1;
        }
    }
}
