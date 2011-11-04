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
