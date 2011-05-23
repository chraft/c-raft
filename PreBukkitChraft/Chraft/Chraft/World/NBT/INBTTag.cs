using System;

namespace Chraft.World.NBT
{
    /// <summary>
    /// Identifier for a NBT node.
    /// </summary>
    public interface INBTTag
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The payload (value) of the node.
        /// </summary>
        dynamic Payload { get; set; }

        /// <summary>
        /// The tag type of the node.
        /// </summary>
        TagNodeType Type { get; set; }
    }
}
