using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockWoodenDoor : BlockBaseDoor
    {
        public BlockWoodenDoor()
        {
            Name = "WoodenDoor";
            Type = BlockData.Blocks.Wooden_Door;
        }
    }
}
