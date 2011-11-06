using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;

namespace Chraft.Interfaces.Containers
{
    class LargeChestContainer : PersistentContainer
    {
        protected string SecondDataFile;
        public UniversalCoords SecondCoords;

        public LargeChestContainer(UniversalCoords secondCoords)
        {
            SlotsCount = 54;
            SecondCoords = secondCoords;
            SecondDataFile = string.Format("x{0}y{1}z{2}.dat", SecondCoords.WorldX, SecondCoords.WorldY, SecondCoords.WorldZ);
        }

        protected override void DoLoad(int slotStart, int slotCount, string dataFile)
        {
            base.DoLoad(0, 27, DataFile);
            base.DoLoad(27, 54, SecondDataFile);
        }

        protected override void DoSave(int slotStart, int slotCount, string dataFile)
        {
            base.DoSave(0, 27, DataFile);
            base.DoSave(27, 54, SecondDataFile);
        }
    }
}
