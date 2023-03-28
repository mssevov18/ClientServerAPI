using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Logic
{
	using Models;
	using Models.Features;

	public abstract class BaseHandler : IHandler
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

		public BaseHandler(Encoding encoding) => Encoding = encoding;

		public abstract Packet Handle(Packet packet);
	}
}
