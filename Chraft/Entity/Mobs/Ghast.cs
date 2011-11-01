using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Ghast : Monster
    {
        public override string Name
        {
            get { return "Ghast"; }
        }

        public override short MaxHealth { get { return 10; } }

        public override short AttackStrength
        {
            get
            {
                return 7; // 3.5 (varies by proximity)
            }
        }

        public override int SightRange
        {
            get
            {
                return 100;
            }
        }

        public override int MaxSpawnedPerGroup
        {
            get
            {
                return 1;
            }
        }

        internal Ghast(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Ghast, data)
        {
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            sbyte count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
                Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), new Interfaces.ItemStack((short)Chraft.World.BlockData.Items.Gunpowder, count, 0));
        }
    }
}
