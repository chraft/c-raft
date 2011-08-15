using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Zombie : Mob
    {
        public override string Name
        {
            get { return "Zombie"; }
        }

        public override short AttackStrength
        {
            get
            {
                // Easy 1
                // Medium 5
                // Hard 7
                return 5; // 2.5 hearts
            }
        }

        public override short MaxHealth
        {
            get
            {
                return 20; // 10 hearts
            }
        }

        internal Zombie(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Zombie, data)
        {
        }
    }
}
