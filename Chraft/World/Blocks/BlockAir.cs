using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockAir : BlockBase
    {
        public BlockAir()
        {
            Name = "Air";
            Type = BlockData.Blocks.Air;
            IsAir = true;
            Opacity = 0x0;
        }

        public override void Destroy(EntityBase entity, StructBlock block) {}

        public override void Touch(EntityBase entity, StructBlock block, BlockFace face) { }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace targetSide) { }


    }
}
