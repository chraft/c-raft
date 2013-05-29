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
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;
using Chraft.World;

namespace Chraft.Entity
{
    public abstract class Animal: Mob
    {
        protected Animal(WorldManager world, int entityId, MobType type, MetaData data)
            : base(world, entityId, type, data)
        {
            MinExp = 1;
            MaxExp = 3;
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
