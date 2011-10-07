using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockCake : BlockBase, IBlockInteractive
    {
        public BlockCake()
        {
            Name = "Cake";
            Type = BlockData.Blocks.Cake;
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            // Eat the cake. No food restoration at the moment.

            // Restore hp/food

            if (block.MetaData == (byte)MetaData.Cake.OneLeft)
            {
                // Cake is dead.
                Destroy(entity, block);
            } else
            {
                // Eat one piece
                block.World.SetBlockData(block.Coords, block.MetaData++);
            }
        }
    }
}
