using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockIronDoor : BlockBaseDoor
    {
        public BlockIronDoor()
        {
            Name = "IronDoor";
            Type = BlockData.Blocks.Iron_Door;
        }
    }
}
