using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockRedRose : BlockBase
    {
        public BlockRedRose()
        {
            Name = "RedRose";
            Type = BlockData.Blocks.Red_Rose;
            IsAir = true;
            DropBlock = BlockData.Blocks.Red_Rose;
            DropBlockAmount = 1;
            Opacity = 0x0;
        }
    }
}
