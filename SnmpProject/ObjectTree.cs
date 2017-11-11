using System;
using System.Collections.Generic;
using System.Linq;

namespace SnmpProject
{
    public class ObjectTree
    {
        public ObjectNode Root { get; }
        public Dictionary<string, DataType> DataTypes { get; }

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
            while (objectTypes.Any())
            {
                var currentType = objectTypes.First();
                if (DataTypes.ContainsKey(currentType.Syntax))
                {
                    var restrictions = MibParser.GetRestrictionsFromSyntax(DataTypes[currentType.Syntax].Restrictions);
                    if (restrictions != null)
                    {
                        currentType.Min = restrictions.Item1;
                        currentType.Max = restrictions.Item2;
                    }
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
            Console.WriteLine(current.ObjectType != null ? current.ObjectType.FullInfo : current.DisplayName);
        }
    }
}
