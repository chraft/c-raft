using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Chraft.World
{
    public class ChunkEntry
    {
        public readonly ManualResetEventSlim ChunkLock = new ManualResetEventSlim(false);

        public int State;
        public int ThreadsWaiting;

        public const int NotInitialized = 0;
        public const int InProgress = 1;
        public const int Initialized = 2;
    }
}
