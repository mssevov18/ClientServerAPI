using System;

namespace CommunicationLibrary.Models.Features
{
	// Remove most unused methods after figuring out what to use..
	public static class PacketFlags
	{
		[Flags]
		public enum Flags : byte
		{
			None = 0b_0000_0000, // None - 0

			// Type
			Message = 0b_0000_0001,  // Message 1
			Error = 0b_0000_0010,    // Error - 2
			File = 0b_0000_0100,     // File - 4

			// Continuity
			Single = 0b_0001_0000,   // Single - 16 
			Start = 0b_0010_0000,    // Start - 32
			End = 0b_0100_0000,      // End - 64
			Response = 0b_1000_0000, // Response - 128

			// Combination ------
			// Message
			SingleMsg = Single | Message,
			StartMsg = Start | Message,
			EndMsg = End | Message,
			RspSingleMsg = Response | SingleMsg,

			// File
			SingleFile = Single | File,

			// Error
			RspSingleErr = Response | Single | Error,

			// if the message is between a startMsg and an endMsg it shouldnt
			// have a continuity flag

			// TwoWay means that the Handler should send the sender a message
		}

		public static readonly Flags[] Values = (Flags[])Enum.GetValues(typeof(Flags));
	}
}
