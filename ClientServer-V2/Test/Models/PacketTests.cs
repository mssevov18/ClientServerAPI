using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using NUnit.Framework;

using static Test.Utils;

using CommunicationLibrary.Models;
using CommunicationLibrary.Models.Flags;
using CommunicationLibrary.Models.Structs;

namespace Test.Models.Packets
{
	public class PacketTests
	{
		private Encoding _encoding = Encoding.UTF8;
		private byte[] pckBytes = { 17, 5, 0, 1, 0, 0, 0, 72, 97, 108, 97, 108 };
		private Packet expectedPacket;
		private string msg = "Halal";
		private byte[] msgBytes => _encoding.GetBytes(msg);

		[SetUp]
		public void SetUp()
		{
			//Packet.SetEncoding(Encoding.UTF8);
			Packet.Encoding = _encoding;
			FileStruct.Encoding = _encoding;
			expectedPacket = new Packet(17, msgBytes, 1);
		}

		[Test]
		public void Ctor_ByteArray()
		{
			Console.Write(Utils.ToString(pckBytes));
			Console.WriteLine();
			Console.Write(Utils.ToString(new Packet(17, msg, 1).ToByteArray()));
			Console.WriteLine();
			Console.WriteLine(expectedPacket);
			Console.WriteLine();
			Packet packet = new Packet(pckBytes, 1);

			PrintDebug(pckBytes, packet.ToByteArray());
			Console.WriteLine();
			PrintDebug(pckBytes, packet.ToByteArray(), false);
			Assert.That(pckBytes, Is.EqualTo(packet.ToByteArray()));
		}

		[Test]
		public void Ctor_Flags_ByteArray()
		{
			Packet packet = new Packet(PacketFlags.SingleMsg, msgBytes, 1);

			PrintDebug(pckBytes, packet.ToByteArray());
			Console.WriteLine();
			PrintDebug(pckBytes, packet.ToByteArray(), false);
			Assert.That(pckBytes, Is.EqualTo(packet.ToByteArray()));
		}

		[Test]
		public void Ctor_String()
		{
			Packet packet = new Packet(PacketFlags.SingleMsg, msg, 1);

			PrintDebug(pckBytes, packet.ToByteArray());
			Assert.That(pckBytes, Is.EqualTo(packet.ToByteArray()));
		}

		[Test]
		public void DoesCount_Increment()
		{
			uint startId = Packet.PacketGenCount;

			int length = RandomNumberGenerator.GetInt32(1, 9);
			for (int i = 0; i < length; i++)
				new Packet();

			PrintDebug($"{Packet.PacketGenCount}", $"{startId + length}");
			Assert.That(Packet.PacketGenCount, Is.EqualTo(startId + length));
		}

		[Test]
		public void TestToByteArray()
		{
			Packet packet = new Packet(PacketFlags.SingleMsg, msgBytes, 1);

			PrintDebug(pckBytes, packet.Bytes);
			Assert.That(pckBytes, Is.EqualTo(packet.ToByteArray()));
		}

		[Test]
		public void CanGetPacketFromStreamReader()
		{
			StreamReader reader = new StreamReader(new MemoryStream(pckBytes));

			Packet packet = PacketBuilder.GetPacketFromStreamReader(reader);

			PrintDebug(pckBytes, packet.Bytes);
			Assert.That(pckBytes, Is.EqualTo(packet.ToByteArray()));
		}

		[Test]
		public void CanWriteToFile()
		{
			// Arrange
			string fileName = "test.txt";
			string content = "This is a test.";
			FileStruct file = new FileStruct(fileName, content);
			Packet packet = new Packet(file);

			if (File.Exists(fileName))
				File.Delete(fileName);
			//Packet packet = new Packet()

			// Act
			string fullPath = packet.WriteToFile("");
			//File.WriteAllText(fileName, content);

			// Assert
			Assert.IsTrue(File.Exists(fullPath));
			Assert.That(File.ReadAllLines(fullPath)[0], Is.EqualTo(content));
		}
	}
}
