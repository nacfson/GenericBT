using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

namespace BTVisual
{
    [CreateAssetMenu(menuName = "BehviourTree/Tree")]
    public class BehaviourTree : UnityEngine.ScriptableObject
    {
        public Node treeRoot;
        public Node.State treeState = Node.State.RUNNING;
        public List<Node> nodes = new List<Node>();
        public BlackBoard blackboard = new BlackBoard();

        public Node.State Update()
        {
            if (treeRoot.state == Node.State.RUNNING)
            {
                treeState = treeRoot.Update();
            }
            return treeState;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 해당 타입의 노드를 생성함. 생성된 노드는 타입을 이름으로 받고, GUID를 생성하여 받음.
        /// </summary>
        /// <param name="type">노드 타입을 받도록 되어있음.</param>
        /// <returns>생성된 노드객체를 반환</returns>
        public Node CreateNode(System.Type type)
        {
            var node = UnityEngine.ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "BT (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "BT (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        /// <summary>
        /// 지정된 노드를 삭제함
        /// </summary>
        /// <param name="node">삭제하고자 하는 노드</param>
        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "BT (DeleteNode)");
            nodes.Remove(node);
            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child)
        {
            foreach (var type in BTBrain.typeList)
            {
                //패런트의 타입에 따라 다르게 넣어줘야 한다.
                var isDecorator = parent.GetType().IsSubclassOf(typeof(DecoratorNode<>).MakeGenericType(type));
                var rootNode = parent as RootNode;
                var isComposite = parent.GetType().IsSubclassOf(typeof(CompositeNode<>).MakeGenericType(type));

                if (isDecorator) //데코레이터 노드라면
                {
                    Undo.RecordObject(parent, "BT (AddChild)");

                    var childField = parent.GetType().GetField("child"); // child라는 이름의 필드를 찾습니다.

                    if (childField != null)
                    {
                        Debug.Log(childField.GetValue(parent));
                        childField.SetValue(parent, child); // 필드에 값을 설정합니다.
                        //Debug.LogError(childField.GetValue("child"));
                    }
                    else
                    {
                        // 필드를 찾지 못한 경우 예외처리 또는 기본값을 설정할 수 있습니다.
                        Debug.LogError("Child field not found in decorator node.");
                    }

                    EditorUtility.SetDirty(parent);
                    return;
                }

                if (rootNode != null)
                {
                    Undo.RecordObject(rootNode, "BT (AddChild)");
                    rootNode.child = child;
                    EditorUtility.SetDirty(rootNode);
                }


                if (isComposite) //데코레이터 노드라면
                {
                    Undo.RecordObject(parent, "BT (AddChild)");

                    var childrenField = parent.GetType().GetField("children");

                    if (childrenField != null)
                    {
                        var childrenList = (List<Node>)childrenField.GetValue(parent); // children 필드의 값을 가져옵니다.
                        childrenList.Add(child); // x 좌표를 기준으로 정렬합니다.
                    }
                    else
                    {
                        // 필드를 찾지 못한 경우 예외처리 또는 기본값을 설정할 수 있습니다.
                        Debug.LogError("Children field not found in composite node.");
                    }

                    //decorator.child = child;
                    EditorUtility.SetDirty(parent);
                    return;
                }

            }

        }

        public void RemoveChild(Node parent, Node child)
        {
            foreach (var type in BTBrain.typeList)
            {
                //패런트의 타입에 따라 다르게 삭제
                var isDecorator = parent.GetType().IsSubclassOf(typeof(DecoratorNode<>).MakeGenericType(type));
                var rootNode = parent as RootNode;
                var isComposite = parent.GetType().IsSubclassOf(typeof(CompositeNode<>).MakeGenericType(type));
                if (isDecorator) //데코레이터 노드라면
                {
                    UnityEngine.Object decorator = parent.ConvertTo(type) as Object;
                    Undo.RecordObject(decorator, "BT (AddChild)");

                    var childField = type.GetField("child"); // child라는 이름의 필드를 찾습니다.

                    if (childField != null)
                    {
                        childField.SetValue(decorator, null); // 필드에 값을 설정합니다.
                    }
                    else
                    {
                        // 필드를 찾지 못한 경우 예외처리 또는 기본값을 설정할 수 있습니다.
                        Debug.LogError("Child field not found in decorator node.");
                    }

                    EditorUtility.SetDirty(decorator);
                    return;
                }
                if (rootNode != null)
                {
                    Undo.RecordObject(rootNode, "BT (RemoveChild)");
                    rootNode.child = null;
                    EditorUtility.SetDirty(rootNode);
                    return;
                }

                if (isComposite)
                {
                    UnityEngine.Object decorator = parent.ConvertTo(type) as Object;
                    Undo.RecordObject(decorator, "BT (AddChild)");

                    var compositeType = parent.GetType();
                    var childrenField = compositeType.GetField("children");

                    if (childrenField != null)
                    {
                        var childrenList = (List<Node>)childrenField.GetValue(parent); // children 필드의 값을 가져옵니다.
                        childrenList.Remove(child); // x 좌표를 기준으로 정렬합니다.
                    }
                    else
                    {
                        // 필드를 찾지 못한 경우 예외처리 또는 기본값을 설정할 수 있습니다.
                        Debug.LogError("Children field not found in composite node.");
                    }

                    //decorator.child = child;
                    EditorUtility.SetDirty(decorator);
                    return;
                }
            }
        }

        public List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();

            foreach (var type in BTBrain.typeList)
            {
                var isComposite = parent.GetType().IsSubclassOf(typeof(CompositeNode<>).MakeGenericType(type));
                if (isComposite) //콤포짓 노드라면
                {
                    List<Node> childrenList = null;

                    var getChildren = parent.GetType().GetField("children");
                    if (getChildren != null)
                    {
                        var list = (List<Node>)getChildren.GetValue(parent);
                        list.ForEach(n => children.Add(n));
                    }
                }

                var rootNode = parent as RootNode;
                if (rootNode != null && rootNode.child != null)
                {
                    children.Add(rootNode.child);
                }

                var isDecorator = parent.GetType().IsSubclassOf(typeof(DecoratorNode<>).MakeGenericType(type));
                if (isDecorator) //데코레이터 노드라면
                {
                    var getChild = parent.GetType().GetField("child");
                    if (getChild != null)
                    {
                        var node = (Node)getChild.GetValue(parent);
                        children.Add(node);
                    }
                }
            }

            return children;
        }
#endif
        public void Traverse(Node node, System.Action<Node> visitor)
        {
            //노드를 순회하면서 각 노드들을 tree.nodes 리스트에 넣어주는 함수
            if (node)
            {
                visitor.Invoke(node);
                var children = GetChildren(node);
                children.ForEach(n => Traverse(n, visitor));
            }
        }

        public BehaviourTree Clone(BTBrain brain)
        {
            var tree = Instantiate(this);
            tree.treeRoot = tree.treeRoot.Clone();
            //트리 리스트에 있던 노드들도 새롭게 클로닝 된 노드들로 변경되어야 한다.
            tree.nodes = new List<Node>();
            Traverse(tree.treeRoot, n =>
            {
                n.brain = brain;
                tree.nodes.Add(n);
            });
            return tree;
        }

        public void Bind(Context context, BTBrain brain)
        {
            Traverse(treeRoot, n =>
            {
                n.blackboard = blackboard;
                n.context = context;
                n.SetBrain(brain);
            });
        }
    }

}