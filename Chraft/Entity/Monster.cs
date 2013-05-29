#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.PluginSystem.Server;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;
using Chraft.World;
using Chraft.PluginSystem;

namespace Chraft.Entity
{
    public abstract class Monster : Mob
    {
        protected Monster(WorldManager world, int entityId, MobType type, MetaData data)
            : base(world, entityId, type, data)
        {
            MinExp = 5;
            MaxExp = 5;
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

        protected override List<World.Paths.PathCoordinate> GetNewPath()
        {
            var pathFinder = new World.Paths.PathFinder(this.World);
            var player = World.GetClosestPlayer(this.Position, 24.0) as Player;
            Target = player;
            if (Target != null)
            {
                var path = pathFinder.CreatePathToEntity(this, Target, SightRange);
                if (path != null)
                    Server.Logger.Log(LogLevel.Debug, "Found player within {0}, can reach in {1} blocks", SightRange, path.Count);

                return path;
            }

            return null;
        }
    }
}
