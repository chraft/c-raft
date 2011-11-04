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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Chraft.Net.Packets;
using Chraft.World;
using System.IO.Compression;

namespace Chraft.Net
{
    public class BigEndianStream : Stream
    {
        public Stream FileStream { get; private set; }
        public StreamRole Role { get; private set; }

        public override bool CanRead { get { return FileStream.CanRead; } }
        public override bool CanSeek { get { return FileStream.CanSeek; } }
        public override bool CanWrite { get { return FileStream.CanWrite; } }
        public override long Length { get { return FileStream.Length; } }
        public override long Position { get { return FileStream.Position; } set { FileStream.Position = value; } }

        public BigEndianStream(Stream stream, StreamRole role)
        {
            FileStream = stream;
            Role = role;
        }

        public new byte ReadByte()
        {
            int b = FileStream.ReadByte();
            if (b >= byte.MinValue && b <= byte.MaxValue)
                return (byte)b;
            throw new EndOfStreamException();
        }

        public byte[] ReadBytes(int Count)
        {
            byte[] Input = new byte[Count];

            for (int i = Count - 1; i >= 0; i--)
            {
                Input[i] = ReadByte();
            }

            return (Input);
        }

        public sbyte ReadSByte()
        {
            return unchecked((sbyte)ReadByte());
        }

        public short ReadShort()
        {
            return unchecked((short)((ReadByte() << 8) | ReadByte()));
        }

        public int ReadInt()
        {
            return unchecked((ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
        }

        public long ReadLong()
        {
            return unchecked((ReadByte() << 56) | (ReadByte() << 48) | (ReadByte() << 40) | (ReadByte() << 32)
                | (ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
        }

        public unsafe float ReadFloat()
        {
            int i = ReadInt();
            return *(float*)&i;
        }

        public unsafe double ReadDouble()
        {
            byte[] r = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                r[i] = ReadByte();
            }
            return BitConverter.ToDouble(r, 0);
        }

        public string ReadString16(short maxLen)
        {
            int len = ReadShort();
            if (len > maxLen)
                throw new IOException("String field too long");
            byte[] b = new byte[len * 2];
            for (int i = 0; i < len * 2; i++)
                b[i] = (byte)ReadByte();
            return ASCIIEncoding.BigEndianUnicode.GetString(b);
        }

        public string ReadString8(short maxLen)
        {
            int len = ReadShort();
            if (len > maxLen)
                throw new IOException("String field too long");
            byte[] b = new byte[len];
            for (int i = 0; i < len; i++)
                b[i] = (byte)ReadByte();
            return ASCIIEncoding.UTF8.GetString(b);
        }

        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        public void Write(byte data)
        {
            FileStream.WriteByte(data);
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
            byte[] b = ASCIIEncoding.BigEndianUnicode.GetBytes(data);
            Write((short)data.Length);
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

        public override void Flush()
        {
            FileStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return FileStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return FileStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            FileStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            FileStream.Write(buffer, offset, count);
        }

        public double ReadDoublePacked()
        {
            return (double)ReadInt() / 32.0;
        }

        public void WriteDoublePacked(double d)
        {
            Write((int)(d * 32.0));
        }
    }
}