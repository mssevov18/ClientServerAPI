using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace CommunicationLibrary.Models
{
	using CommunicationLibrary.Models.Pairs;

	using Features;

	using static Features.PacketFlags;

	public class Packet : IPacket
	{
		public static uint _PacketGenCount => _packetGenCount;
		private static uint _packetGenCount = 1;

		public static Encoding _Encoding;

		public const ushort __MessageMaxSize__ = ushort.MaxValue;
		public const byte __HeaderSize__ = 7;
		public const uint __PacketMaxSize__ = ushort.MaxValue + 7;

		public PacketFlagsPair Flags
		{
			get => flags;
			set => flags = value;
		}
		private PacketFlagsPair flags;

		public ushort Size
		{
			get => size;
			set => size = value;
		}
		private ushort size;

		public uint Id
		{
			get => id;
			set => id = value;
		}
		private uint id;

		public byte[] Bytes
		{
			get => messageBytes;
			set
			{
				if (value.Length > ushort.MaxValue)
					throw new ArgumentOutOfRangeException($"{nameof(value)} is too large");
				messageBytes = value;
				size = (ushort)messageBytes.Length;
			}
		}
		public string Message
		{
			get => _Encoding.GetString(messageBytes);
			set => Bytes = _Encoding.GetBytes(value);
		}
		private byte[] messageBytes;

		#region ctor
		/// <summary>
		/// Default constructor.
		/// If id is 0 (optional), the packet's id will autogenerate
		/// </summary>
		/// <param name="id"></param>
		public Packet(uint id = 0)
		{
			Flags = 0;
			Bytes = new byte[0];
			_PacketGen(id);
		}
		/// <summary>
		/// 
		/// If id is 0 (optional), the packet's id will autogenerate
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="message"></param>
		/// <param name="id"></param>
		/// <exception cref="Exception"></exception>
		public Packet(PacketFlagsPair flags, string message, uint id = 0)
		{
			if (_Encoding == null)
				throw new Exception("Encoding can't be null!");

			Flags = flags;
			Message = message;
			_PacketGen(id);
		}
		/// <summary>
		/// 
		/// If id is 0 (optional), the packet's id will autogenerate
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="msgBytes"></param>
		/// <param name="id"></param>
		public Packet(PacketFlagsPair flags, byte[] msgBytes, uint id = 0)
		{
			Flags = flags;
			Bytes = msgBytes;
			_PacketGen(id);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="packetBytes"></param>
		public Packet(byte[] packetBytes)
		{
			Flags = packetBytes[0];
			Size = BitConverter.ToUInt16(new byte[2] { packetBytes[1],
													   packetBytes[2] });
			Bytes = new ArraySegment<byte>(packetBytes, __HeaderSize__, Size).ToArray();
			_PacketGen(BitConverter.ToUInt32(new byte[4] { packetBytes[3],
														   packetBytes[4],
														   packetBytes[5],
														   packetBytes[6]}));
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="packetBytes"></param>
		/// <param name="id"></param>
		public Packet(byte[] packetBytes, uint id)
		{
			Flags = packetBytes[0];
			Size = BitConverter.ToUInt16(new byte[2] { packetBytes[1],
													   packetBytes[2] });
			Bytes = new ArraySegment<byte>(packetBytes, __HeaderSize__, Size).ToArray();
			_PacketGen(id);
		}
		/// <summary>
		/// 
		/// If id is 0 (optional), the packet's id will autogenerate
		/// </summary>
		/// <param name="fileStruct"></param>
		/// <param name="id"></param>
		public Packet(FileStruct fileStruct, uint id = 0)
		{
			if (fileStruct.Length > __MessageMaxSize__)
				throw new ArgumentOutOfRangeException();

			//fileStruct.NameLength;

			Bytes = FileStruct.GetBytes(fileStruct);

			_PacketGen(id);
		}

		/// <inheritdoc />
		private void _PacketGen(uint id = 0)
		{
			if (id == 0)
				this.id = _packetGenCount++;
			else
				this.id = id;

			//if (flags.HasFlag(PacketType.Flags.File))
			//    fileStruct = FileStruct.GetStruct(bytes);
		}
		#endregion

		/// <inheritdoc />
		public void Clear()
		{
			Flags = 0;
			Size = 0;
			Array.Clear(Bytes, 0, Bytes.Length);
		}

		/// <inheritdoc />
		public byte[] ToByteArray()
		{
			byte[] result = new byte[__HeaderSize__ + Bytes.Length];

			Buffer.SetByte(result, 0, Flags);

			Buffer.BlockCopy(BitConverter.GetBytes(Size), 0,
							 result, 1, 2);

			Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, result, 3, 4);

			Buffer.BlockCopy(Bytes, 0,
							 result, __HeaderSize__, Bytes.Length);

			return result;
		}

		/// <summary>
		/// Writes the packet bytes to a file in the specified directory and returns the full file path.
		/// </summary>
		/// <param name="directoryPath">The directory path where the file should be created.</param>
		/// <param name="fileName">The name of the file to be created (optional). If not specified, a random file name will be generated.</param>
		/// <returns>The full file path of the created file.</returns>
		public string WriteToFile(string directoryPath, string fileName = null)
		{
			// generate a random file name
			if (fileName == null)
				fileName = Path.GetRandomFileName();

			// combine the directory path and file name to create a full file path
			string filePath = Path.Combine(directoryPath, fileName);

			// write the bytes to the file
			//FileStruct = FileStruct.GetStruct(bytes);

			System.IO.File.WriteAllBytes(filePath, FileStruct.GetStruct(Bytes).Data);

			// return the full file path
			return filePath;
		}

		public override string ToString() => $"#{Id} {Flags}[{Size}] {{ {_Encoding.GetString(Bytes)} }}";
	}
}
