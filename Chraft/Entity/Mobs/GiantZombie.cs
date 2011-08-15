using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    class GiantZombie: Mob
    {
        public override string Name
        {
            get
            {
                return "Giant Zombie";
            }
        }

        public override short AttackStrength
        {
            get
            {
                return 17; // 8.5 
            }
        }

        public override short MaxHealth
        {
            get
            {
                return 200; // 100 hearts;
            }
        }

        internal GiantZombie(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Giant, data)
        {
        }
    }
}
