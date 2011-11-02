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
