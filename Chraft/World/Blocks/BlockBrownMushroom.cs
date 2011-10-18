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
    class BlockBrownMushroom : BlockBase
    {
        public BlockBrownMushroom()
        {
            Name = "BrownMushroom";
            Type = BlockData.Blocks.Brown_Mushroom;
            IsAir = true;
            IsSingleHit = true;
            LootTable.Add(new ItemStack((short)Type, 1));
            Opacity = 0x0;
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }

        public void Fertilize(EntityBase entity, StructBlock block)
        {
            byte blockBelow = block.World.GetBlockId(block.Coords.WorldX, block.Coords.WorldY - 1,
                                                                   block.Coords.WorldZ);
            if (blockBelow != (byte)BlockData.Blocks.Dirt && blockBelow != (byte)BlockData.Blocks.Grass && blockBelow != (byte)BlockData.Blocks.Mycelium)
                return;

            int stemHeight = block.World.Server.Rand.Next(3) + 4;
            int capY = block.Coords.WorldY + stemHeight + 1;
            if (capY > 127)
                return;

            for (int dY = block.Coords.WorldY + 1; dY < capY - 1; dY++ )
                if (block.World.GetBlockId(block.Coords.WorldX, dY, block.Coords.WorldZ) != (byte)BlockData.Blocks.Air &&
                    block.World.GetBlockId(block.Coords.WorldX, dY, block.Coords.WorldZ) != (byte)BlockData.Blocks.Leaves)
                    return;

            int absdX, absdZ;
            byte blockId;
            for (int dX = -3; dX < 4; dX++)
                for (int dZ = -3; dZ < 4; dZ++)
                {
                    absdX = Math.Abs(dX);
                    absdZ = Math.Abs(dZ);
                    if (absdX == 3 && absdZ == 3)
                        continue;
                    blockId = block.World.GetBlockId(block.Coords.WorldX + dX, capY, block.Coords.WorldZ + dZ);
                    if (blockId != (byte)BlockData.Blocks.Air && blockId != (byte)BlockData.Blocks.Leaves)
                        return;
                }
            

            byte metaData = (byte)MetaData.HugeMushroom.NorthWeastSouthEast;
            for (int dY = block.Coords.WorldY; dY < capY; dY++)
                if (block.World.GetBlockId(block.Coords.WorldX, dY, block.Coords.WorldZ) != (byte)BlockData.Blocks.Leaves)
                    block.World.SetBlockAndData(block.Coords.WorldX, dY, block.Coords.WorldZ, (byte)BlockData.Blocks.BrownMushroomCap, metaData);

            for (int dX = -3; dX < 4; dX++)
                for (int dZ = -3; dZ < 4; dZ++)
                {
                    absdX = Math.Abs(dX);
                    absdZ = Math.Abs(dZ);
                    if (absdX == 3 && absdZ == 3)
                        continue;
                    blockId = block.World.GetBlockId(block.Coords.WorldX + dX, capY, block.Coords.WorldZ + dZ);
                    if (blockId == (byte)BlockData.Blocks.Leaves)
                        continue;

                    if (absdX < 3 && absdZ < 3)
                        metaData = (byte)MetaData.HugeMushroom.Top;
                    else if ((dX == -3 && dZ == -2) || (dZ == -3 && dX == -2))
                        metaData = (byte)MetaData.HugeMushroom.TopNorthWest;
                    else if ((dX == -3 && dZ == 2) || (dZ == 3 && dX == -2))
                        metaData = (byte)MetaData.HugeMushroom.TopSouthWest;
                    else if ((dX == 3 && dZ == -2) || (dZ == -3 && dX == 2))
                        metaData = (byte)MetaData.HugeMushroom.TopNorthEast;
                    else if ((dX == 3 && dZ == 2) || (dZ == 3 && dX == 2))
                        metaData = (byte)MetaData.HugeMushroom.TopSouthEast;
                    else if (dX == -3 && absdZ < 2)
                        metaData = (byte)MetaData.HugeMushroom.TopWest;
                    else if (dX == 3 && absdZ < 2)
                        metaData = (byte)MetaData.HugeMushroom.TopEast;
                    else if (dZ == -3 && absdX < 2)
                        metaData = (byte)MetaData.HugeMushroom.TopNorth;
                    else if (dZ == 3 && absdX < 2)
                        metaData = (byte)MetaData.HugeMushroom.TopSouth;

                    block.World.SetBlockAndData(block.Coords.WorldX + dX, capY, block.Coords.WorldZ + dZ, (byte)BlockData.Blocks.BrownMushroomCap, metaData);
                }
        }
    }
}
