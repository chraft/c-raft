using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Interfaces.Containers
{
    /// <summary>
    /// TODO: Implement a method to fire the items from the dispenser
    /// </summary>
    class DispenserContainer : PersistentContainer
    {
        public DispenserContainer()
        {
            SlotsCount = 9;
        }
    }
}
