using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Hen : Mob
    {
        public override string Name
        {
            get { return "Chicken"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 0;
            }
        }

        public override short MaxHealth
        {
            get
            {
                return 4; // 2 hearts;
            }
        }

        internal Hen(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Hen, data)
        {
        }
    }
}
