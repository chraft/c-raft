using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Chraft.World
{
	public class ChunkSet
	{
		private readonly object _ChunksWriteLock = new object();
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
            Interlocked.Increment(ref Changes);
		}

        public bool Remove(UniversalCoords coords)
		{
            Chunk chunk;
            Interlocked.Increment(ref Changes);
            return Chunks.TryRemove(coords.ChunkPackedCoords, out chunk);
		}

		internal bool Remove(Chunk chunk)
		{
            chunk.Deleted = true;
            return Remove(chunk.Coords);
		}
	}
}
