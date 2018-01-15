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
            var request = StringToByteArray(Console.ReadLine());    //TODO: Get from socket
            var decodedRequest = BerDecoder.Decode(request);
            var snmpPduNode = decodedRequest.Children[2];
            var varBindNode = snmpPduNode.Children[3].Children[0];
            var testOidNode = varBindNode.Children[0].Value;
            var testOidBytes = testOidNode.Data;
            var testOid = BerDecoder.DecodeOid(testOidBytes);
            Console.WriteLine(testOid);
            tree.PrintObjectInfo(testOid);

            if (snmpPduNode.Value.TypeTagId == 3) //SetRequest
            {
                var valueNode = varBindNode.Children[1];
                var value = BerDecoder.DecodeValue(valueNode.Value);
                if (!tree.ValidateValue(testOid, value))
                {
                    Console.WriteLine("Value invalid");
                    //TODO: code error in response
                }
            }

            var testValue = "test"; //TODO: get real value
            var encodedValue = tree.EncodeObject(testOid, testValue);
            var responseBytes = new byte[request.Length + encodedValue.Length - 2];
            Array.Copy(request, responseBytes, request.Length);
            Array.Copy(encodedValue, 0, responseBytes, request.Length - 2, encodedValue.Length);

            var idx = request.Length - 5 - testOidNode.Length;
            responseBytes[idx] += (byte) (encodedValue.Length - 2); //Varbind
            idx -= 2;
            responseBytes[idx] += (byte)(encodedValue.Length - 2);  //Varbind List
            idx -= 11;
            responseBytes[idx] += (byte)(encodedValue.Length - 2);  //SNMP PDU
            responseBytes[1] += (byte)(encodedValue.Length - 2);  //SNMP Message
            //TODO: send response

            Console.WriteLine(BitConverter.ToString(responseBytes));
            var decodedResponse = BerDecoder.Decode(responseBytes);

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
