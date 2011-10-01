using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockBookshelf : BlockBase
    {
        public BlockBookshelf()
        {
            Name = "Bookshelf";
            Type = BlockData.Blocks.Bookshelf;
            IsSolid = true;
            BurnEfficiency = 300;
        }
    }
}
