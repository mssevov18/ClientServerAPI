using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace CommunicationLibrary.Logic
{
	using System.Threading.Tasks;

	using CommunicationLibrary.Models.Flags;
	using CommunicationLibrary.Models.Pairs;
	using CommunicationLibrary.Models.Structs;

	using Models;
	using Models.Features;

	public class ExamplePacketHandler : BaseHandler<PacketFlags>
	{
		private readonly Dictionary<PacketFlags, string> _responses = new Dictionary<PacketFlags, string>()
		{
			{ PacketFlags.Message, "Message" },
			{ PacketFlags.Message | PacketFlags.Single, "Single Message"},
			{ PacketFlags.Message | PacketFlags.Start, "Start Message"},
			{ PacketFlags.Message | PacketFlags.End, "End Message"},
			{ PacketFlags.Message | PacketFlags.Single | PacketFlags.Response, "Single Response Message"},

			{ PacketFlags.Error | PacketFlags.Single, "Single Error" },
			{ PacketFlags.Error | PacketFlags.Single | PacketFlags.Response, "Single Error Response" },

			{ PacketFlags.File | PacketFlags.Single, "Single File" },
			{ PacketFlags.File | PacketFlags.Start, "Start File" },
			{ PacketFlags.File | PacketFlags.End, "End File" }
		};





		private List<byte[]> _longBuffer = new List<byte[]>();
		private FileStream _fileStream;




		public ExamplePacketHandler(Encoding encoding, TextWriter textWriter) : base(encoding, textWriter) { }

		public override LinkedList<Packet> Handle(Packet packet)
		{
			LinkedList<Packet> retPackets = new LinkedList<Packet>();

			try
			{
				if (_ResultWriter == null)
					throw new NullReferenceException($"{nameof(_ResultWriter)} is null in ExamplePacketHandler.Hanlde");
				if (_Encoding == null)
					throw new NullReferenceException($"{nameof(_Encoding)} is null in ExamplePacketHandler.Hanlde");
				if (packet == null)
					throw new ArgumentNullException($"{nameof(packet)} is null in ExamplePacketHandler.Hanlde");


				switch (packet.Flags.Enum)
				{
					case PacketFlags.Response:
					case PacketFlags.RspSingleMsg:
						_ResultWriter.WriteLine(packet.ToString());

						break;


					case PacketFlags.SingleMsg:
						_ResultWriter.WriteLine(_Encoding.GetString(packet.Bytes));

						break;
					case PacketFlags.StartMsg:
						_longBuffer.Add(packet.Bytes);

						break;

					case PacketFlags.EndMsg:
						_longBuffer.Add(packet.Bytes);

						foreach (byte[] bytes in _longBuffer)
							_ResultWriter.Write(_Encoding.GetString(bytes));
						_ResultWriter.WriteLine();

						_longBuffer.Clear();

						break;

					case PacketFlags.Message:
						if (_longBuffer.Count > 0)
							_longBuffer.Add(packet.Bytes);

						break;

																				case PacketFlags.File | PacketFlags.Single:
						FileStruct fStruct = FileStruct.GetStruct(packet);
						if (fStruct.Name.Length == 0)
							fStruct.Name = Path.GetRandomFileName();
						if (File.Exists(fStruct.Name))
							_fileStream = new FileStream(fStruct.Name, FileMode.Truncate);
						else
							_fileStream = new FileStream(fStruct.Name, FileMode.CreateNew);
						_fileStream.Write(fStruct.Data, 0, fStruct.Data.Length);
						_fileStream.Close();

						_ResultWriter.WriteLine($"File {fStruct.Name} recieved");
						break;

										case PacketFlags.File | PacketFlags.Start:
						throw new NotImplementedException();
					case PacketFlags.File | PacketFlags.End:
												throw new NotImplementedException();
					case PacketFlags.File:
						throw new NotImplementedException();

					case (PacketFlags)0b_0000_1000:
						throw new NotImplementedException();

					case PacketFlags.None:
						throw new ArgumentNullException("Packet's has no flags");
				}

				retPackets.AddLast(new Packet(
				PacketFlags.RspSingleMsg,
				_responses[packet.Flags] + ":: {" + packet + "}",
				packet.Id));
			}
			catch (NotImplementedException nie)
			{
				throw nie;
			}
															catch (Exception e)
			{
				retPackets.AddLast(new Packet(
					PacketFlags.RspSingleErr,
					e.Message + " | {" + packet + "}",
					packet.Id));
			}

			return retPackets;
		}

		public override Task<Packet> WaitForPacketResponse(Packet packet, int timeout = 1000)
			=> throw new NotImplementedException();
	}
}
