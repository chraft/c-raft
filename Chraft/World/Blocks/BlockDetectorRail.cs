using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockDetectorRail : BlockBase
    {
        public BlockDetectorRail()
        {
            Name = "DetectorRail";
            Type = BlockData.Blocks.DetectorRail;
            IsAir = true;
            Opacity = 0x0;
            LootTable.Add(new ItemStack((short)Type, 1));
        }
    }
}
