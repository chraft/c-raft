
namespace Chraft.PluginSystem.Item
{
    public interface IItemDb
    {
        IItemInventory GetItem(string item);
        bool Contains(string item);

        string ItemName(short item);
    }
}