using System;
using System.Collections.Generic;

namespace SnmpProject
{
    public class ObjectNode
    {
        public ObjectIdentifier ObjectIdentifier { get; set; }
        public ObjectType ObjectType { get; set; }
        public List<ObjectNode> Children { get; set; }
        public ObjectNode Parent { get; set; }

        public string DisplayName => ObjectIdentifier != null
            ? $"{ObjectIdentifier.Number}: {ObjectIdentifier.Name}"
            : $"{ObjectType.Number}: {ObjectType.Name}";

        public ObjectNode()
        {
            Children = new List<ObjectNode>();
        }

        public void PrintPretty(string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }
            Console.WriteLine(DisplayName);

            for (int i = 0; i < Children.Count; i++)
                Children[i].PrintPretty(indent, i == Children.Count - 1);
        }
    }
}
