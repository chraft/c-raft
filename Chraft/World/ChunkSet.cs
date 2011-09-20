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
        private readonly ConcurrentDictionary<PointI, Chunk> Chunks = new ConcurrentDictionary<PointI, Chunk>();

        public ICollection<PointI> Keys { get { return Chunks.Keys; } }
		public ICollection<Chunk> Values { get { return Chunks.Values; } }
		public int Count { get { return Chunks.Count; } }
		public bool IsReadOnly { get { return false; } }
        public int Changes;

		public Chunk this[int x, int z]
		{
			get
			{
                Chunk chunk;
                Chunks.TryGetValue(new PointI(x, z), out chunk);
				return chunk;
			}
			private set
			{
                Chunks.AddOrUpdate(new PointI(x, z), value, (key, oldValue) => value);
			}
		}

		public void Add(Chunk value)
		{
			this[value.X, value.Z] = value;
            Interlocked.Increment(ref Changes);
		}

		public bool Remove(int x, int z)
		{
            Chunk chunk;
            Interlocked.Increment(ref Changes);
            return Chunks.TryRemove(new PointI(x, z), out chunk);
		}

		internal bool Remove(Chunk c)
		{
            c.Deleted = true;
			return Remove(c.X, c.Z);
		}
	}
}
