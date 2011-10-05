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
    /// Identifier for a NBT node.
    /// </summary>
    public interface INBTTag
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The payload (value) of the node.
        /// </summary>
        dynamic Payload { get; set; }

        /// <summary>
        /// The tag type of the node.
        /// </summary>
        TagNodeType Type { get; set; }
    }
}
