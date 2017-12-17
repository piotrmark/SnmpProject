using System;
using System.IO;

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
    }
}
