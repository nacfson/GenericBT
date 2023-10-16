using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using BTVisual;
using Unity.VisualScripting;

namespace BTVisual
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Node node;
        public Port input;
        public Port output;
        public Action<NodeView> OnNodeSelected;

        public NodeView(Node node) : base("Assets/01.Scripts/BTVisual/Editor/NodeView/NodeView.uxml")
        {
            this.node = node;
            this.title = node.name;
            this.viewDataKey = node.guid; //고유 아이디 통일
            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();

            Label descLabel = this.Q<Label>("description");
            descLabel.bindingPath = "description";
            descLabel.Bind(new SerializedObject(node)); //이렇게 하면 노드 오브젝트와 바인딩되서 값이 갱신돼
        }

        private void SetupClasses()
        {
            foreach (Type type in BTBrain.typeList)
            {
                if (node.GetType().IsSubclassOf(typeof(ActionNode<>).MakeGenericType(type)))
                {
                    AddToClassList("action");
                }
                else if (node.GetType().IsSubclassOf(typeof(CompositeNode<>).MakeGenericType(type)))
                {
                    AddToClassList("composite");
                }
                else if (node.GetType().IsSubclassOf(typeof(DecoratorNode<>).MakeGenericType(type)))
                {
                    AddToClassList("decorator");
                }
                else if (node is RootNode)
                {
                    AddToClassList("root");
                }
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "BT(SetPosition)");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin; //좌측 상단을 저장함. 좌표는 좌상부터 0, 0임

            EditorUtility.SetDirty(node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }

        private void CreateInputPorts()
        {
            foreach (Type type in BTBrain.typeList)
            {
                //각 노드별로 만들어지는 포트가 달라야한다. 
                if (node.GetType().IsSubclassOf(typeof(ActionNode<>).MakeGenericType(type)))
                {
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                }
                else if (node.GetType().IsSubclassOf(typeof(CompositeNode<>).MakeGenericType(type)))
                {
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                }
                else if (node.GetType().IsSubclassOf(typeof(DecoratorNode<>).MakeGenericType(type)))
                {
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                }
                else if (node is RootNode)
                {
                    //No input
                }

                if (input != null)
                {
                    input.portName = "";
                    //input.style.flexDirection = FlexDirection.Column;
                    inputContainer.Add(input);
                }
            }
        }
        private void CreateOutputPorts()
        {
            //각 노드별로 만들어지는 포트가 달라야한다. 
            if (node is RootNode)
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            }

            foreach (Type type in BTBrain.typeList)
            {
                if (node.GetType().IsSubclassOf(typeof(ActionNode<>).MakeGenericType(type)))
                {
                    //액션노드는 아웃풋이 없다.
                }
                else if (node.GetType().IsSubclassOf(typeof(CompositeNode<>).MakeGenericType(type)))
                {
                    //컴포짓 노드는 여러개의 아웃풋을 가진다.
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                }
                else if (node.GetType().IsSubclassOf(typeof(DecoratorNode<>).MakeGenericType(type)))
                {
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                }

                if (output != null)
                {
                    output.portName = "";
                    //output.style.flexDirection = FlexDirection.ColumnReverse;
                    output.style.marginLeft = new StyleLength(-15);
                    outputContainer.Add(output);
                }
            }
        }

        public void SortChildren()
        {
            var compositeType = node.GetType();
            if(compositeType == typeof(CompositeNode<>))
            {
                var childrenField = compositeType.GetField("children");

                if (childrenField != null)
                {
                    var childrenList = (List<Node>)childrenField.GetValue(node); // children 필드의 값을 가져옵니다.
                    childrenList.Sort((left, right) => left.position.x.CompareTo(right.position.x)); // x 좌표를 기준으로 정렬합니다.
                }
                else
                {
                    // 필드를 찾지 못한 경우 예외처리 또는 기본값을 설정할 수 있습니다.
                    Debug.LogError("Children field not found in composite node.");
                }
            }
        }

        public void UpdateState()
        {
            if (Application.isPlaying)
            {
                RemoveFromClassList("running");
                RemoveFromClassList("failure");
                RemoveFromClassList("success");

                switch (node.state)
                {
                    case Node.State.RUNNING:
                        if (node.started)
                        {
                            AddToClassList("running");
                        }
                        break;
                    case Node.State.FAILURE:
                        AddToClassList("failure");
                        break;
                    case Node.State.SUCCESS:
                        AddToClassList("success");
                        break;
                }
            }
        }
    }
}
