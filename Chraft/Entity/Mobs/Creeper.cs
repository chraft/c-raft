using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Creeper : Mob
    {
        public override string Name
        {
            get { return "Creeper"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 20; // 10 hearts (double when charged) varies based on proximity to blast radius
            }
        }

        internal Creeper(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Creeper, data)
        {
        }
    }
}
