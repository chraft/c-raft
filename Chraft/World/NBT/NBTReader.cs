﻿/*  Minecraft NBT reader
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
﻿using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace Chraft.World.NBT
{
    /// <summary>
    /// Reader for Name Binary Tag files.
    /// </summary>
    public class NBTReader : IDisposable
    {
        private BinaryReader _bRead = null;

        /// <summary>
        /// Base BinaryStream of the reader.
        /// </summary>
        public BinaryReader BaseReader
        {
            get { return _bRead; }
        }


        /// <summary>
        /// Creates a new NBT reader with a specified file location.
        /// </summary>
        /// <param name="fileIn">A file stream in which the NBT is located.</param>
        public NBTReader(FileStream fileIn)
        {
            // decompress the stream
            GZipStream gStream = new GZipStream(fileIn, CompressionMode.Decompress);

            // route the stream to a binary reader
            _bRead = new BinaryReader(gStream);
        }

        /// <summary>
        /// Creates a new NBT reader with a specified memory stream.
        /// </summary>
        /// <param name="memIn">The memory stream in which the NBT is located.</param>
        /// <param name="version">The compression version of the NBT, choose 1 for GZip and 2 for ZLib.</param>
        public NBTReader(MemoryStream memIn, int version)
        {
            /*  Due to a file specification change on how an application reads a NBT file
             *  (Minecraft maps are now compressed via a z-lib deflate stream), this method
             *  provides backwards support for the old GZip decompression stream (in case for raw NBT files
             *  and old Minecraft chunk files).
             */

            // meaning the NBT is compressed via a GZip stream
            if (version == 1)
            {
                // decompress the stream
                GZipStream gStream = new GZipStream(memIn, CompressionMode.Decompress);

                // route the stream to a binary reader
                _bRead = new BinaryReader(memIn);
            }
            // meaning the NBT is compressed via a z-lib stream
            else if (version == 2)
            {
                // a known bug when deflating a zlib stream...
                // for more info, go here: http://www.chiramattel.com/george/blog/2007/09/09/deflatestream-block-length-does-not-match.html
                memIn.ReadByte();
                memIn.ReadByte();

                // deflate the stream
                DeflateStream dStream = new DeflateStream(memIn, CompressionMode.Decompress);

                // route the stream to a binary reader
                _bRead = new BinaryReader(dStream);
            }
        }


        /// <summary>
        /// Reads a single tag node in a NBT reader stream.
        /// </summary>
        /// <param name="inRead">The binary reader in which the NBT is found.</param>
        /// <param name="tagType">The type of data to be read.</param>
        /// <returns>Returns an object that corresponds to its TAG_TYPE (ex: int for TAG_INT, short for TAG_SHORT, etc.)</returns>
        public static dynamic Read(BinaryReader inRead, TagNodeType tagType)
        {
            /*  This method will read the payload of a tag node depending on the TAG_TYPE of the node.
             * 
             *  That is why this method returns a "dynamic" object because the final data type of the
             *  node will only be known during run-time.
             */

            try
            {
                // read the NBT stream depending on the tagType of the node
                switch (tagType)
                {
                    case TagNodeType.TAG_END:
                        {
                            return 0;
                        }
                    case TagNodeType.TAG_BYTE:
                        {
                            return inRead.ReadByte();
                        }
                    case TagNodeType.TAG_BYTEA:
                        {
                            return inRead.ReadBytes(Read(inRead, TagNodeType.TAG_INT));
                        }
                    case TagNodeType.TAG_SHORT:
                        {
                            return EndianConverter.SwapInt16(inRead.ReadInt16());
                        }
                    case TagNodeType.TAG_INT:
                        {
                            return EndianConverter.SwapInt32(inRead.ReadInt32());
                        }
                    case TagNodeType.TAG_LONG:
                        {
                            return EndianConverter.SwapInt64(inRead.ReadInt64());
                        }
                    case TagNodeType.TAG_SINGLE:
                        {
                            return EndianConverter.SwapSingle(inRead.ReadSingle());
                        }
                    case TagNodeType.TAG_DOUBLE:
                        {
                            return EndianConverter.SwapDouble(inRead.ReadDouble());
                        }
                    case TagNodeType.TAG_STRING:
                        {
                            return Encoding.UTF8.GetString(inRead.ReadBytes(Read(inRead, TagNodeType.TAG_SHORT)));
                        }
                    default:
                        {
                            throw new NotSupportedException("Tag type is not supported by this reader!");
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while reading the tag.\n\n");

                Console.WriteLine("Exception message: {0}\n\n", ex.Message);

                Console.WriteLine("Stack trace:\n\n{0}", ex.StackTrace);

                return null;
            }
        }


        private INBTTag readTagHead()
        {
            /*  As Notch says, a NBT file contains a TYPE, NAME, and PAYLOAD values. This method will read the
             *  first two values of a NBT node. The last one will be handled by readTagPlod() method.
             */

            // read the TAG_TYPE of the file
            byte _tagType = NBTReader.Read(this._bRead, TagNodeType.TAG_BYTE);
            string _tagName = null;

            // check whether the TAG_TYPE of the node is NOT a TAG_END type
            if (_tagType != (byte)TagNodeType.TAG_END)
            {
                // if no, then read the name of the node
                _tagName = NBTReader.Read(this._bRead, TagNodeType.TAG_STRING);
            }
            else
            {
                // if yes, then leave the name blank
                _tagName = "";
            }

            // proceed to reading the value (payload) of the node
            return readTagPlod((TagNodeType)_tagType, _tagName);
        }

        private INBTTag readTagPlod(TagNodeType type, string name)
        {
            /*  There are 3 types of nodes in a NBT file, namely TAG_LIST, TAG_COMPOUND and the base TAG_TYPEs.
             * 
             *  The difference between the data types and TAG_LIST and TAG_COMPOUND is both TAG_LIST and TAG_COMPOUND requires
             *  additional reading methodology to effectively read the whole file.
             *  
             *  First, on a TAG_LIST container type, the nodes are sequentially read WITHOUT the name tag because virtually, it is a
             *  custom data type array.
             *  
             *  Unlike TAG_LISTs, a TAG_COMPOUND container type requires the nodes to be read again by readTagHead() for n times
             *  (listing will only stop if it were to see a TAG_END node) because this container type contains heterogeneous
             *  mix of primitive data types.
             *  
             *  Lastly, if it is a base type data node, it will be directly read by the Read(BinaryReader, TagNodeType) method.
             * 
             *  In a nutshell, this method will read the value (payload) of a node depending on the type of the node. 
             */

            // check the tag type of the node
            switch (type)
            {
                // type is a TAG_LIST
                case TagNodeType.TAG_LIST:
                    {
                        // get the common TAG_TYPE of the list
                        byte _tagType = NBTReader.Read(this._bRead, TagNodeType.TAG_BYTE);
                        // then get the total number of items stored in that list
                        int _tagCout = NBTReader.Read(this._bRead, TagNodeType.TAG_INT);

                        // after getting those values, create a TagNodeList (basically a List) that will
                        // hold the succeeding tag values.
                        TagNodeList _assetsList = new TagNodeList(name, (TagNodeType)_tagType);

                        // loop-it according to the total count of the list
                        for (int i = 0; i < _tagCout; i++)
                        {
                            // read the data then immediately add it on the list
                            _assetsList.Add((INBTTag)readTagPlod((TagNodeType)_tagType, ""));
                        }

                        // finally, return _assetsList to the parent method
                        return _assetsList;
                    }
                // type is a TAG_COMPOUND
                case TagNodeType.TAG_COMPOUND:
                    {
                        // create a TagNodeList (basically a Dictionary) that will hold the succeeding tag values.
                        TagNodeListNamed _assetsMaps = new TagNodeListNamed(name);

                        // yes, this is an intentional infinite loop >:)
                        do
                        {
                            // read a tag node
                            INBTTag _nodeMap = readTagHead();

                            // if tag node is not TAG_END, meaning there is more to add
                            if (_nodeMap.Type != TagNodeType.TAG_END)
                            {
                                // add the _nodeMap into the list
                                _assetsMaps.Add(_nodeMap.Name, _nodeMap);
                            }
                            // otherwise
                            else
                            {
                                // break the loop *\o/*
                                break;
                            }
                        } while (true);

                        // return the list containing the newly read nodes to the parent method
                        return _assetsMaps;
                    }
                // tag is a primitive data type
                default:
                    {
                        // read the node according to the type of the node (the method Read() will handle the payload processing)
                        return new TagNode(type, name, Read(this._bRead, type));
                    }
            }
        }


        /// <summary>
        /// Starts the parsing/reading of the NBT file.
        /// </summary>
        /// <returns>Returns a INBTTAG that contains the contents of the NBT file (sequential order).</returns>
        public INBTTag BeginRead()
        {
            return this.readTagHead();
        }

        /// <summary>
        /// Disposes and finalizes the NBT reader.
        /// </summary>
        public void Dispose()
        {
            // finalize and dispose of _bRead (ooh bread! :P)

            _bRead.Close();
            _bRead.Dispose();
        }
    }
}
