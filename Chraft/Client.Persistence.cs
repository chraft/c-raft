using Chraft.Properties;
using System.IO;
using System.Xml.Serialization;
using Chraft.Persistence;
using Chraft.Interfaces;

namespace Chraft
{
    public partial class Client
    {
        private static XmlSerializer Xml = new XmlSerializer(typeof(ClientSurrogate));
        internal string Folder { get { return Settings.Default.PlayersFolder; } }
        internal string DataFile { get { return Folder + Path.DirectorySeparatorChar + Username + ".xml"; } }
        // TODO: Move a bunch of this to DataFile.cs
        private void Load()
        {
            if (!File.Exists(DataFile))
                return;

            ClientSurrogate client;
            using (FileStream rx = File.OpenRead(DataFile))
                client = (ClientSurrogate)Xml.Deserialize(rx);
            Position.X = client.X;
            Position.Y = client.Y + 1; // Players drop one block upon spawning
            Position.Z = client.Z;
            Position.Yaw = client.Yaw;
            Position.Pitch = client.Pitch;
            if (client.Inventory == null) return;
            Inventory = new Inventory {Handle = 0};
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
                this.Inventory[i] = slots[i];
            }
            Inventory.Associate(this);
            GameMode = client.GameMode;
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
                        Inventory = Inventory,
                        X = Position.X,
                        Y = Position.Y,
                        Z = Position.Z,
                        Yaw = Position.Yaw,
                        Pitch = Position.Pitch,
                        GameMode = GameMode 
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
