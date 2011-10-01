using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Net;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockFurnace : BlockBase, IBlockInteractive
    {
        public BlockFurnace()
        {
            Name = "Furnace";
            Type = BlockData.Blocks.Furnace;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Furnace;
            DropBlockAmount = 1;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            Client client = (entity as Client);
            if (client == null)
                return;

            switch (targetSide) //Bugged, as the client has a mind of its own for facing
            {
                case BlockFace.East:
                    block.MetaData = (byte)MetaData.Furnace.East;
                    break;
                case BlockFace.West:
                    block.MetaData = (byte)MetaData.Furnace.West;
                    break;
                case BlockFace.North:
                    block.MetaData = (byte)MetaData.Furnace.North;
                    break;
                case BlockFace.South:
                    block.MetaData = (byte)MetaData.Furnace.South;
                    break;
                default:
                    switch (client.FacingDirection(4)) // Built on floor, set by facing dir
                    {
                        case "N":
                            block.MetaData = (byte)MetaData.Furnace.North;
                            break;
                        case "W":
                            block.MetaData = (byte)MetaData.Furnace.West;
                            break;
                        case "S":
                            block.MetaData = (byte)MetaData.Furnace.South;
                            break;
                        case "E":
                            block.MetaData = (byte)MetaData.Furnace.East;
                            break;
                        default:
                            return;

                    }
                    break;
            }
            base.Place(entity, block, targetBlock, targetSide);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
        }

    }
}
