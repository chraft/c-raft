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

using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;

namespace Chraft.World.Blocks.Physics
{
    public class FallingGravel : FallingSand
    {
        public FallingGravel(WorldManager world, AbsWorldCoords pos) : base(world, pos)
        {
            Type = Net.Packets.AddObjectVehiclePacket.ObjectType.FallingObjects;
            BlockId = (byte) BlockData.Blocks.Gravel;
        }
    }
}
