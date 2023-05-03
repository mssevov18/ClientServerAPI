using System;
using System.Net.Sockets;

using CommunicationLibrary.Models;

namespace CommunicationLibrary.Logic
{
	public class PacketEventArgs : EventArgs
	{
		public Packet Packet { get; private set; }

		public PacketEventArgs(Packet packet)
		{
			Packet = packet;
		}

		public static implicit operator PacketEventArgs(Packet packet) => new PacketEventArgs(packet);
	}
}
