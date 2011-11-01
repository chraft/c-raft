using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Skeleton : Monster
    {
        public override string Name
        {
            get { return "Skeleton"; }
        }

        public override short MaxHealth { get { return 20; } }

        public override short AttackStrength
        {
            get
            {
                return 4; // 2 hearts
            }
        }

        internal Skeleton(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Skeleton, data)
        {
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            UniversalCoords coords = UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z);
            sbyte count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
                Server.DropItem(World, coords, new Interfaces.ItemStack((short)Chraft.World.BlockData.Items.Arrow, count, 0));
            count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
                Server.DropItem(World, coords, new Interfaces.ItemStack((short)Chraft.World.BlockData.Items.Bone, count, 0));
        }
    }
}
