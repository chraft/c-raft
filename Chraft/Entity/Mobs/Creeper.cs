using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Creeper : Monster
    {
        public override string Name
        {
            get { return "Creeper"; }
        }

        public override short MaxHealth { get { return 20; } }

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

        protected override void DoDeath(EntityBase killedBy)
        {
            var killedByMob = killedBy as Mob;
            UniversalCoords coords = UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z);
            if (killedByMob != null && killedByMob.Type == MobType.Skeleton)
            {
                // If killed by a skeleton drop a music disc
                sbyte count = 1;
                short item;
                if (Server.Rand.Next(2) > 1)
                {
                    item = (short)BlockData.Items.Disc13;
                }
                else
                {
                    item = (short)BlockData.Items.DiscCat;
                }
                Server.DropItem(World, coords, new Interfaces.ItemStack(item, count, 0));
            }
            else
            {
                sbyte count = (sbyte)Server.Rand.Next(2);
                if (count > 0)
                    Server.DropItem(World, coords, new Interfaces.ItemStack((short)BlockData.Items.Gunpowder, count, 0));
            }
            base.DoDeath(killedBy);
        }
    }
}
