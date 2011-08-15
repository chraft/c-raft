using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Ghast : Mob
    {
        public override string Name
        {
            get { return "Ghast"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 7; // 3.5 (varies by proximity)
            }
        }

        internal Ghast(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Ghast, data)
        {
        }
    }
}
