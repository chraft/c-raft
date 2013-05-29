using Chraft.PluginSystem.Entity;
using Chraft.Utilities.Coords;

namespace Chraft.PluginSystem.Item
{
    public interface IInterface
    {
        short SlotCount { get; }
        string Title { get; set; }
        bool IsOpen { get; }
        void Open();
        void UpdateClient();
        void UpdateCursor();
        void Close(bool sendCloseToClient);
        void DropAll(UniversalCoords coords);
        bool IsEmpty();
        IItemInventory GetItem(short slot);
        void SetItem(short slot, IItemInventory newItem);
        IItemInventory[] GetSlots();
        IItemInventory GetCursor();
        IPlayer GetPlayer();
    }
}