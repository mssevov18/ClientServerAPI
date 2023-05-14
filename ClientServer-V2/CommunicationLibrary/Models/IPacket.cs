using System.Text;


namespace CommunicationLibrary.Models
{
	using System;
	using CommunicationLibrary.Models.Pairs;

	using Features;

				public interface IPacket
	{
								public static readonly ushort MsgMaxSize = ushort.MaxValue;

								public static readonly byte HeaderSize = 7;

										public static uint PacketGenCount = 1;

								public static Encoding Encoding;

								public PacketFlagsPair Flags { get; set; }

								public ushort Size { get; set; }

								public uint Id { get; set; }

								public byte[] Bytes { get; set; }

								public string Message { get; set; }

								public void Clear();

										public byte[] ToByteArray();

										public string ToString();
	}
}
