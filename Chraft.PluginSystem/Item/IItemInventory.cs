namespace Chraft.PluginSystem.Item
{
    public interface IItemInventory
    {
        short Type { get; }
        sbyte Count { get; }
        short Durability { get; }
        short Slot { get; }
    }
}
