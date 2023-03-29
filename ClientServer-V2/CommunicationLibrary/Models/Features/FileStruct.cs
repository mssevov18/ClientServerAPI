using System;
using System.Text;

namespace CommunicationLibrary.Models.Features
{
	/// <summary>
	/// POCO struct to contain fileInfo
	/// 
	/// 
	/// <list type="bullet">
	///    <listheader>
	///        <term>term</term>
	///        <description>description</description>
	///    </listheader>
	///    <item>
	///        <term>Assembly</term>
	///        <description>The library or executable built from a compilation.</description>
	///    </item>
	///</list>
	/// </summary>
	public struct FileStruct
	{
		public byte NameLength;
		public string Name;
		//public string Type;
		public byte[] Data;

		public static Encoding Encoding;

		public FileStruct(byte nameLength, string name, byte[] data)
		{
			NameLength = nameLength;
			Name = name;
			Data = data;
		}
		public FileStruct(byte nameLength, string name, string data)
		{
			NameLength = nameLength;
			Name = name;
			Data = Encoding.GetBytes(data);
		}
		public FileStruct(string name, byte[] data)
		{
			NameLength = (byte)name.Length;
			Name = name;
			Data = data;
		}
		public FileStruct(string name, string data)
		{
			NameLength = (byte)name.Length;
			Name = name;
			Data = Encoding.GetBytes(data);
		}

		public int Length => 1 + NameLength + Data.Length;

		public static byte[] GetBytes(FileStruct @struct)
		{
			byte[] bytes = new byte[@struct.Length];

			Buffer.SetByte(bytes, 0, @struct.NameLength);
			Buffer.BlockCopy(Encoding.GetBytes(@struct.Name), 0, bytes, 1, @struct.NameLength);
			Buffer.BlockCopy(@struct.Data, 0, bytes, 1 + @struct.NameLength, @struct.Data.Length);

			return bytes;
		}

		public static FileStruct GetStruct(Packet packet)
			=> GetStruct(packet.Bytes);
		public static FileStruct GetStruct(byte[] bytes)
		{
			return new FileStruct(
				bytes[0],
				Encoding.GetString(new ReadOnlySpan<byte>(bytes, 1, bytes[0])),
				new ArraySegment<byte>(
					bytes,
						1 + bytes[0],
					bytes.Length - (1 + bytes[0]))
				.ToArray());
		}
	}
}
