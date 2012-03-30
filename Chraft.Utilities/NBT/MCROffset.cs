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

namespace Chraft.Utilities.NBT
{
    /// <summary>
    /// Indicates the location of a chunk in a region file.
    /// </summary>
    public struct McrOffset
    {
        /// <summary>
        /// The sector offset of the chunk.
        /// </summary>
        public int SectorOffset { get; set; }

        /// <summary>
        /// The sector count of a chunk. A one sector is equal to 1024 bytes.
        /// </summary>
        public byte SectorSize { get; set; }
    }
}
