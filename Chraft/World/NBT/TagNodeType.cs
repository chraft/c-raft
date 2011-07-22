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
