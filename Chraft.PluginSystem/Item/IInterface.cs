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
        IItemStack GetItem(int slot);
        void SetItem(int slot, IItemStack newItem);
        IItemStack[] GetSlots();
        IItemStack GetCursor();
    }
}