using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;

namespace Chraft.World.Blocks.Interfaces
{
    interface IBlockInteractive
    {
        void Interact(EntityBase who, StructBlock block);
    }
}
