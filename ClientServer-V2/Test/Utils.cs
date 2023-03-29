using System;
using System.Text;
using CommunicationLibrary.Models;

namespace Test
{
	internal static partial class Utils
    {
        public static void PrintDebug(string expected, string actual)
        {
            Console.WriteLine($"Expected: {{{RemoveControlCharacters(expected)}}}\n" +
                              $"Got:      {{{RemoveControlCharacters(actual)}}}");
        }
        public static void PrintDebug(byte[] expected, byte[] actual, bool asString = true)
        {
            if (asString)
                PrintDebug(ToUTF8String(expected), ToUTF8String(actual));
            else
                PrintDebug(ToString(expected), ToString(actual));
        }

        public static string ToString(byte[] bytes, bool bitwise = false)
        {
            string @out = "";
            foreach (byte B in bytes)
            {
                if (bitwise)
                    @out += $"{ToString(B)} | ";
                else
                    @out += $"{B} ";
            }
            return @out+'\n';
        }
        public static string ToString(byte B)
        {
            return $"{B & 1}{B & 2}{B & 4}{B & 8}{B & 16}{B & 32}{B & 64}{B & 128}";
        }

        public static string RemoveControlCharacters(string inString)
        {
            if (inString == null) return null;
            StringBuilder newString = new StringBuilder();
            char ch;
            for (int i = 0; i < inString.Length; i++)
            {
                ch = inString[i];
                if (!char.IsControl(ch))
                {
                    newString.Append(ch);
                }
            }
            return newString.ToString();
        }

        //TODO: Update it?!
        public static byte[] TrimPacketMessage(Packet packet, byte[] input)
        {
#warning WHAT DO YOU DO?
            if (packet.Size != Packet.__MessageMaxSize__)
            {
                byte[] output = new byte[packet.Size];
                Buffer.BlockCopy(input, Packet.__HeaderSize__, output, 0, packet.Size);

                return output;
            }
            else
                return input;
        }

        public static string ToUTF8String(byte[] bytes)
            => Encoding.UTF8.GetString(bytes);

        public static string ToByteArray(string str)
        {
            string res = "";
            foreach(byte b in Encoding.UTF8.GetBytes(str))
                res += $"{b}, ";
            return res.Remove(res.Length - 2, 2);
        }
    }
}
