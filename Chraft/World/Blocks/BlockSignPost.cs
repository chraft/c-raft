using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Net.Packets;


namespace Chraft.World.Blocks
{
    class BlockSignPost : BlockSignBase
    {
        public BlockSignPost()
        {
            Name = "SignPost";
            Type = BlockData.Blocks.Sign_Post;
            IsAir = true;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)BlockData.Items.Sign, 1));
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            LivingEntity living = (entity as LivingEntity);
            if (living == null)
                return;
            switch (living.FacingDirection(8))
            {
                case "N":
                    block.MetaData = (byte)MetaData.SignPost.North;
                    break;
                case "NE":
                    block.MetaData = (byte)MetaData.SignPost.Northeast;
                    break;
                case "E":
                    block.MetaData = (byte)MetaData.SignPost.East;
                    break;
                case "SE":
                    block.MetaData = (byte)MetaData.SignPost.Southeast;
                    break;
                case "S":
                    block.MetaData = (byte)MetaData.SignPost.South;
                    break;
                case "SW":
                    block.MetaData = (byte)MetaData.SignPost.Southwest;
                    break;
                case "W":
                    block.MetaData = (byte)MetaData.SignPost.West;
                    break;
                case "NW":
                    block.MetaData = (byte)MetaData.SignPost.Northwest;
                    break;
                default:
                    return;
            }
            base.Place(entity, block, targetBlock, face);
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }
    }
}
