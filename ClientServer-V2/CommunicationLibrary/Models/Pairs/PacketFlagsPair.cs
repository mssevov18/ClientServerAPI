using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunicationLibrary.Models.Features;
using CommunicationLibrary.Models.Flags;

namespace CommunicationLibrary.Models.Pairs
{
	public class PacketFlagsPair : IPair<PacketFlags>, IFlagged<PacketFlags>
	{
		public PacketFlags Enum
		{
			get => (PacketFlags)Byte;
			set => Byte = (byte)value;
		}
		public byte Byte
		{
			get => @byte;
			set => @byte = value;
		}
		private byte @byte;

		public static implicit operator PacketFlags(PacketFlagsPair pair) => pair.Enum;
		public static implicit operator byte(PacketFlagsPair pair) => pair.Byte;

		public static implicit operator PacketFlagsPair(PacketFlags @enum) => new PacketFlagsPair { Enum = @enum };
		public static implicit operator PacketFlagsPair(byte @byte) => new PacketFlagsPair { Byte = @byte };

		public static readonly PacketFlags[] FlagValues = (PacketFlags[])System.Enum.GetValues(typeof(PacketFlags));

		public bool HasFlag(PacketFlags value) => Enum.HasFlag(value);

		public override string ToString() => Enum.ToString();
	}
}
