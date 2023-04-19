using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Models.Features
{
	public interface IEnumerated<TEnum> where TEnum : struct, Enum
	{
		public static readonly TEnum[] EnumValues;
	}
}
