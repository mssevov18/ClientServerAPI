using System;

namespace CommunicationLibrary
{
	public static class LibraryInfo
	{
		public static string Version => $"{MajorVersion}.{MinorVersion}.{Patch}";
		public static readonly int MajorVersion = 1;
		public static readonly int MinorVersion = 2;
		public static readonly int Patch = 4;

		public static readonly DateTime BuildDate = new DateTime(2023, 3, 23, 14, 40, 0);

		public static readonly string[] RoadMapFeatures = new string[]
		{
			"Encoding Handshake (Client - Server)",
			"Handle Start/End File",
			"Handle Single/Start/End Error",
			"Cleanup code",
			"More unit tests",
			"Support .png/.jpg",
			"Support .pdf",
			"Support .docx"
		};

		public static readonly string RepositoryLink = "https://github.com/mssevov18/ClientServerAPI";
	}
}
