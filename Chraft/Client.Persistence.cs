using Chraft.Properties;
using System.IO;
using System.Xml.Serialization;
using Chraft.Persistence;

namespace Chraft
{
	public partial class Client
	{
		private static XmlSerializer Xml = new XmlSerializer(typeof(ClientSurrogate));
        internal string Folder { get { return Settings.Default.PlayersFolder; } }
		internal string DataFile { get { return Folder + "/" + Username + ".xml"; } }
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
			Yaw = client.Yaw;
			Pitch = client.Pitch;
		    if (client.Inventory == null) return;
		    Inventory = client.Inventory;
		    Inventory.Associate(this);
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
						Yaw = Yaw,
						Pitch = Pitch
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
