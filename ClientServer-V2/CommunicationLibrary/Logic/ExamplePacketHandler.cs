using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace CommunicationLibrary.Logic
{
	using CommunicationLibrary.Models.Flags;
	using CommunicationLibrary.Models.Pairs;
	using CommunicationLibrary.Models.Structs;
	using Models;
	using Models.Features;

	//Example Handler
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


		//private Dictionary<PacketType.Flags, Action> preHandleAction = new Dictionary<PacketType.Flags, Action>();
		//private Dictionary<PacketType.Flags, Action> postHandleAction = new Dictionary<PacketType.Flags, Action>();

		//public Reactor<Action> PreHandleReactor = new Reactor<Action>();
		//public Reactor<Func<object, object[]>> PostHandleReactor = new Reactor<Func<object, object[]>>();


		private List<byte[]> _longBuffer = new List<byte[]>();
		private FileStream _fileStream;


		//public void ChangePreHandleAction(PacketType.Flags flags, Action action)
		//{
		//	if (!preHandleAction.ContainsKey(flags))
		//		return;

		//	preHandleAction[flags] = action;
		//}
		//public void ChangePostHandleAction(PacketType.Flags flags, Action action)
		//{
		//	if (!postHandleAction.ContainsKey(flags))
		//		return;

		//	postHandleAction[flags] = action;
		//}
		//public void SetActions(Dictionary<PacketType.Flags, Action> preAction,
		//						 Dictionary<PacketType.Flags, Action> postAction)
		//{
		//	preHandleAction = preAction;
		//	postHandleAction = postAction;
		//}
		public ExamplePacketHandler(Encoding encoding, TextWriter textWriter) : base(encoding, textWriter) { }

		public override Packet Handle(Packet packet)
		{
			try
			{
				if (_ResultWriter == null)
					throw new NullReferenceException($"{nameof(_ResultWriter)} is null in ExamplePacketHandler.Hanlde");
				if (_Encoding == null)
					throw new NullReferenceException($"{nameof(_Encoding)} is null in ExamplePacketHandler.Hanlde");
				if (packet == null)
					throw new ArgumentNullException($"{nameof(packet)} is null in ExamplePacketHandler.Hanlde");

				//_responses[packet.Flags]

				switch (packet.Flags.Enum)
				{
					//=============
					//Responses
					case PacketFlags.Response:
					case PacketFlags.RspSingleMsg:
						_ResultWriter.WriteLine(packet.ToString());

						break;


					//=============
					//Messages
					case PacketFlags.SingleMsg:
						//Clear operation
						_ResultWriter.WriteLine(_Encoding.GetString(packet.Bytes));

						break;
					case PacketFlags.StartMsg:
						_longBuffer.Add(packet.Bytes);

						break;

					case PacketFlags.EndMsg:
						//Clear operation
						_longBuffer.Add(packet.Bytes);

						foreach (byte[] bytes in _longBuffer)
							_ResultWriter.Write(_Encoding.GetString(bytes));
						_ResultWriter.WriteLine();

						_longBuffer.Clear();

						break;

					// If the _longBuffer is in use, then the
					// base messages are added to it
					case PacketFlags.Message:
						if (_longBuffer.Count > 0)
							_longBuffer.Add(packet.Bytes);

						break;

					//=============
					//Files
					//https://code-maze.com/convert-byte-array-to-file-csharp/
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

					//Clear operation
					case PacketFlags.File | PacketFlags.Start:
						throw new NotImplementedException();
					case PacketFlags.File | PacketFlags.End:
						//Clear operation
						throw new NotImplementedException();
					case PacketFlags.File:
						throw new NotImplementedException();

					case (PacketFlags)0b_0000_1000:
						throw new NotImplementedException();

					case PacketFlags.None:
						throw new ArgumentNullException("Packet's has no flags");
				}

			}
			catch (NotImplementedException nie)
			{
				throw nie;
			}
			//catch (ArgumentNullException ane)
			//{
			//	throw ane;
			//}
			catch (Exception e)
			{
				return new Packet(
					PacketFlags.RspSingleErr,
					e.Message + " | {" + packet + "}",
					packet.Id);
			}

			return new Packet(
				PacketFlags.RspSingleMsg,
				_responses[packet.Flags] + ":: {" + packet + "}",
				packet.Id);
		}
	}
}
