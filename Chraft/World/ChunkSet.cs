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
using System.Collections.Concurrent;
using System.Threading;
using Chraft.Utilities;
using Chraft.Utilities.Coords;

namespace Chraft.World
{
	public class ChunkSet
	{
        private readonly ConcurrentDictionary<int, Chunk> Chunks = new ConcurrentDictionary<int, Chunk>();

        public ICollection<int> Keys { get { return Chunks.Keys; } }
		public ICollection<Chunk> Values { get { return Chunks.Values; } }
		public int Count { get { return Chunks.Count; } }
		public bool IsReadOnly { get { return false; } }
        public int Changes;

		public Chunk this[UniversalCoords coords]
		{
			get
			{
                Chunk chunk;
                Chunks.TryGetValue(coords.ChunkPackedCoords, out chunk);
				return chunk;
			}
			private set
			{
                Chunks.AddOrUpdate(coords.ChunkPackedCoords, value, (key, oldValue) => value);
			}
		}

        public Chunk this[int chunkX, int chunkZ]
        {
            get
            {
                Chunk chunk;
                int packedCoords = UniversalCoords.FromChunkToPackedChunk(chunkX, chunkZ);
                Chunks.TryGetValue(packedCoords, out chunk);
                return chunk;
            }
            private set
            {
                int packedCoords = UniversalCoords.FromChunkToPackedChunk(chunkX, chunkZ);
                Chunks.AddOrUpdate(packedCoords, value, (key, oldValue) => value);
            }
        }

		public void Add(Chunk chunk)
		{
            this[chunk.Coords] = chunk;
		    chunk.InitBlockChangesTimer();
            Interlocked.Increment(ref Changes);
		}

        public bool Remove(UniversalCoords coords)
		{
            Chunk chunk;
            Interlocked.Increment(ref Changes);
            
            bool result = Chunks.TryRemove(coords.ChunkPackedCoords, out chunk);

            if(result)
                chunk.Dispose();

            return result;
		}

		internal bool Remove(Chunk chunk)
		{
            chunk.Deleted = true;
            return Remove(chunk.Coords);
		}
	}
}
