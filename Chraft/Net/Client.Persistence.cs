using Chraft.Properties;
using System.IO;
using System.Xml.Serialization;
using Chraft.Persistence;
using Chraft.Interfaces;

namespace Chraft.Net
{
    public partial class Client
    {
        private static XmlSerializer Xml = new XmlSerializer(typeof(ClientSurrogate));
        internal string Folder { get { return Settings.Default.PlayersFolder; } }
        internal string DataFile { get { return Folder + Path.DirectorySeparatorChar + _player.Username + ".xml"; } }
        // TODO: Move a bunch of this to DataFile.cs
        private void Load()
        {
            if (string.IsNullOrEmpty(_player.Username) && string.IsNullOrEmpty(_player.DisplayName)) { return; } //we are the server ping
            if (!File.Exists(DataFile))
                return;

            ClientSurrogate client;
            using (FileStream rx = File.OpenRead(DataFile))
                client = (ClientSurrogate)Xml.Deserialize(rx);
            _player.Position = new Chraft.World.AbsWorldCoords(client.X, client.Y + 1, client.Z);
            _player.Yaw = client.Yaw;
            _player.Pitch = client.Pitch;
            if (client.Inventory != null)
            {
                _player.Inventory = new Inventory {Handle = 0};
                ItemStack[] slots = new ItemStack[client.Inventory.SlotCount];

                for (short i = 0; i < client.Inventory.SlotCount; i++)
                {
                    slots[i] = ItemStack.Void;
                    if (!ItemStack.IsVoid(client.Inventory.Slots[i]))
                    {
                        slots[i].Type = client.Inventory.Slots[i].Type;
                        slots[i].Count = client.Inventory.Slots[i].Count;
                        slots[i].Durability = client.Inventory.Slots[i].Durability;
                        slots[i].Slot = i;
                    }
                    // Using the default indexer on Inventory ensures all event handlers are correctly hooked up
                    _player.Inventory[i] = slots[i];
                }
                _player.Inventory.Associate(_player);
            }
            _player.GameMode = client.GameMode;
            _player.Health = client.Health;
            _player.Food = client.Food;
            _player.FoodSaturation = client.FoodSaturation;
            _player.DisplayName = client.DisplayName;
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(_player.Username) && string.IsNullOrEmpty(_player.DisplayName)) { return;} //we are the server ping
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            string file = DataFile + ".tmp";

            try
            {
                using (FileStream tx = File.Create(file))
                {
                    Xml.Serialize(tx, new ClientSurrogate
                    {
                        Inventory = _player.Inventory,
                        X = _player.Position.X,
                        Y = _player.Position.Y,
                        Z = _player.Position.Z,
                        Yaw = _player.Yaw,
                        Pitch = _player.Pitch,
                        GameMode = _player.GameMode,
                        DisplayName = string.IsNullOrEmpty(_player.DisplayName) ? _player.Username : _player.DisplayName ,
                        Health = _player.Health,
                        Food = _player.Food,
                        FoodSaturation = _player.FoodSaturation
                    });
                    tx.Flush();
                    tx.Close();
                }
            }
            catch (IOException)
            {
                return;
            }

            if (File.Exists(DataFile))
                File.Delete(DataFile);
            File.Move(file, DataFile);
        }

    }
}
