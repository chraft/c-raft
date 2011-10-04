using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chraft.Net
{
    public class PacketReader
    {
        private byte[] _Data;
		private int _Size;
		private int _Index;
        private bool _Failed;

        public StreamRole Role { get; private set; }

        public int Index
        {
            get
            {
                return _Index;
            }
        }

        public int Size
        {
            get
            {
                return _Size;
            }
        }

        public bool Failed
        {
            get { return _Failed; }
            set { _Failed = value; }
        }

        public PacketReader(byte[] data, int size, StreamRole role)
        {
            _Data = data;
            _Size = size;
            _Index = 1;
            _Failed = false;
            Role = role;
        }

        public bool CheckBoundaries(int size)
        {
            if ((_Index + size) > _Size)
                _Failed = true;

            return !_Failed;
        }
        public new byte ReadByte()
        {
            if (!CheckBoundaries(1))
                return 0;

            int b = _Data[_Index++];
            
            return (byte)b;
        }

        public byte[] ReadBytes(int Count)
        {
            byte[] input = new byte[Count];

            for (int i = Count - 1; i >= 0; i--)
            {
                input[i] = ReadByte();
            }

            return input;
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
                b[i] = ReadByte();
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

        public double ReadDoublePacked()
        {
            return (double)ReadInt() / 32.0;
        }

        public MetaData ReadMetaData()
        {
            return new MetaData(this);
        }
    }
}
