using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class ServerProgram
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Server";

            Server server = new Server(Console.Out, Console.OutputEncoding);

            const string sevovIp = "192.168.1.175";
            const string tavanIp = "";
            const string cbwifiIp = "10.2.2.117";

            //server.Start(IPAddress.Any, 50000);
            server.Start(cbwifiIp, 50000);
        }
    }
}
