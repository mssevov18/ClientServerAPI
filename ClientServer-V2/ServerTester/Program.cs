using System.Text;

using CommunicationLibrary.EndPoints;
using CommunicationLibrary.Logic;

namespace ServerTester
{
	internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Server testing";
            Dictionary<string, string> ip = new Dictionary<string, string>
            {
                {"sevov", "192.168.1.175" },
                {"cbwifi", "10.2.2.117" },
				{"acerpc", "192.168.0.222" },
				{"tavan", "192.168.0.175" }
			};

			Server server = new Server(Console.Out, new PacketHandler(Console.OutputEncoding, Console.Out));
            //server.Start(IPAddress.Any, 50000);
            server.Start(ip["tavan"], 50000);
        }
    }
}
