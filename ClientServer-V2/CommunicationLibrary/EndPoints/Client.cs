using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace CommunicationLibrary.EndPoints
{
	using Logic;
	using Models;

	public class Client<TPacketFlags>
		where TPacketFlags :
					struct, Enum
	{
		private TcpClient client;
		private NetworkStream nStream;
		public bool IsConnected => isConnected;
		private bool isConnected;

		private TextReader textReader; // Console.In
		private TextWriter textWriter; // Console.Out
		private Encoding encoding; // Console.OutputEncoding

		private Thread _receiveThread;

		private Queue<Packet> packQueue = new Queue<Packet>();
		private Queue<uint> packIdQueue = new Queue<uint>();

		private IHandler<TPacketFlags> handler;

		public Client(TextReader textReader, TextWriter textWriter, IHandler<TPacketFlags> handler)
		{
			client = new TcpClient();
			isConnected = false;

			this.textReader = textReader;
			this.textWriter = textWriter;
			this.handler = handler;
			this.encoding = handler.Encoding;
		}

		/// <summary>
		/// Attempt connecting to a server on the ipAddress and port
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		public void Connect(string ipAddress, int port) =>
			Connect(IPAddress.Parse(ipAddress), port);
		/// <summary>
		/// Attempt connecting to a server on the ipAddress and port
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		public void Connect(IPAddress ipAddress, int port)
		{
			try
			{
				client.Connect(ipAddress, port);
				nStream = client.GetStream();
				isConnected = true;

				//_cThread = new Thread(HandleCommunication);
				//_cThread.Start();

				_receiveThread = new Thread(ReceiverLoop);
				_receiveThread.Start();
			}
			catch (Exception)
			{
#warning Exception needs handling
				isConnected = false;
				throw;
			}
		}

		public void Disconnect()
		{
			if (!isConnected)
				return;

			try
			{
				isConnected = false;
				client.Close();
				nStream.Close();
			}
			catch (Exception)
			{
#warning Exception needs handling

				throw;
			}
		}


		//this should handle receiving data...
		private void ReceiverLoop()
		{
			try
			{
				//Handle incoming messages....
				while (isConnected)
				{
					// I think im overdoing this...
					// The bank system needs a connection
					// Not a 0ms 24/7 back and forth chat service ):
					//kkdkkdkdkkdkdkmsdfmsdlkfmsldkfmslmflkmgvkmpoimcoinomvrotnvonec
					handler.Handle(PacketBuilder.GetPacketFromNetworkStream(nStream));
				}
			}
			catch (IOException)
			{
				textWriter.WriteLine("Server terminated");
			}
		}

		/// <summary>
		/// The Sending Loop -> Started as a Thread by Send() functions.
		/// </summary>
		private void SendLoop()
		{
			Packet curPacket;

			while (isConnected && packQueue.Count != 0)
			{
				curPacket = packQueue.Dequeue();
				packIdQueue.Enqueue(curPacket.Id);
				nStream.Write(
					curPacket.ToByteArray(),
					0,
					Packet.__HeaderSize__ + curPacket.Size);
				nStream.Flush();
			}
		}

		public void SendPacket(Packet packet)
		{
			if (packet.Size != packet.Bytes.Length)
				throw new Exception("Packet size mismatch!");

			packQueue.Enqueue(packet);

			SendLoop();
		}
		public void SendPacket(Packet[] packets)
		{
			foreach (Packet packet in packets)
			{
				if (packet.Size != packet.Bytes.Length)
					throw new Exception("Packet size mismatch!");

				packQueue.Enqueue(packet);
			}

			SendLoop();
		}

		//public void Receive()
		//{
		//    Span<byte> data = new Span<byte>();
		//    _nStream.Read(data);
		//    _textWriter.Write(_encoding.GetString(data));
		//}

	}
}
