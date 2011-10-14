using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class ZombiePigman : Mob
    {
        public override string Name
        {
            get { return "Zombie Pigman"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 5; // 2.5 hearts
            }
        }

        internal ZombiePigman(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.PigZombie, data)
        {
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            sbyte count = (sbyte)Server.Rand.Next(3);
            if (count > 0)
                Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), new Interfaces.ItemStack((short)Chraft.World.BlockData.Items.Grilled_Pork, count, 0));
        }
    }
}
