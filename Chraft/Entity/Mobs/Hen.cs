using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Hen : Animal
    {
        public override string Name
        {
            get { return "Chicken"; }
        }

        public override short MaxHealth
        {
            get
            {
                return 4; // 2 hearts;
            }
        }

        internal Hen(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Hen, data)
        {
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            sbyte count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
                Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), new Interfaces.ItemStack((short)Chraft.World.BlockData.Items.Feather, count, 0));
        }
    }
}
