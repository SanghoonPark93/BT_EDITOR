using System;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class AI : MonoBehaviour
    {
        protected RootNode _btRoot;
        protected BtState _stateCache;
        protected bool _isStop;
        private float _delayUpdate = -1f;
        private float _delayDef;
        private int _traceDepth = -1;
        private bool _hasRunningNode;
        private bool _hasNodeException;
        private readonly Dictionary<int, NodeExecution> _nodeExecutions =
            new Dictionary<int, NodeExecution>();
        private struct NodeExecution
        {
            public BtState State;
            public float Time;
        }
        public int ActiveNodeId { get; private set; } = -1;
        public BtState ActiveNodeState { get; private set; }
        public float LastNodeTickTime { get; private set; } = -1f;
        public string LastNodeError { get; private set; }
        public NodeScriptableObject TreeAsset => _treeAsset;
        public string TreeKey => _treeKey;
        public NodeScriptableObject ActiveTreeAsset { get; private set; }
        public string ActiveTreeKey { get; private set; }

        [SerializeField] private NodeScriptableObject _treeAsset;
        [SerializeField] private string _treeKey;
        [SerializeField] private bool _initializeAutomatically = true;

        protected virtual bool AutoInitialize => _initializeAutomatically;

        protected virtual void Start()
        {
            if (AutoInitialize && _treeAsset != null && _btRoot == null)
                Initialize(_treeKey);
        }

        public void ConfigureTree(NodeScriptableObject treeAsset, string key)
        {
            _treeAsset = treeAsset;
            _treeKey = key;
        }

        public void SetTree(NodeScriptableObject treeAsset, string key)
        {
            _treeAsset = treeAsset;
            _treeKey = key;
            Initialize(key);
        }
        // Legacy hooks remain virtual so existing AI subclasses keep compiling.
        public virtual BtState HpCheck(bool isFirstTurn) => BtState.FAILUER;
        public virtual BtState Death(bool isFirstTurn) => BtState.FAILUER;
        public virtual BtState Hit(bool isFirstTurn) => BtState.FAILUER;
        public virtual BtState Detector(bool isFirstTurn) => BtState.FAILUER;
        public virtual BtState Attack(bool isFirstTurn) => BtState.FAILUER;
        public virtual BtState Move(bool isFirstTurn) => BtState.FAILUER;
        public virtual BtState Idle(bool isFirstTurn) => BtState.FAILUER;
        public virtual void Initialize(string key)
        {
            _btRoot = null;
            ActiveTreeAsset = null;
            ActiveTreeKey = null;
            ActiveNodeId = -1;
            LastNodeTickTime = -1f;
            LastNodeError = null;
            _traceDepth = -1;
            _hasRunningNode = false;
            _hasNodeException = false;
            _nodeExecutions.Clear();
            _isStop = false;
            if (string.IsNullOrWhiteSpace(key)) key = gameObject.name;
            var asset = _treeAsset != null ? _treeAsset :
                Resources.Load<NodeScriptableObject>("NodeScriptableObject");
            var controller = asset == null ? null : asset.GetNodeController(key);
            Exception loadError = null;

            if (controller != null)
            {
                try
                {
                    controller.Initialize(this);
                    _btRoot = controller.Root;
                    ActiveTreeAsset = asset;
                    ActiveTreeKey = key;
                    return;
                }
                catch (Exception exception)
                {
                    loadError = exception;
                }
            }

            Debug.LogError($"Failed to initialize behavior tree '{key}': " +
                (loadError?.Message ?? "No saved tree was found."), this);
        }
        protected virtual void Update()
        {
            if (_isStop || _btRoot == null) return;
            if (_delayUpdate >= 0f)
            {
                _delayUpdate -= Time.deltaTime;
                if (_delayUpdate > 0f) return;
                _delayUpdate = _delayDef;
            }
            try
            {
                UpdateBody();
            }
            catch (Exception exception)
            {
                _isStop = true;
                if (string.IsNullOrEmpty(LastNodeError))
                {
                    ActiveNodeId = -1;
                    LastNodeError = exception.Message;
                    LastNodeTickTime = Time.realtimeSinceStartup;
                }
                Debug.LogError($"Behavior tree '{ActiveTreeKey}' stopped at node " +
                    $"{ActiveNodeId}: {exception}", this);
            }
        }

        protected virtual void UpdateBody()
        {
            _traceDepth = -1;
            _hasRunningNode = false;
            _hasNodeException = false;
            ActiveNodeId = -1;
            LastNodeError = null;
            _btRoot.Tick();
        }

        internal void RecordNodeException(Node node, Exception exception)
        {
            if (_hasNodeException) return;
            _hasNodeException = true;
            ActiveNodeId = node.id;
            ActiveNodeState = BtState.FAILUER;
            LastNodeError = $"{node.GetType().Name} (#{node.id}): {exception.Message}";
            LastNodeTickTime = Time.realtimeSinceStartup;
            _nodeExecutions[node.id] = new NodeExecution
            {
                State = BtState.FAILUER,
                Time = LastNodeTickTime
            };
        }

        public bool TryGetRecentNodeState(int nodeId, out BtState state)
        {
            if (_nodeExecutions.TryGetValue(nodeId, out var execution) &&
                Time.realtimeSinceStartup - execution.Time < 0.75f)
            {
                state = execution.State;
                return true;
            }
            state = default;
            return false;
        }

        internal void RecordNodeState(Node node, BtState state)
        {
            var depth = 0;
            for (var parent = node.parent; parent != null; parent = parent.parent) depth++;
            if (state == BtState.RUNNING)
            {
                if (!_hasRunningNode || depth >= _traceDepth)
                {
                    ActiveNodeId = node.id;
                    ActiveNodeState = state;
                    _traceDepth = depth;
                }
                _hasRunningNode = true;
            }
            else if (!_hasRunningNode && depth >= _traceDepth)
            {
                ActiveNodeId = node.id;
                ActiveNodeState = state;
                _traceDepth = depth;
            }
            LastNodeTickTime = Time.realtimeSinceStartup;
            _nodeExecutions[node.id] = new NodeExecution
            {
                State = state,
                Time = LastNodeTickTime
            };
        }

        protected void SetDelayTime(float delay)
        {
            _delayUpdate = delay;
            _delayDef = delay;
        }
    }
}
