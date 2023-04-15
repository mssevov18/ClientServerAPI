using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace CommunicationLibrary.Logic
{
	using CommunicationLibrary.Models.Flags;
	using CommunicationLibrary.Models.Pairs;

	using Models;
	using Models.Features;

	//Example Handler
	public class ExamplePacketHandler : BaseHandler<PacketFlags>
	{
		private readonly Dictionary<PacketFlags, string> _sResponses_ = new Dictionary<PacketFlags, string>()
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


		private List<byte[]> longBuffer = new List<byte[]>();
		private FileStream fileStream;


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
				if (resultWriter == null)
					throw new Exception("_textWriter is null");
				if (encoding == null)
					throw new Exception("encoding is null");

#warning Finish the Handle method

				//_responses[packet.Flags]

				switch (packet.Flags.Enum)
				{
					//=============
					//Responses
					case PacketFlags.Response:
					case PacketFlags.RspSingleMsg:
						resultWriter.WriteLine(packet.ToString());

						break;


					//=============
					//Messages
					case PacketFlags.SingleMsg:
						//Clear operation
						resultWriter.WriteLine(encoding.GetString(packet.Bytes));

						break;
					case PacketFlags.StartMsg:
						longBuffer.Add(packet.Bytes);

						break;

					case PacketFlags.EndMsg:
						//Clear operation
						longBuffer.Add(packet.Bytes);

						foreach (byte[] bytes in longBuffer)
							resultWriter.Write(encoding.GetString(bytes));
						resultWriter.WriteLine();

						longBuffer.Clear();

						break;

					// If the _longBuffer is in use, then the
					// base messages are added to it
					case PacketFlags.Message:
						if (longBuffer.Count > 0)
							longBuffer.Add(packet.Bytes);

						break;

					//=============
					//Files
					//https://code-maze.com/convert-byte-array-to-file-csharp/
					case PacketFlags.File | PacketFlags.Single:
						FileStruct fstruct = FileStruct.GetStruct(packet);
						if (fstruct.Name.Length == 0)
							fstruct.Name = Path.GetRandomFileName();
						if (File.Exists(fstruct.Name))
							fileStream = new FileStream(fstruct.Name, FileMode.Truncate);
						else
							fileStream = new FileStream(fstruct.Name, FileMode.CreateNew);
						fileStream.Write(fstruct.Data, 0, fstruct.Data.Length);
						fileStream.Close();

						resultWriter.WriteLine($"File {fstruct.Name} recieved");
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
			catch (Exception e)
			{
				return new Packet(
					PacketFlags.RspSingleErr,
					e.Message + " | {" + packet + "}",
					packet.Id);
			}

			return new Packet(
				PacketFlags.RspSingleMsg,
				_sResponses_[packet.Flags] + ":: {" + packet + "}",
				packet.Id);
		}
	}
}
