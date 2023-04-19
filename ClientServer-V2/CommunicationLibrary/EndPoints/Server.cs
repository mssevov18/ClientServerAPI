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
		where TPacketFlags : struct, Enum
	{
		private TcpListener _server;
		private IPAddress _ipAddress;
		private int _port;
		public bool IsRunning => _isRunning;
		private bool _isRunning;

		private TextWriter _textWriter;

		public Action<Packet> OnPacketRecieved;

		public event EventHandler<ClientEventArgs> OnClientConnected;
		public event EventHandler<ClientEventArgs> OnClientDisconnected;

		private IHandler<TPacketFlags> _handler;

		public Server(TextWriter textWriter, IHandler<TPacketFlags> handler)
		{
			this._textWriter = textWriter;
			this._handler = handler;
		}

		public void Start(string ipAddress, int port)
		{
			Start(IPAddress.Parse(ipAddress), port);
		}
		public void Start(IPAddress ipAddress, int port)
		{
			this._ipAddress = ipAddress;
			this._port = port;
			_server = new TcpListener(this._ipAddress, this._port);
			_server.Start();
			_isRunning = true;

			LoopClients();
		}

		public void Stop()
		{
			_server.Stop();
			_isRunning = false;
		}

		public void LoopClients()
		{
			while (_isRunning)
			{
				// wait for client connection
				TcpClient newClient = _server.AcceptTcpClient();

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

					response = _handler.Handle(packet);
					if (OnPacketRecieved != null)
						OnPacketRecieved(packet);

					network.Write(
						response.ToByteArray(),
						0,
						Packet.HeaderSize + response.Size);
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
			var handler = OnClientConnected;
			if (handler != null)
			{
				handler(this, new ClientEventArgs(client));
			}
		}

		private void RaiseClientDisconnected(TcpClient client)
		{
			var handler = OnClientDisconnected;
			if (handler != null)
			{
				handler(this, new ClientEventArgs(client));
			}
		}
	}
}
