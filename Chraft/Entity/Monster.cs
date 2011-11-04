using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;

namespace Chraft.Entity
{
    public abstract class Monster : Mob
    {
        protected Monster(WorldManager world, int entityId, MobType type, MetaData data)
            : base(world, entityId, type, data)
        {

        }

        protected override double BlockPathWeight(UniversalCoords coords)
        {
            return 0.5 - World.GetBlockLightBrightness(coords); // // stay in lower half of brightness spectrum
        }
        public override bool CanSpawnHere()
        {
            if (World.GetSkyLight(this.BlockPosition) > World.Server.Rand.Next(32))
            {
                return false;
            }

            byte? light = this.World.GetEffectiveLight(this.BlockPosition);

            if (light == null)
                return false;
            // TODO: if world Thundering adjust light value

            return light <= World.Server.Rand.Next(8) && base.CanSpawnHere();
        }
    }
}
