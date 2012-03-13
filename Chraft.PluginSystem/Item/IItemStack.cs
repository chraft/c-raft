using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.PluginSystem
{
    public interface IItemStack
    {
        short Type { get; }
        sbyte Count { get; }
        short Durability { get; }
        short Slot { get; }
        bool IsVoid();
    }
}
