using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Spider : Mob
    {
        public override string Name
        {
            get { return "Spider"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 2; // 1 hearts
            }
        }

        internal Spider(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Spider, data)
        {
        }
    }
}
