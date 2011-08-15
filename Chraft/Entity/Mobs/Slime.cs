using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Slime : Mob
    {
        public override string Name
        {
            get { return "Slime"; }
        }

        public override short AttackStrength
        {
            get
            {
                // TODO: implement slime MetaData
                return 2; // Small 1 heart
                // 0; // Tiny 0 hearts
                // 4; // Big 2 hearts
            }
        }

        internal Slime(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Slime, data)
        {
        }
    }
}
