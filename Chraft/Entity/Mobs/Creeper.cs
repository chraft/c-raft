using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

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

        protected override void DoDeath()
        {
            sbyte count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
                Server.DropItem(World, (int)this.Position.X, (int)this.Position.Y, (int)this.Position.Z, new Interfaces.ItemStack((short)BlockData.Items.Gunpowder, count, 0));
            
            // TODO: if killed by a skeleton drop a music disc - currently we do not record non-Client related kills
        }
    }
}
