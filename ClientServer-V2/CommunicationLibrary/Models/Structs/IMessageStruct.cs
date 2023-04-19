using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Models.Structs
{
	public interface IMessageStruct<TStruct>
		where TStruct : struct
	{
		public int Length => throw new NotImplementedException();

		public static TStruct GetStruct(Packet packet) => throw new NotImplementedException();
		public static TStruct GetStruct(byte[] bytes) => throw new NotImplementedException();

		public static byte[] GetBytes(TStruct @struct) => throw new NotImplementedException();
	}
}
