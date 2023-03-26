using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace CommunicationLibrary
{
    public class Packet : IPacket
    {
        public static uint PacketGenCount => _packetGenCount;
        protected static uint _packetGenCount = 1;

        public static Encoding Encoding;

        public static readonly ushort __MsgMaxSize = ushort.MaxValue;
        public static readonly byte __zHeaderSize = 7;

        public PacketType.Flags Flags
        {
            get => flags;
            set => flags = value;
        }
        protected PacketType.Flags flags;

        public ushort Size
        {
            get => size;
            set => size = value;
        }
        protected ushort size;

        public uint Id
        {
            get => id;
            set
            {
                if (flags.HasFlag(PacketType.Flags.Response))
                    id = value;
            }
        }
        protected uint id;

        public byte[] Bytes
        {
            get => bytes;
            set
            {
                if (value.Length > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException($"{nameof(value)} is too large");
                bytes = value;
                size = (ushort)bytes.Length;
            }
        }
        public string Message
        {
            get => Encoding.GetString(bytes);
            set => Bytes = Encoding.GetBytes(value);
        }
        protected byte[] bytes;

        public FileStruct File
        {
            get
            {
                if (flags.HasFlag(PacketType.Flags.File))
                    return ToFile();

                throw new DataMisalignedException("Packet is not a file!");
                return new FileStruct();
            }
        }

        /// <summary>
        /// Default constructor.
        /// If id is 0 (optional), the packet's id will autogenerate
        /// </summary>
        /// <param name="id"></param>
        public Packet(uint id = 0)
        {
            Flags = 0;
            Bytes = new byte[0];
            _PacketGen(Flags, id);
        }
        /// <summary>
        /// 
        /// If id is 0 (optional), the packet's id will autogenerate
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        public Packet(PacketType.Flags flags, string message, uint id = 0)
        {
            if (Encoding == null)
                throw new Exception("Encoding can't be null!");

            Flags = flags;
            Message = message;
            _PacketGen(Flags, id);
        }
        /// <summary>
        /// 
        /// If id is 0 (optional), the packet's id will autogenerate
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="msgBytes"></param>
        /// <param name="id"></param>
        public Packet(PacketType.Flags flags, byte[] msgBytes, uint id = 0)
        {
            Flags = flags;
            Bytes = msgBytes;
            _PacketGen(Flags, id);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputBytes"></param>
        public Packet(byte[] inputBytes)
        {
            Flags = (PacketType.Flags)inputBytes[0];
            Size = BitConverter.ToUInt16(new byte[2] { inputBytes[1],
                                                       inputBytes[2] });
            Bytes = new ArraySegment<byte>(inputBytes, __zHeaderSize, Size).ToArray();
            _PacketGen(flags,
                BitConverter.ToUInt32(new byte[4] { inputBytes[3],
                                                     inputBytes[4],
                                                     inputBytes[5],
                                                     inputBytes[6]}));
        }
        /// <summary>
        /// 
        /// If id is 0 (optional), the packet's id will autogenerate
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <param name="id"></param>
        public Packet(byte[] inputBytes, uint id = 0)
        {
            Flags = (PacketType.Flags)inputBytes[0];
            Size = BitConverter.ToUInt16(new byte[2] { inputBytes[1],
                                                       inputBytes[2] });
            Bytes = new ArraySegment<byte>(inputBytes, __zHeaderSize, Size).ToArray();
            _PacketGen(Flags, id);
        }
        /// <summary>
        /// 
        /// If id is 0 (optional), the packet's id will autogenerate
        /// </summary>
        /// <param name="fileStruct"></param>
        /// <param name="id"></param>
        public Packet(FileStruct fileStruct, uint id = 0)
        {
            if (fileStruct.Length > __MsgMaxSize)
                throw new ArgumentOutOfRangeException();

            Flags = PacketType.Flags.SingleFile;
            //fileStruct.NameLength;

            Bytes = FileStruct.GetBytes(fileStruct);

            _PacketGen(Flags, id);
        }

        /// <inheritdoc />
        private void _PacketGen(PacketType.Flags flags = PacketType.Flags.None, uint id = 0)
        {
            if (id == 0)
                this.id = _packetGenCount++;
            else
                this.id = id;

            //if (flags.HasFlag(PacketType.Flags.File))
            //    fileStruct = FileStruct.GetStruct(bytes);
        }

        /// <inheritdoc />
        public void Clear()
        {
            Flags = PacketType.Flags.None;
            Size = 0;
            Array.Clear(Bytes, 0, Bytes.Length);
        }

        /// <inheritdoc />
        public byte[] ToByteArray()
        {
            byte[] result = new byte[__zHeaderSize + Bytes.Length];

            Buffer.SetByte(result, 0, (byte)Flags);

            Buffer.BlockCopy(BitConverter.GetBytes(Size), 0,
                             result, 1, 2);

            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, result, 3, 4);

            Buffer.BlockCopy(Bytes, 0,
                             result, __zHeaderSize, Bytes.Length);

            return result;
        }

        /// <inheritdoc />
        public FileStruct ToFile()
        {
            return FileStruct.GetStruct(bytes);
        }

        /// <inheritdoc />
        public string WriteToFile(string directoryPath, string fileName = null)
        {
            // generate a random file name
            if (fileName == null)
                fileName = Path.GetRandomFileName();

            // combine the directory path and file name to create a full file path
            string filePath = Path.Combine(directoryPath, fileName);

            // write the bytes to the file
            //FileStruct = FileStruct.GetStruct(bytes);

            System.IO.File.WriteAllBytes(filePath, File.Data);

            // return the full file path
            return filePath;
        }

        /// <summary>
        /// Returns a packet from a StreamReader object.
        /// </summary>
        /// <param name="reader">A StreamReader object that contains the packet data.</param>
        /// <returns>A Packet object that represents the packet.</returns>
        public static Packet GetPacketFromStreamReader(StreamReader reader)
        {
            Span<byte> buffer = Encoding.GetBytes(reader.ReadToEnd());

            return new Packet((PacketType.Flags)buffer[0],
                buffer.Slice(7, BitConverter.ToUInt16(buffer.Slice(1, 2))).ToArray(),
                BitConverter.ToUInt32(buffer.Slice(3, 4)));
        }

        /// <summary>
        /// Returns a packet from a NetworkStream object.
        /// </summary>
        /// <param name="network">A NetworkStream object that contains the packet data.</param>
        /// <returns>A Packet object that represents the packet.</returns>
        public static Packet GetPacketFromNetworkStream(NetworkStream network)
        {
            byte[] buffer = new byte[Packet.__zHeaderSize];

            // Get the first 7 Bytes to create the header
            PacketType.Flags flags =    //   Flags - 1B
                (PacketType.Flags)network.ReadByte();
            network.Read(buffer, 0, 2); //   Size  - 2B
            ushort size = BitConverter.ToUInt16(buffer, 0);
            network.Read(buffer, 0, 4); //   Id    - 4B
            uint id = BitConverter.ToUInt32(buffer, 0);

            buffer = new byte[size];
            network.Read(buffer, 0, size);
            byte[] bytes = buffer;

            return new Packet(flags, bytes, id);
        }

        public override string ToString() => $"#{Id} {Flags}[{Size}] {{ {Encoding.GetString(Bytes)} }}";
    }
}
