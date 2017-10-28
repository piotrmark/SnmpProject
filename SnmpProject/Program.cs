using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnmpProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "..//..//mibs//ACCOUNTING-CONTROL-MIB";
            var mibFile = File.ReadAllText(path);
            MibParser.Parse(mibFile);

        }
    }
}
