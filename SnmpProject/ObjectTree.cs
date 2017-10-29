using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnmpProject
{
    public class ObjectTree
    {
        public ObjectNode Root { get; set; }

        public ObjectTree(ObjectIdentifier moduleIdentity, ICollection<ObjectIdentifier> oids,
            ICollection<ObjectType> objectTypes)
        {
            var rootOid = moduleIdentity;
            Root = new ObjectNode {ObjectIdentifier = rootOid};
            while (oids.Any())
            {
                var currentOid = oids.First();
                var parent = GetNodeByName(currentOid.Class);
                parent.Children.Add(new ObjectNode {ObjectIdentifier = currentOid, Parent = parent});
                oids.Remove(currentOid);
            }
            while (objectTypes.Any())
            {
                var currentType = objectTypes.First();
                var parent = GetNodeByName(currentType.Class);
                parent.Children.Add(new ObjectNode {ObjectType = currentType, Parent = parent});
                objectTypes.Remove(currentType);
            }
        }

        public ObjectNode GetNodeByName(string name) //BFS
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
            throw new Exception("Node not found");
        }

        public void Print()
        {
            Root.PrintPretty(string.Empty, true);
        }
    }
}
