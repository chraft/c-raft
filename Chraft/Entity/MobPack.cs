using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Entity
{
    /// <summary>
    /// TODO: implement MobPack (used by MobSpawner)
    /// </summary>
    public class MobPack
    {
        public UniversalCoords PackCentrePoint { get; private set; }
        public MobType Type { get; private set; }
    }
}
