using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockCobweb : BlockBase
    {
        public BlockCobweb()
        {
            Name = "Cobweb";
            Type = BlockData.Blocks.Cobweb;
            Opacity = 0x0;
            LootTable.Add(new ItemStack((short)BlockData.Items.Bow_String, 1));
        }
    }
}
