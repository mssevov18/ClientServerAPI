using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary
{
    public class PacketHandler
    {
        private Dictionary<PacketType.Flags, string> _sResponses = new Dictionary<PacketType.Flags, string>()
        {
            { PacketType.Flags.Message, "Message" },
            { PacketType.Flags.Message | PacketType.Flags.Single, "Single Message"},
            { PacketType.Flags.Message | PacketType.Flags.Start, "Start Message"},
            { PacketType.Flags.Message | PacketType.Flags.End, "End Message"},
            { PacketType.Flags.Message | PacketType.Flags.Single | PacketType.Flags.Response, "Single Response Message"},

            { PacketType.Flags.Error | PacketType.Flags.Single, "Single Error" },
            { PacketType.Flags.Error | PacketType.Flags.Single | PacketType.Flags.Response, "Single Error Response" },

            { PacketType.Flags.File | PacketType.Flags.Single, "Single File" },
            { PacketType.Flags.File | PacketType.Flags.Start, "Start File" },
            { PacketType.Flags.File | PacketType.Flags.End, "End File" }
        };
        //TODO: Change the Handle responses to this dictionary -> handling becomes modular
        private Dictionary<PacketType.Flags, Action> _responses = new();
        //private Dictionary<PacketType.Flags, Func<object[], object>> _responses = new();
        //private Dictionary<PacketType.Flags, Action<object[], object>> _responses = new();
        private List<byte[]> _longBuffer = new List<byte[]>();
        private FileStream _file;

        public PacketHandler(Encoding encoding, TextWriter textWriter)
        {
            Encoding = encoding;
            TextWriter = textWriter;
        }

        public void ChangeResponses(PacketType.Flags flags, string response)
        {
            if (!_sResponses.ContainsKey(flags))
                return;

            _sResponses[flags] = response;
        }
        public void SetResponses(Dictionary<PacketType.Flags, string> responses)
        => _sResponses = responses;

        public Encoding Encoding
        {
            get => _encoding;
            set
            {
                _encoding = value;
                //Packet.SetEncoding(_encoding);
                Packet.Encoding = _encoding;
                FileStruct.Encoding = _encoding;
            }
        }
        private Encoding? _encoding;
        public TextWriter TextWriter
        {
            get => _textWriter;
            set => _textWriter = value;
        }
        private TextWriter? _textWriter;

        #region OldHandle-KeepSafe
        //public void Handle(byte[] packetBytes)
        //{
        //    Handle(new Packet(packetBytes));
        //}

        //public void Handle(Packet packet)
        //{
        //    if (_textWriter == null || _encoding == null)
        //        throw new Exception("either _textWriter or _encoding is null");

        //    switch (packet.Flags)
        //    {
        //        //=============
        //        //Messages
        //        case PacketType.Flags.Message | PacketType.Flags.Single:
        //            _textWriter.WriteLine(_encoding.GetString(packet.Bytes));
        //            break;

        //        case PacketType.Flags.Message | PacketType.Flags.Start:
        //            _longBuffer.Add(packet.Bytes);
        //            break;

        //        case PacketType.Flags.Message | PacketType.Flags.End:
        //            _longBuffer.Add(packet.Bytes);

        //            foreach (byte[] bytes in _longBuffer)
        //                _textWriter.Write(_encoding.GetString(bytes));
        //            _textWriter.WriteLine();
        //            _longBuffer.Clear();
        //            break;

        //        // If the _longBuffer is in use, then the
        //        // base messages are added to it
        //        case PacketType.Flags.Message:
        //            if (_longBuffer.Count > 0)
        //                _longBuffer.Add(packet.Bytes);
        //            break;

        //        //=============
        //        //Files
        //        //TODO: FILES
        //        //https://code-maze.com/convert-byte-array-to-file-csharp/
        //        case PacketType.Flags.File | PacketType.Flags.Single:
        //            break;
        //        case PacketType.Flags.File | PacketType.Flags.Start:
        //            break;
        //        case PacketType.Flags.File | PacketType.Flags.End:
        //            break;
        //        case PacketType.Flags.File:
        //            break;
        //    }
        //}
        #endregion

        public Packet Handle(byte[] packetBytes) => Handle(new Packet(packetBytes));
        public Packet Handle(Packet packet)
        {
            try
            {
                if (_textWriter == null)
                    throw new Exception("_textWriter is null");
                if (_encoding == null)
                    throw new Exception("_encoding is null");

#warning FINISH THIS YOU PIECE OF SHIT <3
#warning bez heit

                //_responses[packet.Flags]

                switch (packet.Flags)
                {
                    //=============
                    //Responses
                    //TODO: Responses
                    case PacketType.Flags.Response:
                    case PacketType.Flags.RspSingleMsg:
                        _textWriter.WriteLine(packet.ToString());

                        break;


                    //=============
                    //Messages
                    case PacketType.Flags.SingleMsg:
                        //Clear operation
                        _textWriter.WriteLine(_encoding.GetString(packet.Bytes));

                        break;
                    case PacketType.Flags.StartMsg:
                        _longBuffer.Add(packet.Bytes);

                        break;

                    case PacketType.Flags.EndMsg:
                        //Clear operation
                        _longBuffer.Add(packet.Bytes);

                        foreach (byte[] bytes in _longBuffer)
                            _textWriter.Write(_encoding.GetString(bytes));
                        _textWriter.WriteLine();

                        _longBuffer.Clear();

                        break;

                    // If the _longBuffer is in use, then the
                    // base messages are added to it
                    case PacketType.Flags.Message:
                        if (_longBuffer.Count > 0)
                            _longBuffer.Add(packet.Bytes);

                        break;

                    //=============
                    //Files
                    //TODO: FILES
                    //https://code-maze.com/convert-byte-array-to-file-csharp/
                    case PacketType.Flags.File | PacketType.Flags.Single:
#warning (:
#warning dovurshi go
#warning pederas takuv
#warning tuka ima mn heit
                        FileStruct fstruct = packet.File;
                        if (packet.File.Name.Length == 0)
                            fstruct.Name = Path.GetRandomFileName();
                        if (File.Exists(fstruct.Name))
                            _file = new FileStream(fstruct.Name, FileMode.Truncate);
                        else
                            _file = new FileStream(fstruct.Name, FileMode.CreateNew);
                        _file.Write(fstruct.Data, 0, fstruct.Data.Length);
                        _file.Close();

                        _textWriter.WriteLine($"File {fstruct.Name} recieved");
                        break;

                        //Clear operation
                    case PacketType.Flags.File | PacketType.Flags.Start:
                        throw new NotImplementedException();
                    case PacketType.Flags.File | PacketType.Flags.End:
                        //Clear operation
                        throw new NotImplementedException();
                    case PacketType.Flags.File:
                        throw new NotImplementedException();

                    case PacketType.Flags.None:
                        throw new ArgumentNullException("Packet's has no flags");
                }

            }
            catch (Exception e)
            {
                return new Packet(
                    PacketType.RspShortErr,
                    e.Message + " | {" + packet + "}",
                    packet.Id);
            }

            return new Packet(
                PacketType.RspSingleMsg,
                _sResponses[packet.Flags] + ":: {" + packet + "}",
                packet.Id);
        }
    }
}
