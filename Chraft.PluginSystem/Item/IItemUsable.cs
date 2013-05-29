using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;

namespace Chraft.PluginSystem.Item
{
    public interface IItemUsable
    {
        void Use(IStructBlock baseBlock, BlockFace face);
    }
}
