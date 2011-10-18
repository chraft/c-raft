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
    class BlockStoneButton : BlockBase
    {
        public BlockStoneButton()
        {
            Name = "StoneButton";
            Type = BlockData.Blocks.Stone_Button;
            IsAir = true;
            LootTable.Add(new ItemStack((short)Type, 1));
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            LivingEntity living = (entity as LivingEntity);
            if (living == null)
                return;

            switch (face)
            {
                case BlockFace.Down:
                case BlockFace.Up:
                    return;
                case BlockFace.West: block.MetaData = (byte)MetaData.Button.WestWall;
                    break;
                case BlockFace.East: block.MetaData = (byte)MetaData.Button.EastWall;
                    break;
                case BlockFace.North: block.MetaData = (byte)MetaData.Button.NorthWall;
                    break;
                case BlockFace.South: block.MetaData = (byte)MetaData.Button.SouthWall;
                    break;
            }

            base.Place(entity, block, targetBlock, face);
        }
    }
}
