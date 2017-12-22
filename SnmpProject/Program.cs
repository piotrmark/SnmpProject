using System;
using System.IO;
using System.Linq;

namespace SnmpProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "..//..//mibs//RFC1213-MIB";
            var mibFile = File.ReadAllText(path);
            var tree = MibParser.Parse(mibFile);
            tree.Print();

            //foreach (var type in tree.DistinctTypes)
            //{
            //    Console.WriteLine(type);
            //}

            Console.WriteLine("Decoder test:");
            var test = BerDecoder.Decode(StringToByteArray(Console.ReadLine()));

            while (true)
            {
                Console.WriteLine("Select oid:");
                var oid = Console.ReadLine();
                tree.PrintObjectInfo(oid);
                Console.WriteLine("Input value:");
                var value = Console.ReadLine();
                while (!tree.ValidateValue(oid, value))
                {
                    Console.WriteLine("Value invalid");
                    value = Console.ReadLine();
                }
                Console.WriteLine(tree.EncodeObject(oid, value));
            }
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}
