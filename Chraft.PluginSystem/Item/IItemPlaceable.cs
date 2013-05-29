using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;

namespace Chraft.PluginSystem.Item
{
    public interface IItemPlaceable
    {
        void Place(IStructBlock baseBlock, BlockFace face);
    }
}
