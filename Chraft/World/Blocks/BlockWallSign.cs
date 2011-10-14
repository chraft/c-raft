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
    class BlockWallSign : BlockSignBase
    {
        public BlockWallSign()
        {
            Name = "WallSign";
            Type = BlockData.Blocks.Wall_Sign;
            IsAir = true;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)BlockData.Items.Sign, 1));
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            switch (face)
            {
                case BlockFace.West:
                    block.MetaData = (byte)MetaData.SignWall.West;
                    break;
                case BlockFace.East:
                    block.MetaData = (byte)MetaData.SignWall.East;
                    break;
                case BlockFace.North:
                    block.MetaData = (byte)MetaData.SignWall.North;
                    break;
                case BlockFace.South:
                    block.MetaData = (byte)MetaData.SignWall.South;
                    break;
                default:
                    return;
            }
            base.Place(entity, block, targetBlock, face);
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if (targetBlock.Coords.WorldY > sourceBlock.Coords.WorldY)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }
    }
}
