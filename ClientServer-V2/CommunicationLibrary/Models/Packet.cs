using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace CommunicationLibrary.Models
{
	public class Packet : IPacket
	{
		public static uint _PacketGenCount => _packetGenCount;
		protected static uint _packetGenCount = 1;

		public static Encoding _Encoding;

		public static readonly ushort __MsgMaxSize__ = ushort.MaxValue;
		public static readonly byte __zHeaderSize__ = 7;

		public PacketFlags.Flags Flags
		{
			get => flags;
			set
			{
				flags = value;
				//if (value == FileProtocol.AssignedFlag)
				//	protocol = new ProtocolFamily();
			}
		}
		protected PacketFlags.Flags flags;

		public ushort Size
		{
			get => size;
			set => size = value;
		}
		protected ushort size;

		public uint Id
		{
			get => id;
			set
			{
				if (flags.HasFlag(PacketFlags.Flags.Response))
					id = value;
			}
		}
		protected uint id;

		public byte[] Bytes
		{
			get => bytes;
			set
			{
				if (value.Length > ushort.MaxValue)
					throw new ArgumentOutOfRangeException($"{nameof(value)} is too large");
				bytes = value;
				size = (ushort)bytes.Length;
			}
		}
		public string Message
		{
			get => _Encoding.GetString(bytes);
			set => Bytes = _Encoding.GetBytes(value);
		}
		protected byte[] bytes;

		public FileStruct File
		{
			get
			{
				if (flags.HasFlag(PacketFlags.Flags.File))
					return FileStruct.GetStruct(bytes);

				throw new Exception("Packet is not a file!");
			}
		}

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
		public Packet(PacketFlags.Flags flags, string message, uint id = 0)
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
		public Packet(PacketFlags.Flags flags, byte[] msgBytes, uint id = 0)
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
			Flags = (PacketFlags.Flags)packetBytes[0];
			Size = BitConverter.ToUInt16(new byte[2] { packetBytes[1],
													   packetBytes[2] });
			Bytes = new ArraySegment<byte>(packetBytes, __zHeaderSize__, Size).ToArray();
			_PacketGen(BitConverter.ToUInt32(new byte[4] { packetBytes[3],
														   packetBytes[4],
														   packetBytes[5],
														   packetBytes[6]}));
		}
		/// <summary>
		/// 
		/// If id is 0 (optional), the packet's id will autogenerate
		/// </summary>
		/// <param name="inputBytes"></param>
		/// <param name="id"></param>
		public Packet(byte[] inputBytes, uint id = 0)
		{
			Flags = (PacketFlags.Flags)inputBytes[0];
			Size = BitConverter.ToUInt16(new byte[2] { inputBytes[1],
													   inputBytes[2] });
			Bytes = new ArraySegment<byte>(inputBytes, __zHeaderSize__, Size).ToArray();
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
			if (fileStruct.Length > __MsgMaxSize__)
				throw new ArgumentOutOfRangeException();

			Flags = PacketFlags.Flags.SingleFile;
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
			Flags = PacketFlags.Flags.None;
			Size = 0;
			Array.Clear(Bytes, 0, Bytes.Length);
		}

		/// <inheritdoc />
		public byte[] ToByteArray()
		{
			byte[] result = new byte[__zHeaderSize__ + Bytes.Length];

			Buffer.SetByte(result, 0, (byte)Flags);

			Buffer.BlockCopy(BitConverter.GetBytes(Size), 0,
							 result, 1, 2);

			Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, result, 3, 4);

			Buffer.BlockCopy(Bytes, 0,
							 result, __zHeaderSize__, Bytes.Length);

			return result;
		}

		/// <summary>
		/// Returns a packet from a StreamReader object.
		/// </summary>
		/// <param name="reader">A StreamReader object that contains the packet data.</param>
		/// <returns>A Packet object that represents the packet.</returns>
		public static Packet GetPacketFromStreamReader(StreamReader reader)
		{
			Span<byte> buffer = _Encoding.GetBytes(reader.ReadToEnd());

			return new Packet((PacketFlags.Flags)buffer[0],
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
			byte[] buffer = new byte[__zHeaderSize__];

			// Get the first 7 Bytes to create the header
			PacketFlags.Flags flags =    //   Flags - 1B
				(PacketFlags.Flags)network.ReadByte();
			network.Read(buffer, 0, 2); //   Size  - 2B
			ushort size = BitConverter.ToUInt16(buffer, 0);
			network.Read(buffer, 0, 4); //   Id    - 4B
			uint id = BitConverter.ToUInt32(buffer, 0);

			buffer = new byte[size];
			network.Read(buffer, 0, size);
			byte[] bytes = buffer;

			return new Packet(flags, bytes, id);
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
