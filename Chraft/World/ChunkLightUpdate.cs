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

namespace Chraft.World
{
    public class ChunkLightUpdate
    {
        private int _X;
        private int _Y;
        private int _Z;

        public int X { get { return _X; } }
        public int Y { get { return _Y; } }
        public int Z { get { return _Z; } }

        public Chunk Chunk { get { return _Chunk; } }
        private Chunk _Chunk;

        public ChunkLightUpdate(Chunk chunk)
        {
            _X = -1;
            _Y = -1;
            _Z = -1;
            _Chunk = chunk;
        }
        public ChunkLightUpdate(Chunk chunk, int x, int y, int z)
        {
            _X = x;
            _Y = y;
            _Z = z;
            _Chunk = chunk;
        }
    }
}
