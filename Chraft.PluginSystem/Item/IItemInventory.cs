using Chraft.PluginSystem.World.Blocks;

namespace Chraft.PluginSystem.Item
{
    public interface IItemInventory
    {
        short Type { get; }
        sbyte Count { get; }
        short Durability { get; }
        short Slot { get; }
        void DestroyBlock(IStructBlock block);
        void DamageItem(IStructBlock block);
        void DamageItem(short value);
        short GetDamage();
    }
}
