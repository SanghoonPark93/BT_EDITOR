using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace BT
{
    [Serializable]
    public abstract partial class Node
    {
        [SerializeField] protected string _typeName = "Node";
        [SerializeField] protected BTType _nodeType = BTType.NONE;
        [NonSerialized] protected List<Node> _childs = new List<Node>();
        [NonSerialized] protected Node _parent;
        [NonSerialized] protected AI _ai;

        public int id { get; private set; }
        public Node parent => _parent;
        public BTType nodeType => _nodeType;
        public List<Node> childs => _childs;
        public string typeName => _typeName;

        public void SetId(int value) => id = value;
        public void SetParent(Node value) => _parent = value;

        public virtual void Bind(AI ai)
        {
            _ai = ai;
            foreach (var child in _childs) child.Bind(ai);
        }

        public abstract BtState GetState();
        public BtState Tick()
        {
            var state = GetState();
            if (_ai != null) _ai.RecordNodeState(this, state);
            return state;
        }

        public virtual void Reset()
        {
            foreach (var child in _childs) child.Reset();
        }

        public bool AddChild(Node child)
        {
            if (child == null || child is RootNode || child == this || child._parent != null ||
                _childs.Contains(child) || child.Contains(this))
                return false;
            _childs.Add(child);
            child._parent = this;
            return true;
        }

        public bool RemoveChild(Node child)
        {
            if (!_childs.Remove(child)) return false;
            child._parent = null;
            child.Reset();
            return true;
        }

        private bool Contains(Node target)
        {
            if (this == target) return true;
            foreach (var child in _childs)
                if (child.Contains(target)) return true;
            return false;
        }

        public List<Node> GetAllNodes()
        {
            var result = new List<Node> { this };
            foreach (var child in _childs) result.AddRange(child.GetAllNodes());
            return result;
        }

#if UNITY_EDITOR
        protected virtual float width => 150f;
        protected virtual float height => 100f;
        public Rect rect { get; private set; }

        public void SetRect(float x, float y) => rect = new Rect(x, y, width, height);

        public int InitializeId(int value = 0)
        {
            id = value;
            _childs = _childs.OrderBy(child => child.rect.x).ToList();
            var lastId = value;
            foreach (var child in _childs) lastId = child.InitializeId(lastId + 1);
            return lastId;
        }

        public virtual void DrawBackground() { }

        public void DrawBackgroundTree()
        {
            DrawBackground();
            foreach (var child in _childs) child.DrawBackgroundTree();
        }

        public void DrawNode()
        {
            DrawDescription();
            foreach (var child in _childs) child.DrawNode();
        }

        public Node FindSelectNode(Vector2 position)
        {
            if (rect.Contains(position)) return this;
            foreach (var child in _childs)
            {
                var found = child.FindSelectNode(position);
                if (found != null) return found;
            }
            return null;
        }

        public void DeleteNode()
        {
            if (_parent != null) _parent.RemoveChild(this);
            _childs.Clear();
        }

        public virtual void DrawDescription()
        {
            var drawn = GUI.Window(id, rect, windowId =>
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(nodeType.ToString());
                EditorGUILayout.EndVertical();
                GUI.DragWindow();
            }, id.ToString());
            SetRect(drawn.x, drawn.y);
        }

        public void PrintChild()
        {
            Debug.Log($"child count : {_childs.Count}");
            foreach (var child in _childs) Debug.Log($"child id : {child.id}");
        }

        public void PrintParent()
        {
            if (_parent != null) Debug.Log(_parent.id);
        }
#endif
    }
}