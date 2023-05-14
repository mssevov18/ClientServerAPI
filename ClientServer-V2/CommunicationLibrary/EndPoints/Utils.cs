using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.EndPoints
{
	public static class Utils
	{
		public static string RemoveControlCharacters(string inString)
		{
			if (inString == null)
				return null;
			StringBuilder newString = new StringBuilder();
			char ch;
			for (int i = 0; i < inString.Length; i++)
			{
				ch = inString[i];
				if (!char.IsControl(ch))
				{
					newString.Append(ch);
				}
			}
			return newString.ToString();
		}

		public static string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
				if (ip.AddressFamily == AddressFamily.InterNetwork)
					return ip.ToString();
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
}