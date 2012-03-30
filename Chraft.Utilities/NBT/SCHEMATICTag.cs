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

using System.Collections.Generic;

namespace Chraft.Utilities.NBT
{
    public enum MatertialsType
    {
        CLASSIC,
        ALPHA
    }

    public struct SCHEMATICTag
    {
        public short X { get; private set; }

        public short Z { get; private set; }

        public short Y { get; private set; }

        public MatertialsType MaterialsType { get; set; }

        public byte[] Blocks { get; set; }

        public byte[] Data { get; set; }

        public List<NBTTag> Entities { get; set; }

        public List<NBTTag> TileEntities { get; set; }
    }
}
