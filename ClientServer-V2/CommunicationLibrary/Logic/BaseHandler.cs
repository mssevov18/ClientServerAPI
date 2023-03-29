using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Logic
{
	using System.IO;

	using Models;
	using Models.Features;

	public abstract class BaseHandler<TPacketFlags> : IHandler<TPacketFlags>
		where TPacketFlags :
					struct, Enum
	{
		public Encoding Encoding
		{
			get => encoding;
			set
			{
				encoding = value;
				Packet._Encoding = encoding;
				FileStruct.Encoding = encoding;
			}
		}
		protected Encoding encoding;
		
		public TextWriter ResultWriter
		{
			get => resultWriter;
			set => resultWriter =value;
		}
		protected TextWriter resultWriter;

		public Type PacketFlagsType => typeof(TPacketFlags);

		public BaseHandler(Encoding encoding, TextWriter textWriter)
		{
			Encoding = encoding;
			ResultWriter = textWriter;
		}
		/// <summary>
		/// Returns the response packet
		/// </summary>
		/// <param name="packet"></param>
		/// <returns></returns>
		public abstract Packet Handle(Packet packet);
	}
}
