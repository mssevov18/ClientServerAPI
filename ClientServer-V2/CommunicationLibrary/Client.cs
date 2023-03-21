using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommunicationLibrary
{
    public class Client
    {
        private TcpClient _client;
        private NetworkStream _nStream;
        public bool IsConnected => _isConnected;
        private bool _isConnected;

        private TextReader _textReader; // Console.In
        private TextWriter _textWriter; // Console.Out
        private Encoding _encoding; // Console.OutputEncoding

        private Thread _receiveThread;

        private Queue<Packet> _packQueue = new Queue<Packet>();
        private Queue<uint> _packIdQueue = new Queue<uint>();

        private PacketHandler _handler;

        public Client(TextReader textReader, TextWriter textWriter, Encoding encoding)
        {
            _client = new TcpClient();
            _isConnected = false;

            _textReader = textReader;
            _textWriter = textWriter;
            _encoding = encoding;

            _handler = new PacketHandler(encoding, textWriter);
        }

        /// <summary>
        /// Attempt connecting to a server on the ipAddress and port
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public void Connect(string ipAddress, int port) =>
            Connect(IPAddress.Parse(ipAddress), port);
        /// <summary>
        /// Attempt connecting to a server on the ipAddress and port
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public void Connect(IPAddress ipAddress, int port)
        {
            try
            {
                _client.Connect(ipAddress, port);
                _nStream = _client.GetStream();
                _isConnected = true;

                //_cThread = new Thread(HandleCommunication);
                //_cThread.Start();

                _receiveThread = new Thread(ReceiverLoop);
                _receiveThread.Start();
            }
            catch (Exception)
            {
#warning Exception needs handling
                _isConnected = false;
                throw;
            }
        }

        public void Disconnect()
        {
            if (!_isConnected)
                return;

            try
            {
                _isConnected = false;
                _client.Close();
                _nStream.Close();
            }
            catch (Exception)
            {
#warning Exception needs handling

                throw;
            }
        }


        //this should handle receiving data...
        private void ReceiverLoop()
        {
            try
            {
                //Handle incoming messages....
                while (_isConnected)
                {
                    // I think im overdoing this...
                    // The bank system needs a connection
                    // Not a 0ms 24/7 back and forth chat service ):
                    //kkdkkdkdkkdkdkmsdfmsdlkfmsldkfmslmflkmgvkmpoimcoinomvrotnvonec
                    _handler.Handle(Packet.GetPacketFromNetworkStream(_nStream));
                }
            }
            catch (IOException)
            {
                _textWriter.WriteLine("Server terminated");
            }
        }

        /// <summary>
        /// The Sending Loop -> Started as a Thread by Send() functions.
        /// </summary>
        private void SendLoop()
        {
            Packet curPacket;

            while (_isConnected && _packQueue.Count != 0)
            {
                curPacket = _packQueue.Dequeue();
                _packIdQueue.Enqueue(curPacket.Id);
                _nStream.Write(
                    curPacket.ToByteArray(),
                    0,
                    Packet.__zHeaderSize + curPacket.Size);
                _nStream.Flush();
            }
        }

        //TODO: Handle Server shutdown
        //TODO: Handle User disconnect
        //TODO: Start + End File
        //TODO: User Recieve Data

        /// <summary>
        /// Send a string
        /// </summary>
        /// <param name="data"></param>
        public void SendMsg(string data) =>
            SendMsg(_encoding.GetBytes(data));

        public void SendMsg(byte[] data)
        {
            if (data.Length > Packet.__MsgMaxSize)
                SendLongMsg(data);
            else
                SendPacket(new Packet(PacketType.SingleMsg, data));
        }


        //TODO: UPDATE SendLongMsg
        public void SendLongMsg(byte[] bytes)
        {
            int dataLength = bytes.Length;
            ushort iterations = 0;
            while (dataLength > 0)
            {
                byte[] tempBuffer = new byte[Packet.__MsgMaxSize];

                Buffer.BlockCopy(bytes, iterations * Packet.__MsgMaxSize,
                                 tempBuffer, 0,
                                 Packet.__MsgMaxSize);


                PacketType.Flags flags = PacketType.Flags.Message;

                if (iterations == 0)
                    flags |= PacketType.Flags.Start;
                else
                {
                    //TODO: Check if MsgMaxSize is checked against the correct size!
                    if (dataLength <= Packet.__MsgMaxSize)
                        flags |= PacketType.Flags.End;
                    else
                        flags = PacketType.Flags.Message;
                }

                _packQueue.Enqueue(new Packet(PacketType.StartMsg, tempBuffer));

                iterations++;
                dataLength -= Packet.__MsgMaxSize;
            }
        }

        public void SendFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            byte[] bytes = new byte[fileInfo.Length];

            foreach (string line in File.ReadAllLines(path))
            {
                byte[] buffer = _encoding.GetBytes(line);
                Buffer.BlockCopy(buffer, 0, bytes, bytes.Length, buffer.Length);
            }

            SendFile(path.Split('\\').LastOrDefault(), bytes);
        }
        public void SendFile(string name, byte[] fileBytes)
        {
            if (fileBytes.Length > Packet.__MsgMaxSize)
                SendLongFile(fileBytes);
            else

                SendFile(new FileStruct(name, fileBytes));
        }
        public void SendFile(FileStruct fileStruct) =>
            SendPacket(new Packet(fileStruct));

#warning old method
#warning update
        //TODO: Update method SendLongFile
        public void SendLongFile(byte[] fileBytes)
        {
            int dataLength = fileBytes.Length;
            ushort iterations = 0;
            while (dataLength > 0)
            {
                byte[] tempBuffer = new byte[Packet.__MsgMaxSize];

                Buffer.BlockCopy(fileBytes, iterations * Packet.__MsgMaxSize,
                                 tempBuffer, 0,
                                 Packet.__MsgMaxSize);


                PacketType.Flags flags = PacketType.Flags.File;

                if (iterations == 0)
                    flags |= PacketType.Flags.Start;
                else
                {
                    if (dataLength <= Packet.__MsgMaxSize)
                        flags |= PacketType.Flags.End;
                    else
                        flags = PacketType.Flags.Message;
                }

                _packQueue.Enqueue(new Packet(flags, tempBuffer));

                iterations++;
                dataLength -= Packet.__MsgMaxSize;
            }
        }

        public void SendPacket(Packet packet)
        {
            if (packet.Size != packet.Bytes.Length)
                throw new Exception("Packet size mismatch!");

            _packQueue.Enqueue(packet);

            SendLoop();
        }

        //public void Receive()
        //{
        //    Span<byte> data = new Span<byte>();
        //    _nStream.Read(data);
        //    _textWriter.Write(_encoding.GetString(data));
        //}

    }
}
