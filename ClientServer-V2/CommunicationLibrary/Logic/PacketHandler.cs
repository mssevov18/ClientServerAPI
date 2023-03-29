using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace CommunicationLibrary.Logic
{
	using Models;
	using Models.Features;

	//Example Handler
	public class ExamplePacketHandler : BaseHandler
	{
		private readonly Dictionary<PacketFlags.Flags, string> _sResponses_ = new Dictionary<PacketFlags.Flags, string>()
		{
			{ PacketFlags.Flags.Message, "Message" },
			{ PacketFlags.Flags.Message | PacketFlags.Flags.Single, "Single Message"},
			{ PacketFlags.Flags.Message | PacketFlags.Flags.Start, "Start Message"},
			{ PacketFlags.Flags.Message | PacketFlags.Flags.End, "End Message"},
			{ PacketFlags.Flags.Message | PacketFlags.Flags.Single | PacketFlags.Flags.Response, "Single Response Message"},

			{ PacketFlags.Flags.Error | PacketFlags.Flags.Single, "Single Error" },
			{ PacketFlags.Flags.Error | PacketFlags.Flags.Single | PacketFlags.Flags.Response, "Single Error Response" },

			{ PacketFlags.Flags.File | PacketFlags.Flags.Single, "Single File" },
			{ PacketFlags.Flags.File | PacketFlags.Flags.Start, "Start File" },
			{ PacketFlags.Flags.File | PacketFlags.Flags.End, "End File" }
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

				switch (packet.Flags)
				{
					//=============
					//Responses
					case PacketFlags.Flags.Response:
					case PacketFlags.Flags.RspSingleMsg:
						resultWriter.WriteLine(packet.ToString());

						break;


					//=============
					//Messages
					case PacketFlags.Flags.SingleMsg:
						//Clear operation
						resultWriter.WriteLine(encoding.GetString(packet.Bytes));

						break;
					case PacketFlags.Flags.StartMsg:
						longBuffer.Add(packet.Bytes);

						break;

					case PacketFlags.Flags.EndMsg:
						//Clear operation
						longBuffer.Add(packet.Bytes);

						foreach (byte[] bytes in longBuffer)
							resultWriter.Write(encoding.GetString(bytes));
						resultWriter.WriteLine();

						longBuffer.Clear();

						break;

					// If the _longBuffer is in use, then the
					// base messages are added to it
					case PacketFlags.Flags.Message:
						if (longBuffer.Count > 0)
							longBuffer.Add(packet.Bytes);

						break;

					//=============
					//Files
					//https://code-maze.com/convert-byte-array-to-file-csharp/
					case PacketFlags.Flags.File | PacketFlags.Flags.Single:
						FileStruct fstruct = packet.File;
						if (packet.File.Name.Length == 0)
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
					case PacketFlags.Flags.File | PacketFlags.Flags.Start:
						throw new NotImplementedException();
					case PacketFlags.Flags.File | PacketFlags.Flags.End:
						//Clear operation
						throw new NotImplementedException();
					case PacketFlags.Flags.File:
						throw new NotImplementedException();

					case (PacketFlags.Flags)0b_0000_1000:
						throw new NotImplementedException();

					case PacketFlags.Flags.None:
						throw new ArgumentNullException("Packet's has no flags");
				}

			}
			catch (Exception e)
			{
				return new Packet(
					PacketFlags.Flags.RspSingleErr,
					e.Message + " | {" + packet + "}",
					packet.Id);
			}

			return new Packet(
				PacketFlags.Flags.RspSingleMsg,
				_sResponses_[packet.Flags] + ":: {" + packet + "}",
				packet.Id);
		}
	}
}
