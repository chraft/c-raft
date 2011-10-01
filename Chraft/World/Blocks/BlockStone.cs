﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockStone : BlockBase
    {
        public BlockStone()
        {
            Name = "Stone";
            Type = BlockData.Blocks.Stone;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Cobblestone;
            DropBlockAmount = 1;
        }
    }
}
