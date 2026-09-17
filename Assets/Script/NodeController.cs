using System;
using System.Collections.Generic;
using System.Linq;
using BT.Util;
using UnityEngine;

namespace BT
{
    public enum BtState { FAILUER, SUCCESS, RUNNING }
    public enum BTType { NONE, ROOT, CONDITION, SEQUENCE, SELECTOR }

    [Serializable]
    public class NodeData
    {
        public int id;
        public string typeName;
        public Rect rect;
        public string methodName;
        [SerializeReference] public Node template;
        public List<int> childIds = new List<int>();
    }

    [Serializable]
    public class NodeController
    {
        [SerializeField] private string _key;
        [SerializeField] private List<NodeData> _flatNodeDataList = new List<NodeData>();

        private Dictionary<int, Node> _nodeMap = new Dictionary<int, Node>();
        private RootNode _root;

        public string key => _key;
        public RootNode Root => _root;

        public NodeController(string key) => _key = key;

        [Serializable]
        private class LegacyTreeData
        {
            public List<LegacyNodeData> nodeList = new List<LegacyNodeData>();
        }

        [Serializable]
        private class LegacyNodeData
        {
            public int id;
            public string actionName;
            public int nodeType;
            public int actionType;
            public Rect rect;
            public List<int> childIds = new List<int>();
        }

        public static NodeController FromLegacyJson(string key, string json)
        {
            var legacy = JsonUtility.FromJson<LegacyTreeData>(json);
            if (legacy?.nodeList == null || legacy.nodeList.Count == 0)
                throw new InvalidOperationException("Legacy behavior tree has no nodes.");
            var controller = new NodeController(key);
            foreach (var item in legacy.nodeList)
            {
                string typeName;
                string methodName = null;
                switch (item.nodeType)
                {
                    case 1: typeName = nameof(RootNode); break;
                    case 2: typeName = nameof(SequenceNode); break;
                    case 3: typeName = nameof(SelectorNode); break;
                    case 0:
                        typeName = nameof(ActionNode);
                        var methods = new[] { null, "HpCheck", "Death", "Hit",
                            "Detector", "Attack", "Move", "Idle" };
                        methodName = item.actionType > 0 && item.actionType < methods.Length
                            ? methods[item.actionType] : item.actionName;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown legacy node type: {item.nodeType}");
                }
                controller._flatNodeDataList.Add(new NodeData
                {
                    id = item.id,
                    typeName = typeName,
                    methodName = methodName,
                    rect = item.rect,
                    childIds = item.childIds ?? new List<int>()
                });
            }
            return controller;
        }


#if UNITY_EDITOR
        public void SetNodeList(List<Node> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            _flatNodeDataList.Clear();
            foreach (var node in nodes)
            {
                _flatNodeDataList.Add(new NodeData
                {
                    id = node.id,
                    typeName = node.GetType().AssemblyQualifiedName,
                    rect = node.rect,
                    template = node,
                    methodName = node is ActionNode action ? action.MethodName :
                        node is ConditionNode condition ? condition.MethodName : null,
                    childIds = node.childs.Select(child => child.id).ToList()
                });
            }
        }
#endif

        public void Initialize(AI ai = null)
        {
            if (_flatNodeDataList == null || _flatNodeDataList.Count == 0)
                throw new InvalidOperationException("Behavior tree has no nodes.");

            _root = null;
            _nodeMap = new Dictionary<int, Node>();
            var nodes = new Dictionary<int, Node>();
            foreach (var data in _flatNodeDataList)
            {
                if (data == null || string.IsNullOrWhiteSpace(data.typeName))
                    throw new InvalidOperationException("Behavior tree node type is missing.");
                var type = Utils.GetBTType(data.typeName);
                if (type == null || !typeof(Node).IsAssignableFrom(type) || type.IsAbstract)
                    throw new InvalidOperationException($"Unknown behavior tree node: {data.typeName}");
                var node = (Node)Activator.CreateInstance(type);
                if (data.template != null)
                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.template), node);
                node.SetId(data.id);
#if UNITY_EDITOR
                node.SetRect(data.rect.x, data.rect.y);
#endif
                if (node is ActionNode action) action.MethodName = data.methodName;
                if (node is ConditionNode condition) condition.MethodName = data.methodName;
                if (nodes.ContainsKey(data.id))
                    throw new InvalidOperationException($"Duplicate behavior tree node id: {data.id}");
                nodes.Add(data.id, node);
            }

            var roots = nodes.Values.OfType<RootNode>().ToList();
            if (roots.Count != 1)
                throw new InvalidOperationException("Behavior tree requires exactly one root.");
            var parents = new HashSet<int>();
            foreach (var data in _flatNodeDataList)
            {
                var parent = nodes[data.id];
                foreach (var childId in data.childIds ?? new List<int>())
                {
                    if (!nodes.TryGetValue(childId, out var child))
                        throw new InvalidOperationException($"Missing behavior tree node: {childId}");
                    if (child is RootNode || !parents.Add(childId) || !parent.AddChild(child))
                        throw new InvalidOperationException($"Invalid behavior tree connection: {data.id} -> {childId}");
                }
            }

            var root = roots[0];
            var reachable = root.GetAllNodes();
            if (reachable.Count != nodes.Count)
                throw new InvalidOperationException("Behavior tree contains disconnected nodes.");
            if (ai != null) root.Bind(ai);
            _nodeMap = nodes;
            _root = root;
        }

        public List<Node> GetAllNodes() => _nodeMap.Values.ToList();
    }
}