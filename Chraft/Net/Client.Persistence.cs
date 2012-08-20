#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System.IO;
using System.Xml.Serialization;
using Chraft.Persistence;
using Chraft.Interfaces;
using Chraft.PluginSystem;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Config;
using Chraft.World;

namespace Chraft.Net
{
    public partial class Client
    {
        private static XmlSerializer Xml = new XmlSerializer(typeof(ClientSurrogate));
        internal string Folder { get { return ChraftConfig.PlayersFolder; } }
        internal string DataFile { get { return Folder + Path.DirectorySeparatorChar + Username + ".xml"; } }
        // TODO: Move a bunch of this to DataFile.cs
        private void Load()
        {
            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(_player.DisplayName)) { return; } //we are the server ping
            if (!File.Exists(DataFile))
            {
                _player.Position = new AbsWorldCoords(Owner.World.Spawn.WorldX, Owner.World.Spawn.WorldY, Owner.World.Spawn.WorldZ);
                WaitForInitialPosAck = true;
                _player.LoginPosition = _player.Position;
                return;
            }

            ClientSurrogate client;
            using (FileStream rx = File.OpenRead(DataFile))
                client = (ClientSurrogate)Xml.Deserialize(rx);
            _player.Position = new AbsWorldCoords(client.X, client.Y, client.Z);
            _player.Yaw = client.Yaw;
            _player.Pitch = client.Pitch;
            if (client.Inventory != null)
            {
                _player.Inventory = new Inventory {Handle = 0};
                ItemStack[] slots = new ItemStack[client.Inventory.SlotCount];

                for (short i = 0; i < client.Inventory.SlotCount; i++)
                {
                    slots[i] = ItemStack.Void;
                    if (client.Inventory.Slots[i] != null && !client.Inventory.Slots[i].IsVoid())
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
            WaitForInitialPosAck = true;
            _player.LoginPosition = _player.Position;
            CurrentSightRadius = client.SightRadius;
        }

        internal void Save()
        {
            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(_player.DisplayName)) { return;} //we are the server ping
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
                        DisplayName = string.IsNullOrEmpty(_player.DisplayName) ? Username : _player.DisplayName ,
                        Health = _player.Health,
                        Food = _player.Food,
                        FoodSaturation = _player.FoodSaturation,
                        SightRadius = CurrentSightRadius
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
