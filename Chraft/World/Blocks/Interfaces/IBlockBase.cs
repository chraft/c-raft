using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;

namespace Chraft.World.Blocks.Interfaces
{
    interface IBlockBase
    {

        BlockData.Blocks Type { get; set; }

        /// <summary>
        /// Destroy the block and drop the loot (if any)
        /// </summary>
        void Destroy(StructBlock block);

        /// <summary>
        /// Destroy the block and drop the loot (if any)
        /// </summary>
        void Destroy(EntityBase who, StructBlock block);

        /// <summary>
        /// Block was touched by someone
        /// </summary>
        void Touch(EntityBase who, StructBlock block);

        /// <summary>
        /// Place the block
        /// </summary>
        void Place(StructBlock block, StructBlock targetBlock, BlockFace face);

        /// <summary>
        /// Place the block
        /// </summary>
        void Place(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace face);

    }
}
