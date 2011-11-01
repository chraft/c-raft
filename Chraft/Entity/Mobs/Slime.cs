using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Slime : Monster
    {
        public override string Name
        {
            get { return "Slime"; }
        }

        public override short MaxHealth { get { return 4; } } // Tiny - 1, Small - 4, Big - 16

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

        protected override void DoDeath(EntityBase killedBy)
        {
            sbyte count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
                Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), new Interfaces.ItemStack((short)Chraft.World.BlockData.Items.Slime_Ball, count, 0));
        }
    }
}
