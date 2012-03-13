using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utilities;

namespace Chraft.PluginSystem.Blocks
{
    public interface IStructBlock
    {
        byte Type { get; set; }
        UniversalCoords Coords { get; set; }
        byte MetaData { get; set; }
        IWorldManager WorldInterface { get; }
        string ToString();
    }
}
