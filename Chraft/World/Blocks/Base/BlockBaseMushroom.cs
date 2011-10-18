using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    public abstract class BlockBaseMushroom : BlockBase
    {
        protected BlockBaseMushroom()
        {
            Name = "BaseMushroom";
            IsAir = true;
            IsSingleHit = true;
            Opacity = 0x0;
            BlockBoundsOffset = new BoundingBox(0.3, 0, 0.3, 0.7, 0.4, 0.7);
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }

        public virtual void Fertilize(EntityBase entity, StructBlock block)
        { }
    }
}