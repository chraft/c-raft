using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Interfaces.Containers
{
    class SmallChestContainer : PersistentContainer
    {
        public SmallChestContainer()
        {
            SlotsCount = 27;
        }
    }
}
