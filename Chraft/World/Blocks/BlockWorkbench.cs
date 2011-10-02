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
    class BlockWorkbench : BlockBase, IBlockInteractive
    {
        public BlockWorkbench()
        {
            Name = "Workbench";
            Type = BlockData.Blocks.Workbench;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)Type, 1));
            BurnEfficiency = 300;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            Client client = (entity as Client);
            if (client == null)
                return;

            switch (face) //Bugged, as the client has a mind of its own for facing
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
            base.Place(entity, block, targetBlock, face);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            Client client = entity as Client;
            if (client == null)
                return;
            if (client.CurrentInterface != null)
                return;
            client.CurrentInterface = new WorkbenchInterface();
            client.CurrentInterface.Associate(client);
            ((WorkbenchInterface)client.CurrentInterface).Open(block.X, block.Y, block.Z);
        }
    }
}
