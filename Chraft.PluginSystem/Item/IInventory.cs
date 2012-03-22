using System.Collections.Generic;

namespace Chraft.PluginSystem.Item
{
    public interface IInventory
    {
        short ActiveSlot { get; }
        IEnumerable<IItemStack> GetQuickSlots();

        void AddItem(short id, sbyte count, short durability, bool isInGame = true);
        void RemoveItem(short slot);
        bool DamageItem(short slot, short damageAmount = 1);
        IItemStack GetActiveItem();
    }
}
