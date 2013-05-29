using System.Collections.Generic;
using Chraft.PluginSystem.Entity;

namespace Chraft.PluginSystem.Item
{
    public interface IInventory
    {
        short ActiveSlot { get; }
        IEnumerable<IItemInventory> GetQuickSlots();

        void AddItem(short id, sbyte count, short durability, bool isInGame = true);
        void RemoveItem(short slot);
        IItemInventory GetActiveItem();
    }
}
