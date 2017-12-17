using System;
using System.Collections.Generic;
using System.Linq;

namespace SnmpProject
{
    public static class BerEncoder
    {
        private static readonly Dictionary<string, byte> TagNumbers = new Dictionary<string, byte>
        {
            {"INTEGER", 2},
            {"BIT STRING", 3},
            {"OCTET STRING", 4},
            {"NULL", 5},
            {"OBJECT IDENTIFIER", 6},
            {"SEQUENCE", 16}
        };

        public static List<byte> Encode(string syntax, string value)
        {
            syntax = ClearSyntax(syntax);

            var res = new List<byte>();

            byte identifier = 0; //Class 00 - universal
            if (IsConstructed(syntax))
                identifier |= (1 << 5);
            if (TagNumbers.ContainsKey(syntax))
                identifier |= TagNumbers[syntax];
            res.Add(identifier);

            byte length = 0;

            if (syntax == "INTEGER")
            {
                long n = long.Parse(value);
                var bytes = BitConverter.GetBytes(n);
                length = (byte) bytes.Length;
                res.Add(length);
                bytes = bytes.Reverse().ToArray();
                res.AddRange(bytes);
            }
            else
            {
                var lengthInBytes = value.Length; //Char is 2 bytes, so we ignore half
                if (lengthInBytes <= 127)
                {
                    length |= (byte) lengthInBytes;
                    res.Add(length);
                }
                else
                {
                    byte kOctets = GetNOctets(lengthInBytes);
                    res.Add(kOctets);
                    var bytes = BitConverter.GetBytes(lengthInBytes);
                    for (var i = 0; i < kOctets; i++)
                    {
                        res.Add(bytes[sizeof(int) - kOctets + i]);
                    }
                }

                for (var i = 0; i < lengthInBytes; i++)
                    res.Add(GetNthByte(value, i));
            }

            return res;
        }

        private static bool IsConstructed(string syntax)
        {
            return syntax == "SEQUENCE";
        }

        private static string ClearSyntax(string syntax)
        {
            if (syntax.ToUpper().Contains("INTEGER"))
                return "INTEGER";
            if (syntax.ToUpper().Contains("STRING"))
                return "OCTET STRING";
            if (syntax.ToUpper().Contains("SEQUENCE"))
                return "SEQUENCE";

            return syntax;
        }

        private static byte GetNthByte(string value, int n)
        {
            return (byte) value[n];
        }

        private static byte GetNOctets(int length)
        {
            return (byte) Math.Ceiling((Math.Log(length, 2) + 1) / 8);
        }
    }
}
