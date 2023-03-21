using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationLibrary
{
    public class Server
    {
        private TcpListener _server;
        private IPAddress _ipAddress;
        private int _port;
        public bool IsRunning => _isRunning;
        private bool _isRunning;

        private TextWriter _textWriter;
        private Encoding _encoding;

        private PacketHandler _handler;

        public Server(TextWriter writer, Encoding encoding)
        {
            _textWriter = writer;
            _encoding = encoding;

            _handler = new PacketHandler(encoding, writer);
        }

        public void Start(string ipAddress, int port)
        {
            Start(IPAddress.Parse(ipAddress), port);
        }

        public void Start(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _server = new TcpListener(_ipAddress, _port);
            _server.Start();
            _isRunning = true;

            LoopClients();
        }

        public void Stop()
        {
            _server.Stop();
            _isRunning = false;
        }


        public void LoopClients()
        {
            while (_isRunning)
            {
                // wait for client connection
                TcpClient newClient = _server.AcceptTcpClient();

                // client found.
                // create a thread to handle communication
                //Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                Thread t = new Thread(new ParameterizedThreadStart(HandleClientPackets));
                t.Start(newClient);
            }
        }

        public void HandleClientPackets(object obj)
        {
            try
            {
                // retrieve client from parameter passed to thread
                TcpClient client = (TcpClient)obj;

                NetworkStream network = client.GetStream();

                bool bClientConnected = true;

                Packet packet, response;
#warning write encoding handshake
                //TODO: write encoding handshake
                while (bClientConnected)
                {
                    packet = Packet.GetPacketFromNetworkStream(network);

                    response = _handler.Handle(packet);

                    //TODO: Replies with TwoWay flag
                    if (response.Flags.HasFlag(PacketType.Flags.Response))
                    {
                        network.Write(
                            response.ToByteArray(), 
                            0, 
                            Packet.__zHeaderSize + response.Size);
                        network.Flush();


                        //throw new NotImplementedException();
                    }

                    // to write something back.
                    // sWriter.WriteLine("Meaningfull things here");
                    // sWriter.Flush();
                }
            }
            catch (IOException)
            {
                _textWriter.WriteLine($"Client Disconnected");
            }

        }
    }
}
