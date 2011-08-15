using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;

namespace Chraft.Entity.Mobs
{
    public class Cow: Mob
    {
        public override string Name
        {
            get { return "Cow"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 0;
            }
        }

        internal Cow(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Cow, data)
        {
        }
    }
}
