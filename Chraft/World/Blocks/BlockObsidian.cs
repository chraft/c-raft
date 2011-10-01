using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockObsidian : BlockBase
    {
        public BlockObsidian()
        {
            Name = "Obsidian";
            Type = BlockData.Blocks.Obsidian;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Obsidian;
            DropBlockAmount = 1;
        }
    }
}
