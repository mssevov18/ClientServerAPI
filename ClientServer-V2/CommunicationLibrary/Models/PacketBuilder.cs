using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunicationLibrary.Models.Flags;
using CommunicationLibrary.Models.Structs;

namespace CommunicationLibrary.Models
{
	public static class PacketBuilder
	{
		private static readonly Encoding _encoding = Packet.Encoding;
		public static readonly byte HeaderMaxSize = Packet.HeaderSize;
		public static readonly ushort MessageMaxSize = Packet.MessageMaxSize;
		public static readonly ushort FilePacketDataMaxSize = Packet.MessageMaxSize - 256;

		private static string getString(byte[] data) => _encoding.GetString(data);
		private static byte[] getBytes(string data) => _encoding.GetBytes(data);

		public static Packet[] GetPacketsFromMsg(string message)
			=> GetPacketsFromMsg(getBytes(message));
		public static Packet[] GetPacketsFromMsg(byte[] message)
		{
			if (message.Length > Packet.MessageMaxSize)
				return GetPacketsFromLongMsg(message);
			else
				return new Packet[] { new Packet(PacketFlags.SingleMsg, message) };
		}


#warning Outdated Method
		public static Packet[] GetPacketsFromLongMsg(byte[] message)
		{
			int dataLength = message.Length;
			throw new NotImplementedException("OLD METHOD");
			ushort iteration = 0;
			Packet[] packetsArr = new Packet[(dataLength % MessageMaxSize) + 1];

			while (dataLength > 0)
			{
				byte[] tempBuffer = new byte[Packet.MessageMaxSize];

				Buffer.BlockCopy(message, iteration * Packet.MessageMaxSize,
								 tempBuffer, 0,
								 Packet.MessageMaxSize);


				PacketFlags flags = PacketFlags.Message;

				if (iteration == 0)
					flags |= PacketFlags.Start;
				else
				{
					if (dataLength <= Packet.MessageMaxSize)
						flags |= PacketFlags.End;
					else
						flags = PacketFlags.Message;
				}

				packetsArr[iteration] = new Packet(PacketFlags.StartMsg, tempBuffer);

				iteration++;
				dataLength -= Packet.MessageMaxSize;
			}

			return packetsArr;
		}

		public static Packet[] GetFilePackets(string path)
		{
			byte[] fileBytes = new byte[new FileInfo(path).Length];

			foreach (string line in File.ReadAllLines(path))
			{
				byte[] buffer = _encoding.GetBytes(line);
				Buffer.BlockCopy(buffer, 0, fileBytes, fileBytes.Length, buffer.Length);
			}

			return GetFilePackets(path.Split('\\').LastOrDefault(), fileBytes);
		}
		public static Packet[] GetFilePackets(string name, byte[] fileBytes)
		{
			if (fileBytes.Length > (Packet.MessageMaxSize - (1 + FileStruct.Encoding.GetByteCount(name))))
				return GetLongFilePackets(fileBytes);
			else

				return GetFilePackets(new FileStruct(name, fileBytes));
		}
#warning File may not be split here!
		public static Packet[] GetFilePackets(FileStruct fileStruct) =>
			new Packet[] { new Packet(PacketFlags.SingleFile, FileStruct.GetBytes(fileStruct)) };

#warning Outdated Method
		public static Packet[] GetLongFilePackets(byte[] fileBytes)
		{
			int dataLength = fileBytes.Length;
			ushort iteration = 0;
			Packet[] packetsArr = new Packet[(dataLength % (FilePacketDataMaxSize)) + 1];

			while (dataLength > 0)
			{
				byte[] tempBuffer = new byte[FilePacketDataMaxSize];

				Buffer.BlockCopy(fileBytes, iteration * FilePacketDataMaxSize,
								 tempBuffer, 0,
								 FilePacketDataMaxSize);


				PacketFlags flags = PacketFlags.File;

				if (iteration == 0)
					flags |= PacketFlags.Start;
				else
				{
					if (dataLength <= FilePacketDataMaxSize)
						flags |= PacketFlags.End;
					else
						flags = PacketFlags.Message;
				}

				packetsArr[iteration] = (new Packet((byte)flags, tempBuffer));

				iteration++;
				dataLength -= FilePacketDataMaxSize;
			}

			return packetsArr;
		}

												public static Packet GetPacketFromStreamReader(StreamReader reader)
		{
			Span<byte> buffer = _encoding.GetBytes(reader.ReadToEnd());

			return new Packet(buffer[0],
				buffer.Slice(7, BitConverter.ToUInt16(buffer.Slice(1, 2))).ToArray(),
				BitConverter.ToUInt32(buffer.Slice(3, 4)));
		}

												public static Packet GetPacketFromNetworkStream(NetworkStream network)
		{
			byte[] buffer = new byte[Packet.HeaderSize];

						byte flagsByte = (byte)network.ReadByte();  			network.Read(buffer, 0, 2);                 			ushort size = BitConverter.ToUInt16(buffer, 0);
			network.Read(buffer, 0, 4);                 			uint id = BitConverter.ToUInt32(buffer, 0);

			buffer = new byte[size];
			network.Read(buffer, 0, size);
			byte[] bytes = buffer;

			return new Packet(flagsByte, bytes, id);
		}
	}
}
