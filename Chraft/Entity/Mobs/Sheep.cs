using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Sheep: Mob
    {
        public override string Name
        {
            get { return "Sheep"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 0;
            }
        }

        internal Sheep(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Sheep, data)
        {
        }
    }
}
