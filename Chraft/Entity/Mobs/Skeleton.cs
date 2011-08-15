using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Skeleton : Mob
    {
        public override string Name
        {
            get { return "Skeleton"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 4; // 2 hearts
            }
        }

        internal Skeleton(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Skeleton, data)
        {
        }
    }
}
