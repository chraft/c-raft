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

namespace Chraft.Utilities.Blocks
{
    public enum BlockFace : sbyte
    {
        Self = -1,
        Held = -1,
        Down = 0,
        Up = 1,
        East = 2,
        West = 3,
        North = 4,
        South = 5,
        NorthEast = 6,
        NorthWest = 7,
        SouthEast = 8,
        SouthWest = 9
    }

}