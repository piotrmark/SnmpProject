using System;
using System.IO;

namespace SnmpProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "..//..//mibs//ACCOUNTING-CONTROL-MIB";
            var mibFile = File.ReadAllText(path);
            var tree = MibParser.Parse(mibFile);
            tree.Print();
            Console.ReadKey();
        }
    }
}
