using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class ZombiePigman : Mob
    {
        public override string Name
        {
            get { return "Zombie Pigman"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 5; // 2.5 hearts
            }
        }

        internal ZombiePigman(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.PigZombie, data)
        {
        }
    }
}
