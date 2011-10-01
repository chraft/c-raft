using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockNetherrack : BlockBase
    {
        public BlockNetherrack()
        {
            Name = "Netherrack";
            Type = BlockData.Blocks.Netherrack;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Netherrack;
            DropBlockAmount = 1;
        }
    }
}
