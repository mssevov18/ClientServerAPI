using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace CommunicationLibrary.Models
{
	using CommunicationLibrary.Models.Pairs;
	using CommunicationLibrary.Models.Structs;

	using Features;

	using static CommunicationLibrary.Models.Flags.PacketFlags;

	public class Packet : IPacket
	{
		public static uint PacketGenCount => packetGenCount;
		private static uint packetGenCount = 1;

		public static Encoding Encoding;

		public const ushort MessageMaxSize = ushort.MaxValue;
		public const byte HeaderSize = 7;
		public const uint PacketMaxSize = ushort.MaxValue + 7;

		public PacketFlagsPair Flags
		{
			get => _flags;
			set => _flags = value;
		}
		private PacketFlagsPair _flags;

		public ushort Size
		{
			get => _size;
			set => _size = value;
		}
		private ushort _size;

		public uint Id
		{
			get => _id;
			set => _id = value;
		}
		private uint _id;

		public byte[] Bytes
		{
			get => _messageBytes;
			set
			{
				if (value.Length > ushort.MaxValue)
					throw new ArgumentOutOfRangeException($"{nameof(value)} is too large");
				_messageBytes = value;
				_size = (ushort)_messageBytes.Length;
			}
		}
		public string Message
		{
			get => Encoding.GetString(_messageBytes);
			set => Bytes = Encoding.GetBytes(value);
		}
		private byte[] _messageBytes;

		#region ctor
												public Packet(uint id = 0)
		{
			Flags = 0;
			Bytes = new byte[0];
			_PacketGen(id);
		}
																		public Packet(PacketFlagsPair flags, string message, uint id = 0)
		{
			if (Encoding == null)
				throw new NullReferenceException($"{nameof(Encoding)} can't be null!");

			Flags = flags;
			Message = message;
			_PacketGen(id);
		}
																public Packet(PacketFlagsPair flags, byte[] msgBytes, uint id = 0)
		{
			Flags = flags;
			Bytes = msgBytes;
			_PacketGen(id);
		}
										public Packet(byte[] packetBytes)
		{
			Flags = packetBytes[0];
			Size = BitConverter.ToUInt16(new byte[2] { packetBytes[1],
													   packetBytes[2] });
			Bytes = new ArraySegment<byte>(packetBytes, HeaderSize, Size).ToArray();
			_PacketGen(BitConverter.ToUInt32(new byte[4] { packetBytes[3],
														   packetBytes[4],
														   packetBytes[5],
														   packetBytes[6]}));
		}
												public Packet(byte[] packetBytes, uint id)
		{
			Flags = packetBytes[0];
			Size = BitConverter.ToUInt16(new byte[2] { packetBytes[1],
													   packetBytes[2] });
			Bytes = new ArraySegment<byte>(packetBytes, HeaderSize, Size).ToArray();
			_PacketGen(id);
		}
														public Packet(FileStruct fileStruct, uint id = 0)
		{
			if (fileStruct.Length > MessageMaxSize)
				throw new ArgumentOutOfRangeException($"{nameof(fileStruct)} is too long");

			
			Bytes = FileStruct.GetBytes(fileStruct);

			_PacketGen(id);
		}

				private void _PacketGen(uint id = 0)
		{
			if (id == 0)
				_id = packetGenCount++;
			else
				_id = id;
		}
		#endregion

				public void Clear()
		{
			Flags = 0;
			Size = 0;
			Array.Clear(Bytes, 0, Bytes.Length);
		}

				public byte[] ToByteArray()
		{
			byte[] result = new byte[HeaderSize + Bytes.Length];

			Buffer.SetByte(result, 0, Flags);

			Buffer.BlockCopy(BitConverter.GetBytes(Size), 0,
							 result, 1, 2);

			Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, result, 3, 4);

			Buffer.BlockCopy(Bytes, 0,
							 result, HeaderSize, Bytes.Length);

			return result;
		}

														public string WriteToFile(string directoryPath, string fileName = null)
		{
						if (fileName == null)
				fileName = Path.GetRandomFileName();

						string filePath = Path.Combine(directoryPath, fileName);

						
			System.IO.File.WriteAllBytes(filePath, FileStruct.GetStruct(Bytes).Data);

						return filePath;
		}

		public override string ToString()
			=> $"#{Id} ({Flags}), {Size} Byte{(Size == 1 ? "" : "s")}: {{ {Message} }}";
	}
}
