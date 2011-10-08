using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Cow: Mob
    {
        public override string Name
        {
            get { return "Cow"; }
        }

        internal Cow(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Cow, data)
        {
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            sbyte count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
                Server.DropItem(World, UniversalCoords.FromWorld(Position.X, Position.Y, Position.Z), new Interfaces.ItemStack((short)BlockData.Items.Leather, count, 0));
        }
    }
}
