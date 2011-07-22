using System;
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
		internal string DataFile { get { return Path.Combine(Folder, Username + ".xml"); } }

		private void Load()
		{
			if (!File.Exists(DataFile))
				return;

			ClientSurrogate client;
			using (FileStream rx = File.OpenRead(DataFile))
				client = (ClientSurrogate)Xml.Deserialize(rx);
			this.X = client.X;
			this.Y = client.Y + 1; // Players drop one block upon spawning
			this.Z = client.Z;
			this.Yaw = client.Yaw;
			this.Pitch = client.Pitch;
			this.Inventory = client.Inventory;
			this.Inventory.Associate(this);
		}

		/// <summary>
		/// Saves the player file.  Not thread-safe.
		/// </summary>
		public void Save()
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
						X = X,
						Y = Y,
						Z = Z,
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

	    public void setSleepingIgnored(bool b)
	    {
	        throw new NotImplementedException();
	    }

	    public bool isSleepingIgnored()
	    {
	        throw new NotImplementedException();
	    }
	}
}
