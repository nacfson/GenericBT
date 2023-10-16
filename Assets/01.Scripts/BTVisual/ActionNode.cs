using Unity.VisualScripting;
using UnityEngine;
namespace BTVisual
{
    public abstract class ActionNode<T> : Node where T: BTBrain
    {
        protected T _genericBrain;
        public T Brain
        {
            get
            {
                if(_genericBrain == null)
                {
                    _genericBrain = brain as T;
                }
                return _genericBrain;
            }
        }


    }
}