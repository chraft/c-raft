using Chraft.Utilities.Coords;

namespace Chraft.PluginSystem.World.Blocks
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
