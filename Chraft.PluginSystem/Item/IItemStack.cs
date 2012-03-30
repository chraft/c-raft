namespace Chraft.PluginSystem.Item
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
