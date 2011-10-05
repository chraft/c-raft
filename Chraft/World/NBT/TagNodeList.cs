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
using System.Collections.Generic;

namespace Chraft.World.NBT
{
    /// <summary>
    /// Represents a sequential unnamed custom TAG_TYPE list.
    /// </summary>
    public class TagNodeList : List<INBTTag>, INBTTag
    {
        private string _name;

        /// <summary>
        /// Gets or sets the name of the List.
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }


        private TagNodeType _cType;

        /// <summary>
        /// Gets the TAG_TYPE of the items that the list currently holds.
        /// </summary>
        public TagNodeType ChildType
        {
            get { return this._cType; }
        }


        /// <summary>
        /// Gets the value (payload) of the list.
        /// </summary>
        public dynamic Payload
        {
            get
            {
                return this;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Simply returns a TAG_LIST when called.
        /// </summary>
        public TagNodeType Type
        {
            get
            {
                return TagNodeType.TAG_LIST;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Creates a new TagNodeList.
        /// </summary>
        /// <param name="name">The name of the list.</param>
        /// <param name="contents">The TAG_TYPE of the items currently hold.</param>
        public TagNodeList(string name, TagNodeType contents)
            : base()
        {
            this._name = name;
            this._cType = contents;
        }
    }
}
