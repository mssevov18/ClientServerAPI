using System;
using System.Diagnostics.CodeAnalysis;

namespace CommunicationLibrary
{
    // Remove most unused methods after figuring out what to use..
    public static class PacketType
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
        }

        // if the message is between a startMsg and an endMsg it shouldnt
        // have a continuity flag

        // TwoWay means that the Handler should send the sender a message
        //TODO: Change Short to Single
        public static PacketType.Flags SingleMsg => Flags.SingleMsg;
        public static PacketType.Flags StartMsg => Flags.StartMsg;
        public static PacketType.Flags EndMsg => Flags.EndMsg;


        public static PacketType.Flags SingleFile => Flags.File | Flags.Single;
        public static PacketType.Flags StartFile => Flags.File | Flags.Start;
        public static PacketType.Flags EndFile => Flags.File | Flags.End;

        public static PacketType.Flags SingleErr => Flags.Error | Flags.Single;
        public static PacketType.Flags StartErr => Flags.Error | Flags.Start;
        public static PacketType.Flags EndErr => Flags.Error | Flags.End;

        public static PacketType.Flags RspSingleMsg => Flags.RspSingleMsg;
        public static PacketType.Flags RspShortFile => Flags.File | Flags.Single | Flags.Response;
        public static PacketType.Flags RspShortErr => Flags.Error | Flags.Single | Flags.Response;
    }
}
