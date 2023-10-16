using System.Collections.Generic;
using UnityEngine;
using System;

namespace BTVisual
{
    public abstract class BTBrain : MonoBehaviour
    {
        //This is generic brain type add list
        public static List<Type> typeList = new List<Type>() { typeof(BTBrain) };

        [SerializeField] private BehaviourTree _tree;
        public BehaviourTree Tree => _tree;
        
        [SerializeField] protected Transform _target;
        public Transform Target => _target;

        protected virtual void Awake()
        {
            _tree = _tree.Clone(this); //복제해서 시작함.
            var context = Context.CreateFromGameObject(gameObject);
            _tree.Bind(context, this); //만약 EnemyBrain과 같은 녀석을 여기서 바인드해서 넣어줘야 한다면 수정
        }
        protected virtual void Update()
        {
            _tree.Update();
        }
    }
}

