using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Squid : Mob
    {
        public override string Name
        {
            get { return "Squid"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 0;
            }
        }

        internal Squid(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Squid, data)
        {
        }
    }
}
