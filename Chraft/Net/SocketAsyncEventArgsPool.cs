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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Chraft.Net
{
	public class SocketAsyncEventArgsPool
	{
		private ConcurrentStack<SocketAsyncEventArgs> m_EventsPool;

		public SocketAsyncEventArgsPool(int numConnection)
		{
			m_EventsPool = new ConcurrentStack<SocketAsyncEventArgs>();
		}

		public SocketAsyncEventArgs Pop()
		{		
			if(m_EventsPool.IsEmpty)
				return new SocketAsyncEventArgs();

			SocketAsyncEventArgs popped;
			m_EventsPool.TryPop(out popped);

			return popped;			
		}

		public void Push(SocketAsyncEventArgs item)
		{
			if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
			
            m_EventsPool.Push(item);			
		}

		public int Count
		{
			get{return m_EventsPool.Count;}
		}
		
		public void Dispose()
		{
			foreach (SocketAsyncEventArgs e in m_EventsPool)
			{
				e.Dispose();
			}

			m_EventsPool.Clear();
		}
	}
}
