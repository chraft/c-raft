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
    /// Represents a named tag node in a NBT file.
    /// </summary>
    public class TagNode : INBTTag
    {
        private string _name;

        /// <summary>
        /// Gets or sets the name for this node.
        /// </summary>
        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }


        private dynamic _payload;

        /// <summary>
        /// Gets or sets the value (payload) of this node.
        /// </summary>
        public dynamic Payload
        {
            get { return this._payload; }
            set { this._payload = value; }
        }


        private TagNodeType _type;

        /// <summary>
        /// Gets or sets the TAG_TYPE for this node.
        /// </summary>
        public TagNodeType Type
        {
            get { return this._type; }
            set { this._type = value; }
        }


        /// <summary>
        /// Creates a new tag node.
        /// </summary>
        /// <param name="type">The TAG_TYPE of the node.</param>
        /// <param name="name">The name of the node.</param>
        /// <param name="payload">The value (payload) of the node.</param>
        public TagNode(TagNodeType type, string name, dynamic payload)
        {
            this._type = type;
            this._name = name;
            this._payload = payload;
        }


        /// <summary>
        /// Returns a System.String that represents this TagNode.
        /// </summary>
        /// <returns>The System.String that represents the current TagNode.</returns>
        public override string ToString()
        {
            return string.Format("{0} has tag type of {1} with value {2}", this._name, this._type, this._payload);
        }
    }
}
