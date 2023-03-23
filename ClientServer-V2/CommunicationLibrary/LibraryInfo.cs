using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CommunicationLibrary
{
	public static class LibraryInfo
	{
		public static string Version => $"{MajorVersion}.{MinorVersion}.{BuildVersion}";
		public static readonly int MajorVersion = 0;
		public static readonly int MinorVersion = 7;
		public static readonly int BuildVersion = 9;

		public static readonly DateTime BuildDate = new DateTime(2023, 3, 23);

		public static readonly string[] RoadMapFeatures = new string[]
		{
			"Encoding Handshake (Client - Server)",
			"Handle Start/End File",
			"Handle Single/Start/End Error",
			"More unit tests",
			"Support .png/.jpg",
			"Support .pdf",
			"Support .docx"
		};

		public static readonly string RepositoryLink = "https://github.com/mssevov18/ClientServerAPI";
	}
}
