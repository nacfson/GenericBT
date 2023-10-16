using UnityEngine;

namespace BTVisual
{
    public abstract class DecoratorNode<T> : Node where T: BTBrain
    {
        public Node child;
        
        public override Node Clone()
        {
            DecoratorNode<T> node = Instantiate(this);
            node.child = child.Clone(); //차일드도 클로닝
            return node;
        }
    }
    
}