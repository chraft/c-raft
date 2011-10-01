using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockSoulSand : BlockBase
    {
        public BlockSoulSand()
        {
            Name = "SoulSand";
            Type = BlockData.Blocks.Soul_Sand;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Soul_Sand;
            DropBlockAmount = 1;
        }
    }
}
