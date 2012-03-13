using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.PluginSystem
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
