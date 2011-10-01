using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public BlockData.Blocks DropBlock { get; set; }
        public sbyte DropBlockAmount { get; set; }
        public short DropBlockMeta { get; set; }

        public BlockData.Items DropItem { get; set; }
        public sbyte DropItemAmount { get; set; }
        public short DropItemMeta { get; set; }

        public BlockDestroyEventArgs(BlockBase block, EntityBase destroyedBy)
            : base(block)
        {
            DropBlock = block.DropBlock;
            DropBlockAmount = block.DropBlockAmount;
            DropBlockMeta = block.DropBlockMeta;
            DropItem = block.DropItem;
            DropItemAmount = block.DropItemAmount;
            DropItemMeta = block.DropItemMeta;
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
