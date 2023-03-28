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
	public class Client
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

		private IHandler handler;

		public Client(TextReader textReader, TextWriter textWriter, IHandler handler)
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
					handler.Handle(Packet.GetPacketFromNetworkStream(nStream));
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
					Packet.__zHeaderSize__ + curPacket.Size);
				nStream.Flush();
			}
		}

		/// <summary>
		/// Send a string
		/// </summary>
		/// <param name="data"></param>
		public void SendMsg(string data) =>
			SendMsg(encoding.GetBytes(data));

		public void SendMsg(byte[] data)
		{
			if (data.Length > Packet.__MsgMaxSize__)
				SendLongMsg(data);
			else
				SendPacket(new Packet(PacketFlags.Flags.SingleMsg, data));
		}


#warning Outdated Method
		public void SendLongMsg(byte[] bytes)
		{
			int dataLength = bytes.Length;
			ushort iterations = 0;
			while (dataLength > 0)
			{
				byte[] tempBuffer = new byte[Packet.__MsgMaxSize__];

				Buffer.BlockCopy(bytes, iterations * Packet.__MsgMaxSize__,
								 tempBuffer, 0,
								 Packet.__MsgMaxSize__);


				PacketFlags.Flags flags = PacketFlags.Flags.Message;

				if (iterations == 0)
					flags |= PacketFlags.Flags.Start;
				else
				{
					if (dataLength <= Packet.__MsgMaxSize__)
						flags |= PacketFlags.Flags.End;
					else
						flags = PacketFlags.Flags.Message;
				}

				packQueue.Enqueue(new Packet(PacketFlags.Flags.StartMsg, tempBuffer));

				iterations++;
				dataLength -= Packet.__MsgMaxSize__;
			}
		}

		public void SendFile(string path)
		{
			FileInfo fileInfo = new FileInfo(path);
			byte[] bytes = new byte[fileInfo.Length];

			foreach (string line in File.ReadAllLines(path))
			{
				byte[] buffer = encoding.GetBytes(line);
				Buffer.BlockCopy(buffer, 0, bytes, bytes.Length, buffer.Length);
			}

			SendFile(path.Split('\\').LastOrDefault(), bytes);
		}
		public void SendFile(string name, byte[] fileBytes)
		{
			if (fileBytes.Length > Packet.__MsgMaxSize__)
				SendLongFile(fileBytes);
			else

				SendFile(new FileStruct(name, fileBytes));
		}
		public void SendFile(FileStruct fileStruct) =>
			SendPacket(new Packet(fileStruct));

#warning Outdated Method
		public void SendLongFile(byte[] fileBytes)
		{
			int dataLength = fileBytes.Length;
			ushort iterations = 0;
			while (dataLength > 0)
			{
				byte[] tempBuffer = new byte[Packet.__MsgMaxSize__];

				Buffer.BlockCopy(fileBytes, iterations * Packet.__MsgMaxSize__,
								 tempBuffer, 0,
								 Packet.__MsgMaxSize__);


				PacketFlags.Flags flags = PacketFlags.Flags.File;

				if (iterations == 0)
					flags |= PacketFlags.Flags.Start;
				else
				{
					if (dataLength <= Packet.__MsgMaxSize__)
						flags |= PacketFlags.Flags.End;
					else
						flags = PacketFlags.Flags.Message;
				}

				packQueue.Enqueue(new Packet(flags, tempBuffer));

				iterations++;
				dataLength -= Packet.__MsgMaxSize__;
			}
		}

		public void SendPacket(Packet packet)
		{
			if (packet.Size != packet.Bytes.Length)
				throw new Exception("Packet size mismatch!");

			packQueue.Enqueue(packet);

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
