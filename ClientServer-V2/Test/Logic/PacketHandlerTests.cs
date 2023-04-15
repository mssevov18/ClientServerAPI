using System;
using System.Text;
using System.IO;

using NUnit.Framework;

using static Test.Utils;

using CommunicationLibrary.Logic;
using CommunicationLibrary.Models;
using CommunicationLibrary.Models.Flags;

namespace Test.Logic
{
	public class PacketHandlerTests
	{
		private StringWriter _writer;
		private StringBuilder _builder;
		private Encoding _encoding;
		private ExamplePacketHandler _handler;

		[SetUp]
		public void Setup()
		{
			_builder = new StringBuilder();
			_writer = new StringWriter(_builder);

			Console.OutputEncoding = Encoding.UTF8;
			_encoding = Encoding.UTF8;

			_handler = new ExamplePacketHandler(_encoding, _writer);
			Packet._Encoding = _encoding;
			//Packet.SetEncoding(_encoding);
		}

		private string CleanResult => RemoveControlCharacters(_builder.ToString());

		[TestCase("Hello World")]
		[TestCase("Lorem ipsum")]
		[TestCase("Dolor Sit amet")]
		[TestCase("Здравей pederasso!")]
		public void m_Handle_ShortMessage(string msg)
		{
			_builder.Clear();
			Packet packet = new Packet(PacketFlags.SingleMsg, msg);

			_handler.Handle(packet);

			PrintDebug(msg, CleanResult);
			Assert.That(CleanResult,
				Is.EqualTo(msg));
		}

		private string[] strings;
		[Test]
		public void m_Handle_LongMessage()
		{
			strings = new string[] { "Hello ", "world!" };
			_builder.Clear();
			Packet p1 = new Packet(PacketFlags.StartMsg, strings[0]);
			Packet p2 = new Packet(PacketFlags.EndMsg, strings[1]);

			_handler.Handle(p1);
			_handler.Handle(p2);

			PrintDebug($"{strings[0]}{strings[1]}", CleanResult);
			Assert.That(CleanResult,
				Is.EqualTo($"{strings[0]}{strings[1]}"));
		}

		[Test]
		public void m_Handle_LongERMessage()
		{
			strings = new string[] { "He", "llo ", "wor", "ld!" };
			_builder.Clear();
			Packet p1 = new Packet(PacketFlags.StartMsg, strings[0]);
			Packet p2 = new Packet(PacketFlags.Message, strings[1]);
			Packet p3 = new Packet(PacketFlags.Message, strings[2]);
			Packet p4 = new Packet(PacketFlags.EndMsg, strings[3]);

			_handler.Handle(p1);
			_handler.Handle(p2);
			_handler.Handle(p3);
			_handler.Handle(p4);

			PrintDebug($"{strings[0]}{strings[1]}{strings[2]}{strings[3]}", CleanResult);
			Assert.That(CleanResult,
				Is.EqualTo($"{strings[0]}{strings[1]}{strings[2]}{strings[3]}"));
		}

		[Test]
		public void m_Handle_Response_Message()
		{
			string msg = "";
			_builder.Clear();

			Packet packet = new Packet(PacketFlags.SingleMsg, msg);

			Packet response = _handler.Handle(packet);

			PrintDebug(packet.ToString(), response.ToString());
			Assert.That(packet.Id,
				Is.EqualTo(response.Id));
		}

		[Test]
		public void m_Handle_Response_Message_Fail()
		{
			string msg = "";
			_builder.Clear();

			Packet packet = new Packet(PacketFlags.None, msg);

			Packet response = _handler.Handle(packet);

			PrintDebug(packet.ToString(), response.ToString());
			Assert.That(packet.Id,
				Is.EqualTo(response.Id));
			Assert.True(((PacketFlags)response.Flags).HasFlag(PacketFlags.Error));
		}

		[Test]
		public void m_Handle_File()
		{

		}
	}
}
