using System.Collections.Generic;

namespace SnmpProject
{
    public class DecodedTypeNode
    {
        public DecodedTypeNode Parent { get; set; }
        public List<DecodedTypeNode> Children { get; set; }
        public DecodedType Value { get; set; }
    }
}
