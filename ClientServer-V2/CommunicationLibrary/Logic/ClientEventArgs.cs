using System.Net.Sockets;
using System;

namespace CommunicationLibrary.Logic
{
	public class ClientEventArgs : EventArgs
	{
		public TcpClient Client { get; private set; }

		public ClientEventArgs(TcpClient client)
		{
			Client = client;
		}
	}
}
