using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Persistence;
using Chraft.Utilities.NBT;
using Chraft.Utilities.Config;

namespace Chraft.Utils
{
    public class PlayerNBTConverter
    {
        /// <summary>
        /// Converts a Minecraft NBT format player file to c#raft xml
        /// </summary>
        /// <param name="fileName">Filepath of nbt</param>
        internal void ConvertPlayerNBT(string fileName)
        {
            FileStream s = null;
            NBTFile nbt = null;
            try
            {
                ClientSurrogate p = new ClientSurrogate();
                s = new FileStream(fileName, FileMode.Open);
                nbt = NBTFile.OpenFile(s, 1);
                foreach (KeyValuePair<string, NBTTag> sa in nbt.Contents)
                {
                    switch (sa.Key)
                    {
                        case "Health":
                            p.Health = sa.Value.Payload;
                            break;
                        case "Pos":
                            p.X = sa.Value.Payload[2].Payload;
                            p.Y = sa.Value.Payload[1].Payload;
                            p.Z = sa.Value.Payload[0].Payload;
                            break;
                        case "Rotation":
                            p.Pitch = sa.Value.Payload[1].Payload;
                            p.Yaw = sa.Value.Payload[0].Payload;
                            break;
                        case "playerGameType":
                            p.GameMode = (byte)sa.Value.Payload;
                            break;
                        case "foodLevel":
                            p.Food = (short)sa.Value.Payload;
                            break;
                        case "foodSaturationLevel":
                            p.FoodSaturation = sa.Value.Payload;
                            break;
                        case "Inventory":
                            Inventory inv = new Inventory();
                            foreach (NBTTag tag in sa.Value.Payload)
                            {
                                inv.AddItem((short)tag.Payload["id"].Payload, (sbyte)tag.Payload["Count"].Payload,
                                            (short)tag.Payload["Damage"].Payload, false);
                            }
                            p.Inventory = inv;
                            break;
                    }
                }
                SavePlayerXml(p, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error converting file" + fileName + " to C#raft format");
                Console.WriteLine(ex);
            }
            finally
            {
                if (s != null)
                    s.Dispose();
                if (nbt != null)
                    nbt.Dispose();
            }
        }

        private void SavePlayerXml(ClientSurrogate cs, string fileName)
        {
            XmlSerializer xml = new XmlSerializer(typeof(ClientSurrogate));
            string folder = ChraftConfig.PlayersFolder;
            string dataFile = folder + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileName) + ".xml";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string file = dataFile + ".tmp";
            try
            {
                using (FileStream tx = File.Create(file))
                {
                    xml.Serialize(tx, cs);
                    tx.Flush();
                    tx.Close();
                }
            }
            catch (IOException)
            {
                return;
            }
            if (File.Exists(dataFile))
                File.Delete(dataFile);
            File.Move(file, dataFile);
            File.Move(fileName, Path.ChangeExtension(fileName, ".conv"));
        }

    }
}

