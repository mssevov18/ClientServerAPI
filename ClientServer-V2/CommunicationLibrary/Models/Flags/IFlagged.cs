using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Models.Flags
{
	public interface IFlagged<TFlagEnum> : IEnumerated<TFlagEnum> where TFlagEnum : struct, Enum
	{
		public bool HasFlag(TFlagEnum flag);
	}
}
