using System;
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
        public int ActiveNodeId { get; private set; } = -1;
        public BtState ActiveNodeState { get; private set; }
        public float LastNodeTickTime { get; private set; } = -1f;
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
            UpdateBody();
        }

        protected virtual void UpdateBody()
        {
            _traceDepth = -1;
            _hasRunningNode = false;
            ActiveNodeId = -1;
            _btRoot.Tick();
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
        }

        protected void SetDelayTime(float delay)
        {
            _delayUpdate = delay;
            _delayDef = delay;
        }
    }
}