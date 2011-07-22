using System;

namespace Chraft
{
	public class ClientEventArgs : EventArgs
	{
		public Client Client { get; private set; }

		internal ClientEventArgs(Client client)
		{
			Client = client;
		}
	}
}
