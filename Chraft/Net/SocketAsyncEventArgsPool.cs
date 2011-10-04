using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Chraft.Net
{
	public class SocketAsyncEventArgsPool
	{
		private Stack<SocketAsyncEventArgs> m_EventsPool;

		public SocketAsyncEventArgsPool(int numConnection)
		{
			m_EventsPool = new Stack<SocketAsyncEventArgs>(numConnection);
		}

		public SocketAsyncEventArgs Pop()
		{
			lock(m_EventsPool)
			{
				if(m_EventsPool.Count == 0)
								return new SocketAsyncEventArgs();
							else
								return m_EventsPool.Pop();
			}
		}

		public void Push(SocketAsyncEventArgs item)
		{
			if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
			lock(m_EventsPool)
			{
				m_EventsPool.Push(item);
			}
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
