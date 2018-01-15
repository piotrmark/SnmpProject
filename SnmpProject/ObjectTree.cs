using System;
using System.Collections.Generic;
using System.Linq;

namespace SnmpProject
{
    public class ObjectTree
    {
        private ObjectNode Root { get; }
        private Dictionary<string, DataType> DataTypes { get; }

        private HashSet<string> DistinctTypes { get; }

        public ObjectTree(ICollection<ObjectIdentifier> oids, ICollection<ObjectType> objectTypes, List<DataType> dataTypes)
        {
            DataTypes = new Dictionary<string, DataType>();
            foreach (var dataType in dataTypes)
            {
                DataTypes.Add(dataType.Name, dataType);
            }
            var rootOid = oids.First(o => oids.All(oi => oi.Name != o.Class));
            oids.Remove(rootOid);

            Root = new ObjectNode {ObjectIdentifier = rootOid};
            while (oids.Any())
            {
                var currentOid = oids.First(o => oids.All(oi => oi.Name != o.Class));
                var parent = GetNodeByName(currentOid.Class);
                parent?.Children.Add(new ObjectNode {ObjectIdentifier = currentOid, Parent = parent});
                oids.Remove(currentOid);
            }
            DistinctTypes = new HashSet<string>();
            while (objectTypes.Any())
            {
                var currentType = objectTypes.First();
                if (DataTypes.ContainsKey(currentType.Syntax))
                {
                    DistinctTypes.Add(DataTypes[currentType.Syntax].BaseType);
                    var restrictions = MibParser.GetRestrictionsFromSyntax(DataTypes[currentType.Syntax].Restrictions);
                    if (restrictions != null)
                    {
                        currentType.Min = restrictions.Item1;
                        currentType.Max = restrictions.Item2;
                    }
                }
                else
                {
                    DistinctTypes.Add(currentType.Syntax);
                }
                var parent = GetNodeByName(currentType.Class);
                parent?.Children.Add(new ObjectNode {ObjectType = currentType, Parent = parent});
                objectTypes.Remove(currentType);
            }
        }

        private ObjectNode GetNodeByName(string name) //BFS
        {
            var queue = new Queue<ObjectNode>();
            queue.Enqueue(Root);
            while (queue.Any())
            {
                var currentNode = queue.Dequeue();
                var nodeName = currentNode.ObjectIdentifier != null
                    ? currentNode.ObjectIdentifier.Name
                    : currentNode.ObjectType.Name;
                if (name == nodeName)
                    return currentNode;
                foreach (var child in currentNode.Children)
                    queue.Enqueue(child);
            }
            //throw new Exception("Node not found");
            return null;
        }

        public void Print()
        {
            Root.PrintPretty(string.Empty, true);
        }

        public void PrintObjectInfo(string oid)
        {
            var current = FindInTree(oid);   
            Console.WriteLine(current.ObjectType != null ? current.ObjectType.FullInfo : current.DisplayName);
        }

        public byte[] EncodeObject(string oid, string value)
        {
            var leaf = FindInTree(oid);
            var syntax = DataTypes.ContainsKey(leaf.ObjectType.Syntax)
                ? DataTypes[leaf.ObjectType.Syntax].BaseType
                : leaf.ObjectType.Syntax;
            return BerEncoder.Encode(syntax, value).ToArray();
        }

        public bool ValidateValue(string oid, string value)
        {
            var leaf = FindInTree(oid);
            var syntax = DataTypes.ContainsKey(leaf.ObjectType.Syntax)
                ? DataTypes[leaf.ObjectType.Syntax].BaseType
                : leaf.ObjectType.Syntax;
            if (syntax.ToUpper().Contains("INTEGER"))
            {
                if (!long.TryParse(value, out var n))
                    return false;
                if (n < leaf.ObjectType.Min || n > leaf.ObjectType.Max)
                    return false;
            }
            if (syntax.ToUpper().Contains("STRING"))
            {
                if (value.Length > leaf.ObjectType.Max)
                    return false;
            }
            return true;
        }

        private ObjectNode FindInTree(string oid)
        {
            var path = oid.Split('.');
            var current = Root;
            foreach (var step in path)
            {
                var number = int.Parse(step);
                current = current.Children.FirstOrDefault(n =>
                    n.ObjectIdentifier != null ? n.ObjectIdentifier.Number == number : n.ObjectType.Number == number);
                if (current == null)
                    throw new Exception("Node not found");
            }
            return current;
        }
    }
}
