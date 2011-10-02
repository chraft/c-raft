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
    class BlockLever : BlockBase, IBlockInteractive
    {
        public BlockLever()
        {
            Name = "Lever";
            Type = BlockData.Blocks.Lever;
            IsAir = true;
            LootTable.Add(new ItemStack((short)Type, 1));
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            Client client = (entity as Client);
            if (client == null)
                return;

            /*switch (face)
            {
                // Set metadata properly
            }*/

            base.Place(entity, block, targetBlock, face);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            // Switch the lever
        }
    }
}
