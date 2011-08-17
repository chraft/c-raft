using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Squid : Mob
    {
        public override string Name
        {
            get { return "Squid"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 0;
            }
        }

        internal Squid(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Squid, data)
        {
        }

        protected override void DoDeath()
        {
            sbyte count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
                Server.DropItem(World, (int)this.Position.X, (int)this.Position.Y, (int)this.Position.Z, new Interfaces.ItemStack((short)Chraft.World.BlockData.Items.Ink_Sack, count, 0));
        }
    }
}
