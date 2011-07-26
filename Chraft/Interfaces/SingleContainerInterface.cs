using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Chraft.Interfaces
{
    public class SingleContainerInterface : PersistentContainerInterface
    {
        protected int X { get; private set; }
        protected int Y { get; private set; }
        protected int Z { get; private set; }

        protected String DataFile { get { return Path.Combine(DataPath, String.Format("x{0}y{1}z{2}.dat", X, Y, Z)); } }

        public SingleContainerInterface(World.WorldManager world, int x, int y, int z, sbyte slotCount)
            : base(world, InterfaceType.Chest, slotCount)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;

            Load();
        }

        internal SingleContainerInterface(World.WorldManager world, InterfaceType interfaceType, int x, int y, int z, sbyte slotCount)
            : base(world, interfaceType, slotCount)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;

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
