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
    class BlockCobblestoneStairs : BlockBase
    {
        public BlockCobblestoneStairs()
        {
            Name = "CobblestoneStairs";
            Type = BlockData.Blocks.Cobblestone_Stairs;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Cobblestone_Stairs;
            DropBlockAmount = 1;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
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
            base.Place(entity, block, targetBlock, targetSide);
        }
    }
}
