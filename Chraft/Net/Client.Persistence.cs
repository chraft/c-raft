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
        internal string DataFile { get { return Folder + Path.DirectorySeparatorChar + _Player.Username + ".xml"; } }
        // TODO: Move a bunch of this to DataFile.cs
        private void Load()
        {
            if (!File.Exists(DataFile))
                return;

            ClientSurrogate client;
            using (FileStream rx = File.OpenRead(DataFile))
                client = (ClientSurrogate)Xml.Deserialize(rx);
            _Player.Position.X = client.X;
            _Player.Position.Y = client.Y + 1; // Players drop one block upon spawning
            _Player.Position.Z = client.Z;
            _Player.Position.Yaw = client.Yaw;
            _Player.Position.Pitch = client.Pitch;
            if (client.Inventory != null)
            {
                _Player.Inventory = new Inventory {Handle = 0};
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
                    _Player.Inventory[i] = slots[i];
                }
                _Player.Inventory.Associate(_Player);
            }
            _Player.GameMode = client.GameMode;
            _Player.Health = client.Health;
            _Player.Food = client.Food;
            _Player.FoodSaturation = client.FoodSaturation;
            _Player.DisplayName = client.DisplayName;
        }

        private void Save()
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            string file = DataFile + ".tmp";

            try
            {
                using (FileStream tx = File.Create(file))
                {
                    Xml.Serialize(tx, new ClientSurrogate
                    {
                        Inventory = _Player.Inventory,
                        X = _Player.Position.X,
                        Y = _Player.Position.Y,
                        Z = _Player.Position.Z,
                        Yaw = _Player.Position.Yaw,
                        Pitch = _Player.Position.Pitch,
                        GameMode = _Player.GameMode,
                        DisplayName = _Player.DisplayName,
                        Health = _Player.Health,
                        Food = _Player.Food,
                        FoodSaturation = _Player.FoodSaturation
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
