using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Logic
{
	using System.IO;
	using System.Threading;

	using CommunicationLibrary.Models.Structs;
	using Models;
	using Models.Features;

	public abstract class BaseHandler<TPacketFlags> : IHandler<TPacketFlags>
		where TPacketFlags : struct, Enum
	{
		public Encoding Encoding
		{
			get => _Encoding;
			set
			{
				_Encoding = value;
				Packet.Encoding = _Encoding;
				FileStruct.Encoding = _Encoding;
			}
		}
		protected Encoding _Encoding;
		
		public TextWriter ResultWriter
		{
			get => _ResultWriter;
			set => _ResultWriter =value;
		}
		protected TextWriter _ResultWriter;

		public Type PacketFlagsType => typeof(TPacketFlags);

		public BaseHandler(Encoding encoding, TextWriter textWriter)
		{
			Encoding = encoding;
			ResultWriter = textWriter;
		}
												public abstract LinkedList<Packet> Handle(Packet packet);

		public virtual async Task<Packet> WaitForPacketResponse(Packet packet, int timeout = 1000)
			=> throw new NotImplementedException();
	}
}
