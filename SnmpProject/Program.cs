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
            while (true)
            {
                var oid = Console.ReadLine();
                tree.PrintObjectInfo(oid);
            }
        }
    }
}
