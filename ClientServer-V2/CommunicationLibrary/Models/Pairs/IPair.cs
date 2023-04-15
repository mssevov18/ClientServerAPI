using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Models.Pairs
{
	public interface IEnumPair<TEnum> : IEnumerated<TEnum> where TEnum : struct, Enum
	{
		public TEnum Enum { get; set; }
		public byte Byte { get; set; }

	}
}
