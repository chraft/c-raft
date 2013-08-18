using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World.Blocks.Base;
using Chraft.Entity.Items;
using Chraft.Utilities.Blocks;

namespace Chraft.World.Blocks
{
    class BlockCoal : BlockBase
    {
        public BlockCoal()
        {
            Name = "Block of Coal";
            Type = BlockData.Blocks.Block_Of_Coal;
            IsSolid = true;
            var item = ItemHelper.GetInstance((short)Type);
            item.Count = 1;
            LootTable.Add(item);
        }
    }
}
