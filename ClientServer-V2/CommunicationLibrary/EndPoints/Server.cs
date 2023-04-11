using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace CommunicationLibrary.EndPoints
{
	using System;
	using System.Collections.ObjectModel;

	using CommunicationLibrary.Logic;

	using Logic;

	using Models;
	using Models.Features;

	public class Server<TPacketFlags>
		where TPacketFlags :
					struct, Enum
	{
		private TcpListener server;
		private IPAddress ipAddress;
		private int port;
		public bool IsRunning => isRunning;
		private bool isRunning;

		private TextWriter textWriter;

		public Action<Packet> OnPacketRecieved;

		public event EventHandler<ClientEventArgs> ClientConnected;
		public event EventHandler<ClientEventArgs> ClientDisconnected;

		private IHandler<TPacketFlags> BaseHandler;

		public Server(TextWriter textWriter, IHandler<TPacketFlags> handler)
		{
			this.textWriter = textWriter;
			this.BaseHandler = handler;
		}

		public void Start(string ipAddress, int port)
		{
			Start(IPAddress.Parse(ipAddress), port);
		}

		public void Start(IPAddress ipAddress, int port)
		{
			this.ipAddress = ipAddress;
			this.port = port;
			server = new TcpListener(this.ipAddress, this.port);
			server.Start();
			isRunning = true;

			LoopClients();
		}

		public void Stop()
		{
			server.Stop();
			isRunning = false;
		}


		public void LoopClients()
		{
			while (isRunning)
			{
				// wait for client connection
				TcpClient newClient = server.AcceptTcpClient();

				// client found.
				// create a thread to handle communication
				//Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
				Thread t = new Thread(new ParameterizedThreadStart(HandleClientPackets));
				t.Start(newClient);
			}
		}

		private void HandleClientPackets(object obj)
		{
			// retrieve client from parameter passed to thread
			TcpClient client = (TcpClient)obj;
			NetworkStream network = client.GetStream();
			bool bClientConnected = true;

			RaiseClientConnected(client);
			try
			{

				Packet packet, response;
#warning Write encoding handshake hahaha
				while (bClientConnected)
				{
					packet = PacketBuilder.GetPacketFromNetworkStream(network);

					response = BaseHandler.Handle(packet);
					OnPacketRecieved(packet);

					network.Write(
						response.ToByteArray(),
						0,
						Packet.__HeaderSize__ + response.Size);
					network.Flush();
				}
			}
			catch (IOException)
			{
				RaiseClientDisconnected(client);
			}
			catch (Exception)
			{
				throw new NotImplementedException();
			}
		}

		private void RaiseClientConnected(TcpClient client)
		{
			var handler = ClientConnected;
			if (handler != null)
			{
				handler(this, new ClientEventArgs(client));
			}
		}

		private void RaiseClientDisconnected(TcpClient client)
		{
			var handler = ClientDisconnected;
			if (handler != null)
			{
				handler(this, new ClientEventArgs(client));
			}
		}
	}
}
