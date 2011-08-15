using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Pig : Mob
    {
        public override string Name
        {
            get { return "Pig"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 0;
            }
        }

        internal Pig(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Pig, data)
        {
        }
    }
}
