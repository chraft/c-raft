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
