using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.World.Blocks.Interfaces
{
    interface IBlockGrowable
    {
        void Grow(StructBlock block);
        bool CanGrow(StructBlock block);
    }
}
