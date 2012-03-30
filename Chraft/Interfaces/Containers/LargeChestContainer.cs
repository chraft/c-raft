#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
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
