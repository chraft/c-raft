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
