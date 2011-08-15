using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity
{
    /// <summary>
    /// TODO: implement MobPack (used by MobSpawner)
    /// </summary>
    public class MobPack
    {
        public Chraft.World.PointI PackCentrePoint { get; private set; }
        public MobType Type { get; private set; }
    }
}
