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

using System;
using System.IO;
using System.Xml;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Interfaces;
using Chraft.PluginSystem.Entity;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Config;

namespace Chraft.Net
{
    public partial class Client
    {
        internal string Folder { get { return ChraftConfig.PlayersFolder; } }
        internal string DataFile { get { return Folder + Path.DirectorySeparatorChar + Username + ".xml"; } }

        // TODO: Move a bunch of this to DataFile.cs
        private void Load()
        {
            // We are the server ping
            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(_player.DisplayName)) { return; }

            if (!File.Exists(DataFile))
            {
                _player.Position = new AbsWorldCoords(Owner.World.Spawn.WorldX, Owner.World.Spawn.WorldY, Owner.World.Spawn.WorldZ);
                WaitForInitialPosAck = true;
                _player.LoginPosition = _player.Position;
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(DataFile);

            double x, y, z, yaw, pitch;
            short health, food;
            float foodSaturation;
            GameMode gameMode;
            string displayName;
            int sightRadius, experience = 0;

            var playerNode = doc["Player"];

            if (playerNode == null)
                return;

            x = double.Parse(playerNode["X"].InnerText);
            y = double.Parse(playerNode["Y"].InnerText);
            z = double.Parse(playerNode["Z"].InnerText);
            yaw = double.Parse(playerNode["Yaw"].InnerText);
            pitch = double.Parse(playerNode["Pitch"].InnerText);
            health = short.Parse(playerNode["Health"].InnerText);
            food = short.Parse(playerNode["Food"].InnerText);
            foodSaturation = float.Parse(playerNode["FoodSaturation"].InnerText);
            gameMode = (GameMode)byte.Parse(playerNode["GameMode"].InnerText);
            displayName = playerNode["DisplayName"].InnerText;
            sightRadius = int.Parse(playerNode["SightRadius"].InnerText);
            if (playerNode["Experience"] != null)
                experience = int.Parse(playerNode["Experience"].InnerText);

            _player.Position = new AbsWorldCoords(x, y, z);
            _player.Yaw = yaw;
            _player.Pitch = pitch;
            _player.Health = health;
            _player.Food = food;
            _player.FoodSaturation = foodSaturation;
            _player.GameMode = gameMode;
            _player.DisplayName = displayName;
            _player.Experience = Math.Max(experience, 0);
            CurrentSightRadius = sightRadius;
            WaitForInitialPosAck = true;
            _player.LoginPosition = _player.Position;

            _player.Inventory = new Inventory { Handle = 0 };

            short slot, type, durability, count;

            for (short i = 0; i < 45; i++)
                _player.Inventory[i] = ItemHelper.Void;

            foreach (XmlNode itemXml in playerNode["Inventory"].ChildNodes)
            {
                slot = short.Parse(itemXml.Attributes["Slot"].InnerText);
                type = short.Parse(itemXml.Attributes["Type"].InnerText);
                durability = short.Parse(itemXml.Attributes["Durability"].InnerText);
                count = short.Parse(itemXml.Attributes["Count"].InnerText);
                var item = ItemHelper.GetInstance(type);
                item.Count = (sbyte)count;
                item.Durability = durability;
                _player.Inventory[slot] = item;
            }

            _player.Inventory.Associate(_player);
        }

        internal void Save()
        {
            // We are the server ping
            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(_player.DisplayName))
                return;

            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            string file = DataFile + ".tmp";

            try
            {
                var doc = new XmlDocument();

                var dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(dec);
                var root = doc.CreateElement("Player");

                var arg = doc.CreateElement("X");
                arg.InnerText = _player.Position.X.ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("Y");
                arg.InnerText = _player.Position.Y.ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("Z");
                arg.InnerText = _player.Position.Z.ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("Yaw");
                arg.InnerText = _player.Yaw.ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("Pitch");
                arg.InnerText = _player.Pitch.ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("Health");
                arg.InnerText = _player.Health.ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("Food");
                arg.InnerText = _player.Food.ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("FoodSaturation");
                arg.InnerText = _player.FoodSaturation.ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("GameMode");
                arg.InnerText = ((byte)_player.GameMode).ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("DisplayName");
                arg.InnerText = string.IsNullOrEmpty(_player.DisplayName) ? Username : _player.DisplayName;
                root.AppendChild(arg);
                arg = doc.CreateElement("SightRadius");
                arg.InnerText = CurrentSightRadius.ToString();
                root.AppendChild(arg);
                arg = doc.CreateElement("Experience");
                arg.InnerText = _player.Experience.ToString();
                root.AppendChild(arg);

                XmlElement inventoryNode = doc.CreateElement("Inventory");
                ItemInventory item;
                XmlElement itemDoc;

                for (short i = 5; i <= 44; i++)
                {
                    if (_player.Inventory[i] == null || ItemHelper.IsVoid(_player.Inventory[i]))
                        continue;
                    item = _player.Inventory[i];
                    itemDoc = doc.CreateElement("Item");
                    itemDoc.SetAttribute("Slot", i.ToString());
                    itemDoc.SetAttribute("Type", item.Type.ToString());
                    itemDoc.SetAttribute("Count", item.Count.ToString());
                    itemDoc.SetAttribute("Durability", item.Durability.ToString());
                    inventoryNode.AppendChild(itemDoc);
                }
                root.AppendChild(inventoryNode);
                doc.AppendChild(root);

                doc.Save(file);
            }
            catch (Exception)
            {
                return;
            }

            if (File.Exists(DataFile))
                File.Delete(DataFile);
            File.Move(file, DataFile);
        }
    }
}
