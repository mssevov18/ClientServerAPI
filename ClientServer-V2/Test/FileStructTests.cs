using CommunicationLibrary;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test.Utils;

namespace Test
{
    public class FileStructTests
    {
        private static Encoding _encoding = Encoding.UTF8;

        [SetUp]
        public void SetUp()
        {
            FileStruct.Encoding = _encoding;
        }

        [Test]
        public void ctor()
        {
            FileStruct @struct = new FileStruct(5, "Hello", _encoding.GetBytes("yabadabado"));

            Assert.That(@struct.NameLength, Is.EqualTo(5));
            Assert.That(@struct.Name, Is.EqualTo("Hello"));
            Assert.That(@struct.Length, Is.EqualTo(16));
        }

        [Test]
        public void sm_GetBytes()
        {
            FileStruct @struct = new FileStruct(5, "Hello", _encoding.GetBytes("yabadabado"));

            PrintDebug($"[{@struct.NameLength}] {Utils.ToByteArray(@struct.Name)} -> {{{Utils.ToByteArray("yabadabado")}}}", "dont read");
            Assert.That(FileStruct.GetBytes(@struct), Is.EqualTo(new byte[]
            { 5, 72, 101, 108, 108, 111, 121, 97, 98, 97, 100, 97, 98, 97, 100, 111}));
        }

        [Test]
        public void sm_GetStruct()
        {
            byte[] bytes = new byte[]
            { 5, 72, 101, 108, 108, 111, 121, 97, 98, 97, 100, 97, 98, 97, 100, 111};

            FileStruct @struct = new FileStruct(5, "Hello", _encoding.GetBytes("yabadabado"));

            Assert.That(bytes, Is.EqualTo(FileStruct.GetBytes(@struct)));
        }
    }
}
