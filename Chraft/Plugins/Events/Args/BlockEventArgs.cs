using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces;
using Chraft.World;
using Chraft.World.Blocks;
using Chraft.Entity;

namespace Chraft.Plugins.Events.Args
{
    /// <summary>
    /// The base EventArgs for an Block Event.
    /// </summary>
    public class BlockEventArgs : ChraftEventArgs
    {
        public BlockBase Block { get; private set; }
        public BlockEventArgs(BlockBase block)
        {
            Block = block;
        }
    }

    public class BlockPlaceEventArgs : BlockEventArgs
    {
        public EntityBase PlacedBy { get; set; }
        public BlockPlaceEventArgs(BlockBase block, EntityBase placedBy)
            : base(block)
        {
            PlacedBy = placedBy;
        }
    }
    public class BlockDestroyEventArgs : BlockEventArgs
    {
        public EntityBase DestroyedBy{ get; set; }
        public List<ItemStack> LootTable { get; set; }

        public BlockDestroyEventArgs(BlockBase block, EntityBase destroyedBy)
            : base(block)
        {
            LootTable = block.LootTable;
            DestroyedBy = destroyedBy;
        }
    }

    public class BlockTouchEventArgs : BlockEventArgs
    {
        public EntityBase TouchedBy { get; set; }

        public BlockTouchEventArgs(BlockBase block, EntityBase touchedBy)
            : base(block)
        {
            TouchedBy = touchedBy;
        }
    }
}
