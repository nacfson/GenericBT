using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BTVisual
{
    public abstract class CompositeNode<T> : Node where T : BTBrain
    {
        public List<Node> children = new List<Node>();
        
        public override Node Clone()
        {
            CompositeNode<T> node = Instantiate(this);
            node.children = children.ConvertAll(c => c.Clone()); //Linq의 Mapping
            return node;
        }
    }
}