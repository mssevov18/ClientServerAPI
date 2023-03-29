using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using CommunicationLibrary.Models.Features;

using static CommunicationLibrary.Models.Features.PacketFlags;

namespace CommunicationLibrary.Models
{
	public static class PacketBuilder
	{
		private static readonly Encoding encoding = Packet._Encoding;
		public static readonly byte headerMaxSize = Packet.__HeaderSize__;
		public static readonly ushort messageMaxSize = Packet.__MessageMaxSize__;
		public static readonly ushort filePacketDataMaxSize = Packet.__MessageMaxSize__ - 256;

		private static string getString(byte[] data) => encoding.GetString(data);
		private static byte[] getBytes(string data) => encoding.GetBytes(data);

		public static Packet[] GetPacketsFromMsg(string message)
			=> GetPacketsFromMsg(getBytes(message));
		public static Packet[] GetPacketsFromMsg(byte[] message)
		{
			if (message.Length > Packet.__MessageMaxSize__)
				return GetPacketsFromLongMsg(message);
			else
				return new Packet[] { new Packet((byte)Flags.SingleMsg, message) };
		}


#warning Outdated Method
		public static Packet[] GetPacketsFromLongMsg(byte[] message)
		{
			int dataLength = message.Length;
			ushort iteration = 0;
			Packet[] packetsArr = new Packet[(dataLength % messageMaxSize) + 1];

			while (dataLength > 0)
			{
				byte[] tempBuffer = new byte[Packet.__MessageMaxSize__];

				Buffer.BlockCopy(message, iteration * Packet.__MessageMaxSize__,
								 tempBuffer, 0,
								 Packet.__MessageMaxSize__);


				Flags flags = Flags.Message;

				if (iteration == 0)
					flags |= Flags.Start;
				else
				{
					if (dataLength <= Packet.__MessageMaxSize__)
						flags |= Flags.End;
					else
						flags = Flags.Message;
				}

				packetsArr[iteration] = new Packet((byte)Flags.StartMsg, tempBuffer);

				iteration++;
				dataLength -= Packet.__MessageMaxSize__;
			}

			return packetsArr;
		}

		public static Packet[] GetFilePackets(string path)
		{
			FileInfo fileInfo = new FileInfo(path);
			byte[] fileBytes = new byte[fileInfo.Length];

			foreach (string line in File.ReadAllLines(path))
			{
				byte[] buffer = encoding.GetBytes(line);
				Buffer.BlockCopy(buffer, 0, fileBytes, fileBytes.Length, buffer.Length);
			}

			return GetFilePackets(path.Split('\\').LastOrDefault(), fileBytes);
		}
		public static Packet[] GetFilePackets(string name, byte[] fileBytes)
		{
			if (fileBytes.Length > (Packet.__MessageMaxSize__ - (1 + FileStruct.Encoding.GetByteCount(name))))
				return GetLongFilePackets(fileBytes);
			else

				return GetFilePackets(new FileStruct(name, fileBytes));
		}
#warning File may not be split here!
		public static Packet[] GetFilePackets(FileStruct fileStruct) =>
			new Packet[] { new Packet((byte)Flags.SingleFile, FileStruct.GetBytes(fileStruct)) };

#warning Outdated Method
		public static Packet[] GetLongFilePackets(byte[] fileBytes)
		{
			int dataLength = fileBytes.Length;
			ushort iteration = 0;
			Packet[] packetsArr = new Packet[(dataLength % (filePacketDataMaxSize)) + 1];

			while (dataLength > 0)
			{
				byte[] tempBuffer = new byte[filePacketDataMaxSize];

				Buffer.BlockCopy(fileBytes, iteration * filePacketDataMaxSize,
								 tempBuffer, 0,
								 filePacketDataMaxSize);


				Flags flags = Flags.File;

				if (iteration == 0)
					flags |= Flags.Start;
				else
				{
					if (dataLength <= filePacketDataMaxSize)
						flags |= Flags.End;
					else
						flags = Flags.Message;
				}

				packetsArr[iteration] = (new Packet((byte)flags, tempBuffer));

				iteration++;
				dataLength -= filePacketDataMaxSize;
			}

			return packetsArr;
		}

		/// <summary>
		/// Returns a packet from a StreamReader object.
		/// </summary>
		/// <param name="reader">A StreamReader object that contains the packet data.</param>
		/// <returns>A Packet object that represents the packet.</returns>
		public static Packet GetPacketFromStreamReader(StreamReader reader)
		{
			Span<byte> buffer = encoding.GetBytes(reader.ReadToEnd());

			return new Packet(buffer[0],
				buffer.Slice(7, BitConverter.ToUInt16(buffer.Slice(1, 2))).ToArray(),
				BitConverter.ToUInt32(buffer.Slice(3, 4)));
		}

		/// <summary>
		/// Returns a packet from a NetworkStream object.
		/// </summary>
		/// <param name="network">A NetworkStream object that contains the packet data.</param>
		/// <returns>A Packet object that represents the packet.</returns>
		public static Packet GetPacketFromNetworkStream(NetworkStream network)
		{
			byte[] buffer = new byte[Packet.__HeaderSize__];

			// Get the first 7 Bytes to create the header
			byte flagsByte = (byte)network.ReadByte();  //   Flags - 1B
			network.Read(buffer, 0, 2);                 //   Size  - 2B
			ushort size = BitConverter.ToUInt16(buffer, 0);
			network.Read(buffer, 0, 4);                 //   Id    - 4B
			uint id = BitConverter.ToUInt32(buffer, 0);

			buffer = new byte[size];
			network.Read(buffer, 0, size);
			byte[] bytes = buffer;

			return new Packet(flagsByte, bytes, id);
		}
	}
}
