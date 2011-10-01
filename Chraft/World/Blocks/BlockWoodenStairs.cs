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
    class BlockWoodenStairs : BlockBase
    {
        public BlockWoodenStairs()
        {
            Name = "WoodenStairs";
            Type = BlockData.Blocks.Wooden_Stairs;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Wood;
            DropBlockAmount = 1;
            BurnEfficiency = 300;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            Client client = entity as Client;
            if (client == null)
                return;

            // TODO: Bugged - should depend on the player's Yaw/Pitch
            switch (client.FacingDirection(4))
            {
                case "N":
                    block.MetaData = (byte)MetaData.Stairs.South;
                    break;
                case "E":
                    block.MetaData = (byte)MetaData.Stairs.West;
                    break;
                case "S":
                    block.MetaData = (byte)MetaData.Stairs.North;
                    break;
                case "W":
                    block.MetaData = (byte)MetaData.Stairs.East;
                    break;
                default:
                    return;
            }
            base.Place(entity, block, targetBlock, face);
        }
    }
}
