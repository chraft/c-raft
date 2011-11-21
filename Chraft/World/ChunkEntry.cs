#region C#raft License

//This file is part of C#raft. Copyright C#raft Team
//
//C#raft is free software: you can redistribute it and/or modify
//it under the terms of the GNU Affero General Public License as
//published by the Free Software Foundation, either version 3 of the
//License, or (at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU Affero General Public License for more details.
//
//You should have received a copy of the GNU Affero General Public License
//along with this program. If not, see <http://www.gnu.org/licenses/>.

#endregion
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Chraft.World
{
    public class ChunkEntry
    {
        public ConcurrentQueue<ClientRequest> Requests = new ConcurrentQueue<ClientRequest>();
        public readonly ManualResetEvent ChunkLock = new ManualResetEvent(false);

        public int State;
        public int NotifyStatus;
        public int ThreadsWaiting;

        public Chunk ChunkRequested;

        public const int NotInitialized = 0;
        public const int InProgress = 1;
        public const int Initialized = 2;
        public const int NotNotified = 3;
        public const int Notified = 4;
    }
}
