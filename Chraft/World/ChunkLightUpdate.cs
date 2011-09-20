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
