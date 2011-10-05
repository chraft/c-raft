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

namespace Chraft.World.NBT
{
    /// <summary>
    /// Provides the basic TAG_TYPE for a node.
    /// </summary>
    public enum TagNodeType : int
    {
        /// <summary>
        /// Empty tag
        /// </summary>
        TAG_END,
        /// <summary>
        /// Byte tag
        /// </summary>
        TAG_BYTE,
        /// <summary>
        /// Short integer tag
        /// </summary>
        TAG_SHORT,
        /// <summary>
        /// Normal integer tag
        /// </summary>
        TAG_INT,
        /// <summary>
        /// Large integer tag
        /// </summary>
        TAG_LONG,
        /// <summary>
        /// Single precision floating-point tag
        /// </summary>
        TAG_SINGLE,
        /// <summary>
        /// Double precision floating-point tag
        /// </summary>
        TAG_DOUBLE,
        /// <summary>
        /// Byte array tag
        /// </summary>
        TAG_BYTEA,
        /// <summary>
        /// String tag
        /// </summary>
        TAG_STRING,
        /// <summary>
        /// Unnamed, custom type array tag
        /// </summary>
        TAG_LIST,
        /// <summary>
        /// Named, custom type array tag
        /// </summary>
        TAG_COMPOUND,
    }
}
