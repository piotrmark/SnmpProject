using System;

namespace SnmpProject
{
    public class ObjectType
    {
        public string Name { get; set; }
        public string Syntax { get; set; }
        public string Access { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string Class { get; set; }
        public int Number { get; set; }
        public long? Min { get; set; }
        public long? Max { get; set; }

        public string FullInfo => "Name: " + Name + Environment.NewLine + "Syntax: " + Syntax + Environment.NewLine +
                                  "Min: " + Min + " Max: " + Max + Environment.NewLine +
                                  "Access: " +
                                  Access + Environment.NewLine + "Status: " + Status + Environment.NewLine +
                                  "Description: " +
                                  Description + Environment.NewLine;
    }
}
