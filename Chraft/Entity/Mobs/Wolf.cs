using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Wolf : Mob
    {
        public override string Name
        {
            get { return "Wolf"; }
        }

        public override short AttackStrength
        {
            get
            {
                return (short)((this.Data.IsTamed) ? 4 : 2); // Wild 2, Tame 4;
            }
        }

        internal Wolf(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Wolf, data)
        {
            this.Data.IsSitting = false;
            this.Data.IsTamed = false;
            this.Data.IsAggressive = false;
        }

        protected override void DoDeath()
        {
        }
    }
}
