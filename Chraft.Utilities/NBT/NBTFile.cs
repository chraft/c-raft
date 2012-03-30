/*  Minecraft NBT reader
 * 
 *  Copyright 2010-2011 Michael Ong, all rights reserved.
 *  
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public License
 *  as published by the Free Software Foundation; either version 2
 *  of the License, or (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Chraft.Utilities.NBT
{
    /// <summary>
    /// A Minecraft NBT file.
    /// </summary>
    public class NBTFile : IDisposable
    {
        List<NBTTag> _list;
        Dictionary<string, NBTTag> _dict;
        /// <summary>
        /// Gets the contents of this NBT file.
        /// </summary>
        public ICollection Contents
        {
            get
            {
                if (NamedNBT)
                    return _dict;
                return _list;
            }
        }

        /// <summary>
        /// Gets the list type of this NBT file. 'true' if the file is TAG_COMPOUND, 'false' for TAG_LIST.
        /// </summary>
        public bool NamedNBT { get; private set; }

        /// <summary>
        /// Gets the root name of this NBT file.
        /// </summary>
        public string RootName { get; private set; }

        /// <summary>
        /// Returns the total number of elements the root list have.
        /// </summary>
        public int CollectionCount { get; private set; }

        /// <summary>
        /// Gets a NBT tag on a specified index (TAG_LIST only).
        /// </summary>
        /// <param name="index">The index of the tag.</param>
        /// <returns>The NBTTag of that index.</returns>
        public NBTTag this[int index]
        {
            get { return _list[index]; }
        }
        /// <summary>
        /// Gets a NBT tag on a specified key (TAG_COMPOUND only).
        /// </summary>
        /// <param name="name">The key of the tag.</param>
        /// <returns>The NBTTag of that index.</returns>
        public NBTTag this[string name]
        {
            get { return _dict[name]; }
        }

        private NBTFile()
        {
            _list = new List<NBTTag>();
            _dict = new Dictionary<string, NBTTag>();

            NamedNBT = false;
            RootName = "";

            CollectionCount = 0;
        }
        /// <summary>
        /// Creates a new NBT file.
        /// </summary>
        /// <param name="named">The list type of this NBT file, 'true' for TAG_COMPOUND, 'false' for TAG_LIST.</param>
        /// <param name="rootname">The root name of this NBT file.</param>
        public NBTFile(bool named, string rootname)
        {
            NamedNBT = named;
            RootName = rootname;
        }

        /// <summary>
        /// Inserts a new NBT tag in the list.
        /// </summary>
        /// <param name="tag">The tag to be inserted.</param>
        public void InsertTag(NBTTag tag)
        {
            if (NamedNBT)
                _dict.Add(tag.Name, tag);
            else
                _list.Add(tag);
        }
        /// <summary>
        /// Removes a existing NBT tag in the list.
        /// </summary>
        /// <param name="tag">The tag to be removed.</param>
        public void RemoveTag(NBTTag tag)
        {
            if (NamedNBT)
                _dict.Remove(tag.Name);
            else
                _list.Remove(tag);
        }
        /// <summary>
        /// Modifies the a NBT tag in the list.
        /// </summary>
        /// <param name="tag">The tag to be modified.</param>
        /// <param name="info">The new tag to be replaced.</param>
        public void ModifyTag(NBTTag tag, NBTTag info)
        {
            if (NamedNBT)
                _dict[tag.Name] = info;
            else
                _list[_list.IndexOf(tag)] = info;
        }

        /// <summary>
        /// Saves this NBT file in a stream.
        /// </summary>
        /// <param name="stream">The output stream this NBT file will write onto.</param>
        /// <param name="version">The compression version of the NBT, specify '1' for the original gzip compression, '2' for the mcregion zlib compression.</param>
        public void SaveTag(Stream stream, int version)
        {
            Stream compressStream;

            if (version == 1)
            {
                compressStream = new GZipStream(stream, CompressionMode.Compress);
            }
            else
            {
                stream.WriteByte(0);
                stream.WriteByte(0);

                compressStream = new DeflateStream(stream, CompressionMode.Compress);
            }

            BinaryWriter writer = new BinaryWriter(compressStream);
            {
                writer.Write((byte)(NamedNBT ? 10 : 9));
                writer.Write(EndiannessConverter.ToInt16((short)RootName.Length));

                byte[] oString = Encoding.UTF8.GetBytes(RootName);

                for (int i = 0; i < oString.Length; i++)
                {
                    writer.Write(oString[i]);
                }

                if (this.NamedNBT)
                {
                    foreach (KeyValuePair<string, NBTTag> tag in _dict)
                    {
                        writer.Write(tag.Value.Type);
                        writer.Write(EndiannessConverter.ToInt16((short)tag.Value.Name.Length));

                        oString = Encoding.UTF8.GetBytes(tag.Value.Name);

                        for (int i = 0; i < oString.Length; i++)
                        {
                            writer.Write(oString[i]);
                        }

                        SavePayload(ref writer, tag.Value.Type, tag.Value.Payload);
                    }
                    writer.Write((byte)0);
                }
                else
                {
                    writer.Write(_list[0].Type);
                    writer.Write(EndiannessConverter.ToInt32(_list.Count));

                    for (int i = 0; i < _list.Count; i++)
                    {
                        SavePayload(ref writer, _list[0].Type, _list[i].Payload);
                    }
                }
            }
            writer.Dispose();
            compressStream.Dispose();
        }

        /// <summary>
        /// Opens an existing NBT file from a stream.
        /// </summary>
        /// <param name="stream">The stream to get the NBT file from.</param>
        /// <param name="version">The compression version of the NBT, specify '1' for the original gzip compression, '2' for the mcregion zlib compression.</param>
        /// <returns>An opened NBT file.</returns>
        public static NBTFile OpenFile(Stream stream, int version)
        {
            NBTFile file = new NBTFile();

            Stream compressStream;

            if (version == 1)
            {
                compressStream = new GZipStream(stream, CompressionMode.Decompress);
            }
            else
            {
                stream.ReadByte();
                stream.ReadByte();

                compressStream = new DeflateStream(stream, CompressionMode.Decompress);
            }

            BinaryReader reader = new BinaryReader(compressStream);
            {
                Encoding textEncoding = Encoding.UTF8;

                file.NamedNBT = reader.ReadByte() == 10;
                file.RootName = textEncoding.GetString(reader.ReadBytes(EndiannessConverter.ToInt16(reader.ReadInt16())));

                if (file.NamedNBT)
                {
                    byte type;

                    while ((type = reader.ReadByte()) != 0)
                    {
                        string name = textEncoding.GetString(reader.ReadBytes(EndiannessConverter.ToInt16(reader.ReadInt16())));

                        file.InsertTag(new NBTTag(name, type, file.ReadPayload(ref reader, type)));
                    }
                }
                else
                {
                    byte type = reader.ReadByte();
                    int size = EndiannessConverter.ToInt32(reader.ReadInt32());

                    for (int i = 0; i < size; i++)
                    {
                        file.InsertTag(new NBTTag("", type, file.ReadPayload(ref reader, type)));
                    }
                }
            }
            reader.Dispose();
            compressStream.Dispose();

            return file;
        }

        private dynamic ReadPayload(ref BinaryReader reader, byte type)
        {
            switch (type)
            {
                case 0:
                    return 0;
                case 1:
                    return reader.ReadByte();
                case 2:
                    return EndiannessConverter.ToInt16(reader.ReadInt16());
                case 3:
                    return EndiannessConverter.ToInt32(reader.ReadInt32());
                case 4:
                    return EndiannessConverter.ToInt64(reader.ReadInt64());
                case 5:
                    return EndiannessConverter.ToSingle(reader.ReadSingle());
                case 6:
                    return EndiannessConverter.ToDouble(reader.ReadDouble());
                case 7:
                    return reader.ReadBytes(EndiannessConverter.ToInt32(reader.ReadInt32()));
                case 8:
                    return Encoding.UTF8.GetString(reader.ReadBytes(EndiannessConverter.ToInt16(reader.ReadInt16())));
                case 9:
                    {
                        List<NBTTag> ret = new List<NBTTag>();
                        {
                            byte containerType = reader.ReadByte();
                            int containerSize = EndiannessConverter.ToInt32(reader.ReadInt32());

                            for (int i = 0; i < containerSize; i++)
                                ret.Add(new NBTTag("", containerType, ReadPayload(ref reader, containerType)));
                        }
                        return ret;
                    }
                case 10:
                    {
                        Dictionary<string, NBTTag> dic = new Dictionary<string, NBTTag>();
                        {
                            byte containerType;

                            while ((containerType = reader.ReadByte()) != 0)
                            {
                                string containerName = Encoding.UTF8.GetString(reader.ReadBytes(EndiannessConverter.ToInt16(reader.ReadInt16())));

                                dic.Add(containerName, new NBTTag(containerName, containerType, ReadPayload(ref reader, containerType)));
                            }
                        }
                        return dic;
                    }
                default:
                    throw new NotSupportedException("Tag type is invalid!");
            }
        }
        private void SavePayload(ref BinaryWriter writer, byte type, dynamic payload)
        {
            switch (type)
            {
                case 0:
                    writer.Write((byte)0);
                    break;
                case 1:
                    writer.Write((byte)payload);
                    break;
                case 2:
                    writer.Write(EndiannessConverter.ToInt16(payload));
                    break;
                case 3:
                    writer.Write(EndiannessConverter.ToInt32(payload));
                    break;
                case 4:
                    writer.Write(EndiannessConverter.ToInt64(payload));
                    break;
                case 5:
                    writer.Write(EndiannessConverter.ToSingle(payload));
                    break;
                case 6:
                    writer.Write(EndiannessConverter.ToDouble(payload));
                    break;
                case 7:
                    writer.Write(EndiannessConverter.ToInt32(payload.Length));

                    for (int i = 0; i < payload.Length; i++)
                    {
                        writer.Write(payload[i]);
                    }
                    break;
                case 8:
                    writer.Write(EndiannessConverter.ToInt16((short)payload.Length));

                    byte[] oString = Encoding.UTF8.GetBytes(payload);

                    for (int i = 0; i < oString.Length; i++)
                    {
                        writer.Write(oString[i]);
                    }
                    break;
                case 9:

                    writer.Write(payload[0].Type);
                    writer.Write(EndiannessConverter.ToInt32(payload.Count));

                    foreach (NBTTag tag in payload)
                    {
                        SavePayload(ref writer, tag.Type, tag.Payload);
                    }

                    break;
                case 10:

                    foreach (KeyValuePair<string, NBTTag> tag in payload)
                    {
                        writer.Write(tag.Value.Type);
                        writer.Write(EndiannessConverter.ToInt16((short)tag.Key.Length));

                        byte[] cString = Encoding.UTF8.GetBytes(tag.Key);

                        for (int i = 0; i < cString.Length; i++)
                        {
                            writer.Write(cString[i]);
                        }

                        SavePayload(ref writer, tag.Value.Type, tag.Value.Payload);
                    }
                    writer.Write((byte)0);

                    break;
            }
        }

        /// <summary>
        /// Cleans up any resources this file used.
        /// </summary>
        public void Dispose()
        {
            _dict.Clear();
            _list.Clear();

            _dict = null;
            _list = null;
        }
    }
}
