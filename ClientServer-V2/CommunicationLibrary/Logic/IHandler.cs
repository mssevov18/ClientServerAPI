using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationLibrary.Models;

namespace CommunicationLibrary.Logic
{
	public interface IHandler<TPacketFlags>
		where TPacketFlags : struct, Enum
	{
		public Encoding Encoding { get; set; }

		/// <summary>
		/// TextWriter -> {StreamWriter, StringWriter}
		/// </summary>
		public TextWriter ResultWriter { get; set; }

		public Type PacketFlagsType { get; }

		public Packet Handle(Packet packet);
	}
}
