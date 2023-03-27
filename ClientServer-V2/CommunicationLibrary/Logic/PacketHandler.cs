using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CommunicationLibrary.Models;

namespace CommunicationLibrary.Logic
{
	public class PacketHandler : BaseHandler
	{
		private readonly Dictionary<PacketType.Flags, string> _sResponses_ = new Dictionary<PacketType.Flags, string>()
		{
			{ PacketType.Flags.Message, "Message" },
			{ PacketType.Flags.Message | PacketType.Flags.Single, "Single Message"},
			{ PacketType.Flags.Message | PacketType.Flags.Start, "Start Message"},
			{ PacketType.Flags.Message | PacketType.Flags.End, "End Message"},
			{ PacketType.Flags.Message | PacketType.Flags.Single | PacketType.Flags.Response, "Single Response Message"},

			{ PacketType.Flags.Error | PacketType.Flags.Single, "Single Error" },
			{ PacketType.Flags.Error | PacketType.Flags.Single | PacketType.Flags.Response, "Single Error Response" },

			{ PacketType.Flags.File | PacketType.Flags.Single, "Single File" },
			{ PacketType.Flags.File | PacketType.Flags.Start, "Start File" },
			{ PacketType.Flags.File | PacketType.Flags.End, "End File" }
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
		public PacketHandler(Encoding encoding, TextWriter textWriter)
		{
			Encoding = encoding;
			TextWriter = textWriter;
		}

		public TextWriter TextWriter
		{
			get => _textWriter;
			set => _textWriter = value;
		}
		private TextWriter _textWriter;

		public override Packet Handle(Packet packet)
		{
			try
			{
				if (_textWriter == null)
					throw new Exception("_textWriter is null");
				if (encoding == null)
					throw new Exception("encoding is null");

#warning Finish the Handle method

				//_responses[packet.Flags]

				switch (packet.Flags)
				{
					//=============
					//Responses
					case PacketType.Flags.Response:
					case PacketType.Flags.RspSingleMsg:
						_textWriter.WriteLine(packet.ToString());

						break;


					//=============
					//Messages
					case PacketType.Flags.SingleMsg:
						//Clear operation
						_textWriter.WriteLine(encoding.GetString(packet.Bytes));

						break;
					case PacketType.Flags.StartMsg:
						longBuffer.Add(packet.Bytes);

						break;

					case PacketType.Flags.EndMsg:
						//Clear operation
						longBuffer.Add(packet.Bytes);

						foreach (byte[] bytes in longBuffer)
							_textWriter.Write(encoding.GetString(bytes));
						_textWriter.WriteLine();

						longBuffer.Clear();

						break;

					// If the _longBuffer is in use, then the
					// base messages are added to it
					case PacketType.Flags.Message:
						if (longBuffer.Count > 0)
							longBuffer.Add(packet.Bytes);

						break;

					//=============
					//Files
					//https://code-maze.com/convert-byte-array-to-file-csharp/
					case PacketType.Flags.File | PacketType.Flags.Single:
						FileStruct fstruct = packet.File;
						if (packet.File.Name.Length == 0)
							fstruct.Name = Path.GetRandomFileName();
						if (File.Exists(fstruct.Name))
							fileStream = new FileStream(fstruct.Name, FileMode.Truncate);
						else
							fileStream = new FileStream(fstruct.Name, FileMode.CreateNew);
						fileStream.Write(fstruct.Data, 0, fstruct.Data.Length);
						fileStream.Close();

						_textWriter.WriteLine($"File {fstruct.Name} recieved");
						break;

					//Clear operation
					case PacketType.Flags.File | PacketType.Flags.Start:
						throw new NotImplementedException();
					case PacketType.Flags.File | PacketType.Flags.End:
						//Clear operation
						throw new NotImplementedException();
					case PacketType.Flags.File:
						throw new NotImplementedException();

					case PacketType.Flags.Group:
						throw new NotImplementedException();

					case PacketType.Flags.None:
						throw new ArgumentNullException("Packet's has no flags");
				}

			}
			catch (Exception e)
			{
				return new Packet(
					PacketType.Flags.RspSingleErr,
					e.Message + " | {" + packet + "}",
					packet.Id);
			}

			return new Packet(
				PacketType.Flags.RspSingleMsg,
				_sResponses_[packet.Flags] + ":: {" + packet + "}",
				packet.Id);
		}
	}
}
