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
    class BlockRedstoneTorchOn : BlockBase
    {
        public BlockRedstoneTorchOn()
        {
            Name = "RedstoneTorchOn";
            Type = BlockData.Blocks.Redstone_Torch_On;
            IsAir = true;
            IsSingleHit = true;
            Luminance = 0x7;
            DropBlock = BlockData.Blocks.Redstone_Torch;
            DropBlockAmount = 1;
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            Client client = (entity as Client);
            if (client == null)
                return;

            switch (face)
            {
                case BlockFace.Down: return;
                case BlockFace.Up: block.MetaData = (byte)MetaData.Torch.Standing;
                    break;
                case BlockFace.West: block.MetaData = (byte)MetaData.Torch.West;
                    break;
                case BlockFace.East: block.MetaData = (byte)MetaData.Torch.East;
                    break;
                case BlockFace.North: block.MetaData = (byte)MetaData.Torch.North;
                    break;
                case BlockFace.South: block.MetaData = (byte)MetaData.Torch.South;
                    break;
            }

            base.Place(entity, block, targetBlock, face);
        }
    }
}
