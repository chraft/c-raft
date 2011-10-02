using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Net;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockMobSpawner : BlockBase
    {
        public BlockMobSpawner()
        {
            Name = "MobSpawner";
            Type = BlockData.Blocks.Mob_Spawner;
            Opacity = 0x0;
            IsSolid = true;
        }
    }
}
