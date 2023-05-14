using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CommunicationLibrary.Logic;
using CommunicationLibrary.Models;

namespace CommunicationLibrary.EndPoints
{
	public class Client<TPacketFlags> : IDisposable
		where TPacketFlags : struct, Enum
	{
		private TcpClient _client;
		private NetworkStream _nStream;
		public bool IsConnected => _isConnected;
		private bool _isConnected;

		public TextReader TextReader => _textReader;
		private TextReader _textReader; // Console.In
		public TextWriter TextWriter => _textWriter;
		private TextWriter _textWriter; // Console.Out
		private Encoding _encoding; // Console.OutputEncoding

		private Thread _receiveThread;

		private Queue<Packet> _packQueue = new Queue<Packet>();
		private Queue<uint> _packIdQueue = new Queue<uint>();

		protected IHandler<TPacketFlags> _handler;

		public event EventHandler<PacketEventArgs> OnPacketRecieved;
		public event EventHandler<PacketEventArgs> OnPacketSent;

		public Client(TextReader textReader, TextWriter textWriter, IHandler<TPacketFlags> handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			_client = new TcpClient();
			_isConnected = false;

			_textReader = textReader;
			_textWriter = textWriter;
			_handler = handler;
			_encoding = handler.Encoding;
		}

		~Client()
		{
			Dispose();
		}

		public void Connect(int port)
			=> Connect(Utils.GetLocalIPAddress(), port);
		/// <summary>
		/// Attempt connecting to a server on the ipAddress and port
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		public void Connect(string ipAddress, int port)
			=> Connect(IPAddress.Parse(ipAddress), port);
		/// <summary>
		/// Attempt connecting to a server on the ipAddress and port
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		public void Connect(IPAddress ipAddress, int port)
		{
			try
			{
				_client.Connect(ipAddress, port);
				_nStream = _client.GetStream();
				_isConnected = true;

				//_cThread = new Thread(HandleCommunication);
				//_cThread.Start();

				_receiveThread = new Thread(ReceiverLoop);
				_receiveThread.Start();
			}
			catch (Exception)
			{
#warning Exception needs handling
				_isConnected = false;
				throw;
			}
		}

		public void Disconnect()
		{
			if (!_isConnected)
				return;

			try
			{
				_isConnected = false;
				_client.Close();
				_nStream.Close();
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
				Packet packet;
				LinkedList<Packet> responses = new LinkedList<Packet>();
				//Handle incoming messages....
				while (_isConnected)
				{
					// I think im overdoing this...
					// The bank system needs a connection
					// Not a 0ms 24/7 back and forth chat service ):
					//kkdkkdkdkkdkdkmsdfmsdlkfmsldkfmslmflkmgvkmpoimcoinomvrotnvonec
					packet = PacketBuilder.GetPacketFromNetworkStream(_nStream);

					if (OnPacketRecieved != null)
						OnPacketRecieved.Invoke(this, packet);

					responses = _handler.Handle(packet);

					if (responses != null && responses.Count > 0)
						foreach (Packet pck in responses)
							SendPacket(pck);
				}
			}
			catch (IOException)
			{
				_textWriter.WriteLine("Server terminated");
			}
		}

		/// <summary>
		/// The Sending Loop -> Started as a Thread by Send() functions.
		/// </summary>
		private void SendLoop()
		{
			Packet curPacket;

			while (_isConnected && _packQueue.Count != 0)
			{
				curPacket = _packQueue.Dequeue();
				_packIdQueue.Enqueue(curPacket.Id);
				_nStream.Write(
					curPacket.ToByteArray(),
					0,
					Packet.HeaderSize + curPacket.Size);
				_nStream.Flush();
				if (OnPacketSent != null)
					OnPacketSent(this, curPacket);
			}
		}

		public void SendPacket(Packet packet)
		{
			if (packet.Size != packet.Bytes.Length)
				throw new Exception("Packet size mismatch!");

			//_packQueue.Enqueue(packet);
			//SendLoop();

			_nStream.Write(
				packet.ToByteArray(),
				0,
				Packet.HeaderSize + packet.Size);
			_nStream.Flush();
			if (OnPacketSent != null)
				OnPacketSent(this, packet);

		}
		public void SendPacket(Packet[] packets)
		{
			foreach (Packet packet in packets)
			{
				//if (packet.Size != packet.Bytes.Length)
				//	throw new Exception("Packet size mismatch!");
				//_packQueue.Enqueue(packet);

				SendPacket(packet);
			}

			//SendLoop();
		}

		public void Dispose()
		{
			if (IsConnected)
				Disconnect();

			_client.Dispose();
			_nStream.Dispose();
		}

		public virtual async Task<Packet> WaitForPacketResponse(Packet packet, int timeout = 1000)
		{
			return await _handler.WaitForPacketResponse(packet, timeout);
		}

		//public void Receive()
		//{
		//    Span<byte> data = new Span<byte>();
		//    _nStream.Read(data);
		//    _textWriter.Write(_encoding.GetString(data));
		//}
	}
}
