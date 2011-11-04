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
using System.Linq;
using System.Text;
using System.IO;
using Chraft.World;

namespace Chraft.Interfaces
{
    public class SingleContainerInterface : PersistentContainerInterface
    {
        protected UniversalCoords Coords { get; private set; }

        protected String DataFile { get { return Path.Combine(DataPath, String.Format("x{0}y{1}z{2}.dat", Coords.WorldX, Coords.WorldY, Coords.WorldZ)); } }

        public SingleContainerInterface(World.WorldManager world, UniversalCoords coords, sbyte slotCount)
            : base(world, InterfaceType.Chest, slotCount)
        {
            Coords = coords;

            Load();
        }

        internal SingleContainerInterface(World.WorldManager world, InterfaceType interfaceType, UniversalCoords coords, sbyte slotCount)
            : base(world, interfaceType, slotCount)
        {
            Coords = coords;

            Load();
        }

        protected override bool CanLoad()
        {
            return base.CanLoad() && File.Exists(DataFile);
        }

        protected override void DoLoad()
        {
            DoLoadFromFile(Slots, DataFile);
        }

        protected override void DoSave()
        {
            DoSaveToFile(Slots, DataFile);
        }
    }
}
