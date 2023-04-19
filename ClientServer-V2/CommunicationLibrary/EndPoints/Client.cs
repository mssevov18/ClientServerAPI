using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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

		private TextReader _textReader; // Console.In
		private TextWriter _textWriter; // Console.Out
		private Encoding _encoding; // Console.OutputEncoding

		private Thread _receiveThread;

		private Queue<Packet> _packQueue = new Queue<Packet>();
		private Queue<uint> _packIdQueue = new Queue<uint>();

		private IHandler<TPacketFlags> _handler;

		public Client(TextReader textReader, TextWriter textWriter, IHandler<TPacketFlags> handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			_client = new TcpClient();
			_isConnected = false;

			this._textReader = textReader;
			this._textWriter = textWriter;
			this._handler = handler;
			this._encoding = handler.Encoding;
		}

		~Client()
		{
			Dispose();
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
				//Handle incoming messages....
				while (_isConnected)
				{
					// I think im overdoing this...
					// The bank system needs a connection
					// Not a 0ms 24/7 back and forth chat service ):
					//kkdkkdkdkkdkdkmsdfmsdlkfmsldkfmslmflkmgvkmpoimcoinomvrotnvonec
					_handler.Handle(PacketBuilder.GetPacketFromNetworkStream(_nStream));
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
			}
		}

		public void SendPacket(Packet packet)
		{
			if (packet.Size != packet.Bytes.Length)
				throw new Exception("Packet size mismatch!");

			_packQueue.Enqueue(packet);

			SendLoop();
		}
		public void SendPacket(Packet[] packets)
		{
			foreach (Packet packet in packets)
			{
				if (packet.Size != packet.Bytes.Length)
					throw new Exception("Packet size mismatch!");

				_packQueue.Enqueue(packet);
			}

			SendLoop();
		}

		public void Dispose()
		{
			_client.Dispose();
			_nStream.Dispose();
		}

		//public void Receive()
		//{
		//    Span<byte> data = new Span<byte>();
		//    _nStream.Read(data);
		//    _textWriter.Write(_encoding.GetString(data));
		//}
	}
}
