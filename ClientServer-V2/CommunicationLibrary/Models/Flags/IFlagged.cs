using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationLibrary.Models.Features;

namespace CommunicationLibrary.Models.Flags
{
	public interface IEnumFlagged<TFlagEnum> : IEnumerated<TFlagEnum> where TFlagEnum : struct, Enum
	{
		public bool HasFlag(TFlagEnum flag);
	}
}
