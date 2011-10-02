using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockBedrock : BlockBase
    {
        public BlockBedrock()
        {
            Name = "Bedrock";
            Type = BlockData.Blocks.Bedrock;
            IsSolid = true;
        }

        public override void Destroy(EntityBase entity, StructBlock block) {}
    }
}
