using System;
using System.Reflection;

namespace CommunicationLibrary.Metadata
{
	public static class LibraryInfo
	{
		public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

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
