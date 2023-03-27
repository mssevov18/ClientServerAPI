using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunicationLibrary.Models;

namespace CommunicationLibrary.Logic
{
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

		public abstract Packet Handle(Packet packet);
	}
}
