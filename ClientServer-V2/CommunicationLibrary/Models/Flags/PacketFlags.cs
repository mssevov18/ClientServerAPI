using System;

namespace CommunicationLibrary.Models.Flags
{
	[Flags]
	public enum PacketFlags : byte
	{
		None = 0b_0000_0000, 
				Message = 0b_0000_0001,  		Error = 0b_0000_0010,    		File = 0b_0000_0100,     
				Single = 0b_0001_0000,   		Start = 0b_0010_0000,    		End = 0b_0100_0000,      		Response = 0b_1000_0000, 
						SingleMsg = Single | Message,
		StartMsg = Start | Message,
		EndMsg = End | Message,
		RspSingleMsg = Response | SingleMsg,

				SingleFile = Single | File,

				RspSingleErr = Response | Single | Error,
	}
}
