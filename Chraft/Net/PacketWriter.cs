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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Chraft.Net.Packets;

namespace Chraft.Net
{
    public class PacketWriter
    {
        private static ConcurrentStack<PacketWriter> _Pool = new ConcurrentStack<PacketWriter>();

        private int _Capacity;
        public int Capacity
        {
            get { return _Capacity; }
            set { _Capacity = value; }
        }

        private MemoryStream _Stream;
        public MemoryStream UnderlyingStream
        {
            get { return _Stream; }
            set { _Stream = value; }
        }

        private Queue<byte[]> _Strings;
        public Queue<byte[]> Strings
        {
            get { return _Strings; }
            set { _Strings = value; }
        }

        public PacketWriter(int capacity)
        {
            _Stream = new MemoryStream(capacity);
            _Capacity = capacity;
        }

        public static PacketWriter CreateInstance(int capacity, Queue<byte[]> strings)
        {
            PacketWriter pw = CreateInstance(capacity);
            pw.Strings = strings;
            return pw;
        }

        public static PacketWriter CreateInstance()
        {
            return CreateInstance(32);
        }

        public static PacketWriter CreateInstance(int capacity)
        {
            PacketWriter pw = null;

            if (_Pool.Count > 0)
            {
                _Pool.TryPop(out pw);
                    
                if (pw != null)
                {
                    pw._Capacity = capacity;
                    pw._Stream.SetLength(0);
                    pw._Stream.Position = 0;
                }
            }

            if (pw == null)
                pw = new PacketWriter(capacity);

            return pw;
        }

        public static void ReleaseInstance(PacketWriter pw)
        {
            if (!_Pool.Contains(pw))
                _Pool.Push(pw);
            else
            {
                try
                {
                    using (StreamWriter op = new StreamWriter("neterr.log", true))
                    {
                        op.WriteLine("{0}\tInstance pool contains writer", DateTime.Now);
                        op.WriteLine();
                    }
                }
                catch
                {
                    Console.WriteLine("net error");
                }
            }
        }

        public void Write(byte data)
        {
            _Stream.WriteByte(data);
        }

        public void WriteByte(byte data)
        {
            _Stream.WriteByte(data);
        }

        public void Write(sbyte data)
        {
            Write(unchecked((byte)data));
        }

        public void Write(short data)
        {
            Write(unchecked((byte)(data >> 8)));
            Write(unchecked((byte)data));
        }

        public void Write(ushort data)
        {
            Write(unchecked((byte)(data >> 8)));
            Write(unchecked((byte)data));
        }

        public void Write(int data)
        {
            Write(unchecked((byte)(data >> 24)));
            Write(unchecked((byte)(data >> 16)));
            Write(unchecked((byte)(data >> 8)));
            Write(unchecked((byte)data));
        }

        public void Write(long data)
        {
            Write(unchecked((byte)(data >> 56)));
            Write(unchecked((byte)(data >> 48)));
            Write(unchecked((byte)(data >> 40)));
            Write(unchecked((byte)(data >> 32)));
            Write(unchecked((byte)(data >> 24)));
            Write(unchecked((byte)(data >> 16)));
            Write(unchecked((byte)(data >> 8)));
            Write(unchecked((byte)data));
        }

        public unsafe void Write(float data)
        {
            Write(*(int*)&data);
        }

        public unsafe void Write(double data)
        {
            Write(*(long*)&data);
        }

        public void Write(string data)
        {
            byte[] b;
            int length = data.Length;
            if (_Strings != null && _Strings.Count > 0)
            {
                b = _Strings.Dequeue();
                length = b.Length/2;
            }
            else
                b = ASCIIEncoding.BigEndianUnicode.GetBytes(data);

            Write((short)length);
            Write(b, 0, b.Length);
        }

        public void Write8(string data)
        {
            byte[] b = ASCIIEncoding.UTF8.GetBytes(data);
            Write((short)b.Length);
            Write(b, 0, b.Length);
        }

        public void Write(bool data)
        {
            Write((byte)(data ? 1 : 0));
        }

        public void WritePacket(Packet packet)
        {
            Write((byte)packet.GetPacketType());
            //packet.WriteFlush(this);
        }

        public void Write(MetaData Data)
        {
            Data.Write(this);
        }

        public void Write(int[] array)
        {
            int byteDim = array.Length*sizeof (int);
            byte[] bytes = new byte[byteDim];
            Buffer.BlockCopy(array, 0, bytes, 0, byteDim);
            _Stream.Write(bytes, 0, byteDim);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _Stream.Write(buffer, offset, count);
        }

        public void WriteDoublePacked(double d)
        {
            Write((int)(d * 32.0));
        }
        
    }
}
