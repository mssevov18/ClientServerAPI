using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace CommunicationLibrary.EndPoints
{
	using Logic;
	using Models;
	using Models.Features;

	public class Server
	{
		private TcpListener server;
		private IPAddress ipAddress;
		private int port;
		public bool IsRunning => isRunning;
		private bool isRunning;

		private TextWriter textWriter;
		private Encoding encoding;

		private IHandler handler;

		public Server(TextWriter textWriter, IHandler handler)
		{
			this.textWriter = textWriter;
			this.handler = handler;
			this.encoding = handler.Encoding;
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

		public void HandleClientPackets(object obj)
		{
			try
			{
				// retrieve client from parameter passed to thread
				TcpClient client = (TcpClient)obj;

				NetworkStream network = client.GetStream();

				bool bClientConnected = true;

				Packet packet, response;
#warning Write encoding handshake hahaha
				while (bClientConnected)
				{
					packet = Packet.GetPacketFromNetworkStream(network);

					response = handler.Handle(packet);

					if (response.Flags.HasFlag(PacketFlags.Flags.Response))
					{
						network.Write(
							response.ToByteArray(),
							0,
							Packet.__zHeaderSize__ + response.Size);
						network.Flush();
					}
				}
			}
			catch (IOException)
			{
				textWriter.WriteLine($"Client Disconnected");
			}
		}
	}
}
