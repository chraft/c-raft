using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;


namespace Chraft.World.NBT
{
    public class NBTWriter : IDisposable
    {
        private BinaryWriter _bWrite;


        public NBTWriter(FileStream fileOut)
        {
            GZipStream _gzStream = new GZipStream(fileOut, CompressionMode.Compress);

            _bWrite = new BinaryWriter(_gzStream);
        }

        public NBTWriter(MemoryStream fileOut, int version)
        {
            throw new NotImplementedException();
        }


        public static void WritePayload(BinaryWriter bWrite, dynamic payload, TagNodeType type)
        {
            switch (type)
            {
                case TagNodeType.TAG_END:
                    {
                        bWrite.Write(0);
                    }
                    break;
                case TagNodeType.TAG_BYTE:
                    {
                        bWrite.Write((byte)payload);
                    }
                    break;
                case TagNodeType.TAG_BYTEA:
                    {
                        WritePayload(bWrite, ((byte[])payload).Length, TagNodeType.TAG_INT);

                        for (int i = 0; i < ((byte[])payload).Length; i++)
                        {
                            WritePayload(bWrite, ((byte[])payload)[i], TagNodeType.TAG_BYTE);
                        }
                    }
                    break;
                case TagNodeType.TAG_SHORT:
                    {
                        bWrite.Write(EndianConverter.SwapInt16((short)payload));
                    }
                    break;
                case TagNodeType.TAG_INT:
                    {
                        bWrite.Write(EndianConverter.SwapInt32((int)payload));
                    }
                    break;
                case TagNodeType.TAG_LONG:
                    {
                        bWrite.Write(EndianConverter.SwapInt64((long)payload));
                    }
                    break;
                case TagNodeType.TAG_SINGLE:
                    {
                        bWrite.Write(EndianConverter.SwapSingle((float)payload));
                    }
                    break;
                case TagNodeType.TAG_DOUBLE:
                    {
                        bWrite.Write(EndianConverter.SwapDouble((double)payload));
                    }
                    break;
                case TagNodeType.TAG_STRING:
                    {
                        WritePayload(bWrite, ((string)payload).Length, TagNodeType.TAG_SHORT);

                        byte[] _outString = Encoding.UTF8.GetBytes((string)payload);

                        for (int i = 0; i < ((string)payload).Length; i++)
                            WritePayload(bWrite, _outString[i], TagNodeType.TAG_BYTE);
                    }
                    break;
                default:
                    {
                        throw new NotSupportedException("Tag type is not supported by this writer!");
                    }
            }
        }


        private void write(BinaryWriter bWrite, INBTTag tag)
        {
            WritePayload(bWrite, tag.Type, TagNodeType.TAG_BYTE);

            if (tag.Type != TagNodeType.TAG_END)
                WritePayload(bWrite, tag.Name, TagNodeType.TAG_STRING);

            if (tag is TagNode)
            {
                WritePayload(bWrite, tag.Payload, tag.Type);
            }
            else if (tag is TagNodeList)
            {
                WritePayload(bWrite, ((TagNodeList)tag).ChildType, TagNodeType.TAG_BYTE);
                WritePayload(bWrite, ((TagNodeList)tag).Count, TagNodeType.TAG_INT);

                foreach (INBTTag node in (TagNodeList)tag)
                {
                    WritePayload(bWrite, node.Payload, node.Type);
                }
            }
            else if (tag is TagNodeListNamed)
            {
                foreach (INBTTag node in ((TagNodeListNamed)tag).Values)
                {
                    if (node.Type != TagNodeType.TAG_END)
                        write(bWrite, node);
                }

                bWrite.Write(0);
            }
        }


        public void BeginWrite(INBTTag tagIn)
        {
            write(this._bWrite, tagIn);
        }

        public void Dispose()
        {
            _bWrite.Close();
            _bWrite.Dispose();
        }
    }
}
