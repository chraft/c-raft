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
using System.Collections.Generic;
using Chraft.Utils.NBT;

namespace NBT.Tag
{
    public enum MatertialsType
    {
        CLASSIC,
        ALPHA
    }

    public struct SCHEMATICTag
    {
        short                   x;
        short                   z;
        short                   y;

        public short            X
        {
            get { return this.x; }
		}
        public short            Z
        {
            get { return this.z; }
        }
        public short            Y
        {
            get { return this.y; }
        }

        MatertialsType          material;

        public MatertialsType   MaterialsType
        {
            get { return this.material; }
            set { this.material = value; }
        }

        byte[]                  blocks;
        byte[]                  data;

        public byte[]           Blocks
        {
            get { return this.blocks; }
            set { this.blocks = value; }
        }
        public byte[]           Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

        List<NBTTag>            entities;
        List<NBTTag>            tileents;

        public List<NBTTag>     Entities
        {
            get { return this.entities; }
            set { this.entities = value; }
        }
        public List<NBTTag>     TileEntities
        {
            get { return this.tileents; }
            set { this.tileents = value; }
        }
    }
}
