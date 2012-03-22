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

using System;
using System.Collections.Generic;

namespace Chraft.Utilities.NBT
{
    /// <summary>
    /// A NBT tag, the building blocks of a NBT file.
    /// </summary>
    public struct NBTTag
    {
        /// <summary>
        /// The payload of this tag.
        /// </summary>
        public dynamic Payload { get; private set; }

        /// <summary>
        /// The tag type of this tag.
        /// </summary>
        public byte Type { get; private set; }

        /// <summary>
        /// The name of this tag.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Creates a new NBT tag.
        /// </summary>
        /// <param name="name">The name of the tag.</param>
        /// <param name="type">The type of the tag.</param>
        /// <param name="payload">The payload of the tag.</param>
        public NBTTag(string name, byte type, dynamic payload) : this()
        {
            bool error = false;

            switch (type)
            {
                case 1:
                    if (!(payload is byte))
                        error = true;
                    break;
                case 2:
                    if (!(payload is short))
                        error = true;
                    break;
                case 3:
                    if (!(payload is int))
                        error = true;
                    break;
                case 4:
                    if (!(payload is long))
                        error = true;
                    break;
                case 5:
                    if (!(payload is float))
                        error = true;
                    break;
                case 6:
                    if (!(payload is double))
                        error = true;
                    break;
                case 7:
                    if (!(payload is byte[]))
                        error = true;
                    break;
                case 8:
                    if (!(payload is string))
                        error = true;
                    break;
                case 9:
                    if (!(payload is List<NBTTag>))
                        error = true;
                    break;
                case 10:
                    if (!(payload is Dictionary<string, NBTTag>))
                        error = true;
                    break;
            }

            if (error)
                throw new InvalidCastException("Wrong type used on tag payload!");

            Payload = payload;

            Type = type;
            Name = name;
        }

        /// <summary>
        /// Converts this tag to a human readable string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string payloadValue = Payload.ToString();

            if (Payload is List<NBTTag> || Payload is Dictionary<string, NBTTag>)
            {
                if (Payload is List<NBTTag>)
                {
                    payloadValue = "list ";
                }
                else
                {
                    payloadValue = "cmpd ";
                }

                payloadValue += "items: " + Payload.Count;
            }

            return string.Format("name: {0}, value: {1}", Name, payloadValue);
        }
    }
}
