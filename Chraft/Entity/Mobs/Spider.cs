using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Spider : Mob
    {
        public override string Name
        {
            get { return "Spider"; }
        }

        public override short MaxHealth { get { return 20; } }

        public override short AttackStrength
        {
            get
            {
                return 2; // 1 hearts
            }
        }

        internal Spider(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Spider, data)
        {
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            sbyte count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
                Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), new Interfaces.ItemStack((short)Chraft.World.BlockData.Items.Bow_String, count, 0));
        }
    }
}
