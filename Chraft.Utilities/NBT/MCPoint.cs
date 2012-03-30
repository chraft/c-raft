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
    /// Defines a chunk point in a region file.
    /// </summary>
    public struct MCPoint
    {
        /// <summary>
        /// The x-location of the chunk.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The y-location of the chunk.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Creates a new point.
        /// </summary>
        /// <param name="x">The x-location.</param>
        /// <param name="y">The y-location.</param>
        public MCPoint(int x, int y) : this()
        {
            X = x;
            Y = y;
        }
    }
}
