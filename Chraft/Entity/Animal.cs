using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;

namespace Chraft.Entity
{
    public abstract class Animal: Mob
    {
        protected Animal(WorldManager world, int entityId, MobType type, MetaData data)
            : base(world, entityId, type, data)
        {

        }

        protected override double BlockPathWeight(UniversalCoords coords)
        {
            if (this.World.GetBlockId(coords.WorldX, coords.WorldY - 1, coords.WorldZ) == (byte)BlockData.Blocks.Grass)
            {
                return 10.0;
            }
            else
            {
                return this.World.GetBlockLightBrightness(coords) - 0.5; // stay out of lower half of brightness spectrum
            }
        }

        public override bool CanSpawnHere()
        {
            return World.GetBlockId(this.BlockPosition.WorldX, this.BlockPosition.WorldY - 1, this.BlockPosition.WorldZ) == (byte)BlockData.Blocks.Grass && World.GetFullBlockLight(this.BlockPosition) > 8 && base.CanSpawnHere();
        }
    }
}
