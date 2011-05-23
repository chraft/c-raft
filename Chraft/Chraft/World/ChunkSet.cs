using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Chraft.World
{
	public class ChunkSet
	{
		private readonly object _ChunksWriteLock = new object();
		private readonly Dictionary<PointI, Chunk> Chunks = new Dictionary<PointI, Chunk>();

		public PointI[] Keys { get { lock (_ChunksWriteLock) return Chunks.Keys.ToArray(); } }
		public Chunk[] Values { get { lock (_ChunksWriteLock) return Chunks.Values.ToArray(); } }
		public int Count { get { return Chunks.Count; } }
		public bool IsReadOnly { get { return false; } }

		public Chunk this[int x, int z]
		{
			get
			{
				lock (_ChunksWriteLock)
				{
					if (Chunks.ContainsKey(new PointI(x, z)))
						return Chunks[new PointI(x, z)];
					return null;
				}
			}
			private set
			{
				Chunks[new PointI(x, z)] = value;
			}
		}

		public void Add(Chunk value)
		{
			lock (_ChunksWriteLock)
				this[value.X >> 4, value.Z >> 4] = value;
		}

		public bool ContainsKey(PointI key)
		{
			return Chunks.ContainsKey(key);
		}

		public bool Remove(int x, int z)
		{
			lock (_ChunksWriteLock)
				return Chunks.Remove(new PointI(x, z));
		}

		internal bool Remove(Chunk c)
		{
			return Remove(c.X >> 4, c.Z >> 4);
		}
	}
}
