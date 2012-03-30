
namespace Chraft.PluginSystem.Item
{
    public interface IItemDb
    {
        IItemStack GetItemStack(string item);
        bool Contains(string item);

        string ItemName(short item);
    }
}