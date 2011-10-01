using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockNoteBlock : BlockBase
    {
        public BlockNoteBlock()
        {
            Name = "NoteBlock";
            Type = BlockData.Blocks.Note_Block;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Note_Block;
            DropBlockAmount = 1;
            BurnEfficiency = 300;
        }
    }
}
