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
    class BlockLadder : BlockBase
    {
        public BlockLadder()
        {
            Name = "Ladder";
            Type = BlockData.Blocks.Ladder;
            IsAir = true;
            LootTable.Add(new ItemStack((short)Type, 1));
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            switch (face)
            {
                case BlockFace.East:
                    block.MetaData = (byte)MetaData.Ladders.East;
                    break;
                case BlockFace.West:
                    block.MetaData = (byte)MetaData.Ladders.West;
                    break;
                case BlockFace.North:
                    block.MetaData = (byte)MetaData.Ladders.North;
                    break;
                case BlockFace.South:
                    block.MetaData = (byte)MetaData.Ladders.South;
                    break;
                default:
                    return;
            }
            base.Place(entity, block, targetBlock, face);
        }
    }
}
