#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;

namespace Chraft.World.NBT
{
    /// <summary>
    /// Represents a sequential named custom TAG_TYPE list.
    /// </summary>
    public class TagNodeListNamed : Dictionary<string, INBTTag>, INBTTag
    {
        private string _name;

        /// <summary>
        /// Gets the name of the list.
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
        /// Simply returns a TAG_COMPOUND when called.
        /// </summary>
        public TagNodeType Type
        {
            get
            {
                return TagNodeType.TAG_COMPOUND;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Creates a new TagNodeListNamed.
        /// </summary>
        /// <param name="name">The name of the list.</param>
        public TagNodeListNamed(string name)
            : base()
        {
            this._name = name;
        }
    }
}
