using CommunicationLibrary;
using System.Reflection;
using System.Text;

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
                {"tavan", "" }
            };

            Server server = new Server(Console.Out, Console.OutputEncoding);
            //server.Start(IPAddress.Any, 50000);
            server.Start(ip["cbwifi"], 50000);
        }
    }
}
