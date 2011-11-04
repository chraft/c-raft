using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockIce : BlockBase
    {
        public BlockIce()
        {
            Name = "Ice";
            Type = BlockData.Blocks.Ice;
            Opacity = 0x2;
            IsSolid = true;
        }

        protected override void UpdateOnDestroy(StructBlock block)
        {
            Chunk chunk = GetBlockChunk(block);

            if (chunk == null)
                return;

            chunk.SetBlockAndData(block.Coords, (byte)BlockData.Blocks.Still_Water, 0);
            chunk.RecalculateHeight(block.Coords);
            chunk.RecalculateSky(block.Coords.BlockX, block.Coords.BlockZ);
            chunk.SpreadSkyLightFromBlock((byte)(block.Coords.BlockX), (byte)block.Coords.BlockY, (byte)(block.Coords.BlockZ & 0xf));
            block.World.Update(block.Coords, false);
        }
    }
}
