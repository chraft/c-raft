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

namespace Chraft.Net
{
	public class ByteQueue
	{
		private int m_Head;
		private int m_Tail;
		private int m_Size;

		private byte[] m_Buffer;

		public int Length{ get{ return m_Size; } }

		public byte[] UnderlyingBuffer
		{
			get { return m_Buffer; }
            set { m_Buffer = value; }
		}

		public int Head
		{
			get { return m_Head; }
		}

		public int Tail
		{
			get { return m_Tail; }
		}

		public int Size
		{
			get { return m_Size; }
		}

		public ByteQueue()
		{
			m_Buffer = new byte[2048];
		}

		public void Clear()
		{
			m_Head = 0;
			m_Tail = 0;
			m_Size = 0;
		}

		public void SetCapacity( int capacity, bool alwaysNewBuffer ) 
		{
            if(!alwaysNewBuffer)
            {
                if(m_Buffer == null || m_Buffer.Length < capacity)
                    m_Buffer = new byte[capacity]; 
            }
            else
            {
                byte[] newBuffer = new byte[capacity];

                if ( m_Size > 0 )
                {
                    if ( m_Head < m_Tail )
                    {
                        Buffer.BlockCopy( m_Buffer, m_Head, newBuffer, 0, m_Size );
                    }
                    else
                    {
                        Buffer.BlockCopy( m_Buffer, m_Head, newBuffer, 0, m_Buffer.Length - m_Head );
                        Buffer.BlockCopy( m_Buffer, 0, newBuffer, m_Buffer.Length - m_Head, m_Tail );
                    }
                }

                m_Buffer = newBuffer;
            }

			m_Head = 0;
			m_Tail = m_Size;
			
		}

		public byte GetPacketID()
		{
			if ( m_Size >= 1 )
				return m_Buffer[m_Head];

			return 0xF0;
		}

        public int CopyAll(byte[] buffer)
        {
            if (m_Head < m_Tail)
            {
                Buffer.BlockCopy(m_Buffer, m_Head, buffer, 0, m_Size);
            }
            else
            {
                int rightLength = (m_Buffer.Length - m_Head);

                if (rightLength >= m_Size)
                {
                    Buffer.BlockCopy(m_Buffer, m_Head, buffer, 0, m_Size);
                }
                else
                {
                    Buffer.BlockCopy(m_Buffer, m_Head, buffer, 0, rightLength);
                    Buffer.BlockCopy(m_Buffer, 0, buffer, 0 + rightLength, m_Size - rightLength);
                }
            }

            return m_Size;
        }

		public int Dequeue( byte[] buffer, int offset, int size )
		{
			if ( size > m_Size )
				size = m_Size;

			if ( size == 0 )
				return 0;

			if ( m_Head < m_Tail )
			{
				Buffer.BlockCopy( m_Buffer, m_Head, buffer, offset, size );
			}
			else
			{
				int rightLength = ( m_Buffer.Length - m_Head );

				if ( rightLength >= size )
				{
					Buffer.BlockCopy( m_Buffer, m_Head, buffer, offset, size );
				}
				else
				{
					Buffer.BlockCopy( m_Buffer, m_Head, buffer, offset, rightLength );
					Buffer.BlockCopy( m_Buffer, 0, buffer, offset + rightLength, size - rightLength );
				}
			}

			m_Head = ( m_Head + size ) % m_Buffer.Length;
			m_Size -= size;

			if ( m_Size == 0 )
			{
				m_Head = 0;
				m_Tail = 0;
			}

			return size;
		}

		public void Enqueue( byte[] buffer, int offset, int size )
		{
			if ( (m_Size + size) > m_Buffer.Length )
				SetCapacity( (m_Size + size + 2047) & ~2047, true );

			if ( m_Head < m_Tail )
			{
				int rightLength = ( m_Buffer.Length - m_Tail );

				if ( rightLength >= size )
				{
					Buffer.BlockCopy( buffer, offset, m_Buffer, m_Tail, size );
				}
				else
				{
					Buffer.BlockCopy( buffer, offset, m_Buffer, m_Tail, rightLength );
					Buffer.BlockCopy( buffer, offset + rightLength, m_Buffer, 0, size - rightLength );
				}
			}
			else
			{
				Buffer.BlockCopy( buffer, offset, m_Buffer, m_Tail, size );
			}

			m_Tail = ( m_Tail + size ) % m_Buffer.Length;
			m_Size += size;
		}
	}
}