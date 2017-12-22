using System;
using System.Collections.Generic;
using System.Linq;

namespace SnmpProject
{
    public static class BerDecoder
    {
        public static DecodedTypeNode Decode(byte[] bytes)
        {
            return DecodeObject(bytes, null);
        }

        private static DecodedTypeNode DecodeObject(byte[] bytes, DecodedTypeNode parent)
        {
            var res = new DecodedType();

            var current = bytes;
            while (current.Any())
            {
                var identifier = current[0];
                if ((identifier & 1 << 7) == 0)
                {
                    res.Visibility = (identifier & 1 << 6) == 0 ? VisibilitClass.Universal : VisibilitClass.Application;
                }
                else
                {
                    res.Visibility = (identifier & 1 << 6) == 0
                        ? VisibilitClass.ContextSpecific
                        : VisibilitClass.Private;
                }

                res.IsConstructed = (identifier & 1 << 5) != 0;
                res.TypeTagId = identifier & 31;

                long length;
                int dataStart;
                if (current[1] <= 127)
                {
                    length = current[1];
                    dataStart = 2;
                }
                else
                {
                    length = BitConverter.ToInt64(current.Skip(2).Take(current[1] & 127).ToArray(), 0);
                    dataStart = current[1] & 127 + 2;
                }
                res.Length = length;
                res.Data = current.Skip(dataStart).Take((int) length).ToArray();

                var node = new DecodedTypeNode {Value = res, Children = new List<DecodedTypeNode>()};
                if (res.IsConstructed) //sequence
                {
                    DecodeObject(current.Skip(dataStart).Take((int)length).ToArray(), node);
                }
                if (parent != null)
                {
                    node.Parent = parent;
                    parent.Children.Add(node);
                }
                else
                {
                    return node;    //Only root returns a value
                }
                
                current = current.Skip(dataStart + (int) length).ToArray();
            }

            return null;
        }
    }
}
