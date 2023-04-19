using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunicationLibrary.Models.Flags;

namespace CommunicationLibrary.Models.Pairs
{
	public class GenericPair<TEnum>
		where TEnum : struct, Enum
	{
		public TEnum Enum
		{
			get => (TEnum)System.Enum.ToObject(typeof(TEnum), _byte);
			set => Byte = Convert.ToByte(value);
		}
		public byte Byte
		{
			get => _byte;
			set => _byte = value;
		}
		private byte _byte;

		public static implicit operator TEnum(GenericPair<TEnum> pair) => pair.Enum;
		public static implicit operator byte(GenericPair<TEnum> pair) => pair.Byte;

		public static implicit operator GenericPair<TEnum>(TEnum @enum) => new GenericPair<TEnum> { Enum = @enum };
		public static implicit operator GenericPair<TEnum>(byte @byte) => new GenericPair<TEnum> { Byte = @byte };

		public override string ToString() => Enum.ToString();
	}
}
