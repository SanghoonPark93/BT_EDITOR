#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using BT.Util;
using System.IO;
using System;

namespace BT
{
	public enum ActionState
	{
		AddNode,
		AddCondition,
		AddSequence,
		AddSelector,
		Debug_child,
		Debug_parent,
		Connect
	};

	public class BehaviorTreeRenderer : EditorWindow
	{
		private readonly string SCRIPTABLE_OBJ_PATH = "Assets/Resources/NodeScriptableObject.asset";
		private enum MouseButtonState
		{
			Left = 0,
			Right = 1
		}

		/// <summary>
		/// 노드 새로 생성 후 트리에 속하기 직전까지 임시 보관 용 리스트
		/// </summary>
		private List<Node> _tempNodes = new List<Node>();
		
		private Vector2 _mousePos;

		private Node _prevSelect;
		private Node _curSelect;
		private Node _root;

		private bool _searchingForConnectedNode = false;
		private bool _layoutRequested;
		
		private AI _target;
		private NodeScriptableObject _scriptableObj;
		private NodeScriptableObject scriptableObj 
		{
			get 
			{
				if(_scriptableObj == null) 
				{
					_scriptableObj = AssetDatabase.LoadAssetAtPath<NodeScriptableObject>(SCRIPTABLE_OBJ_PATH);

					if(_scriptableObj == null)
						Debug.LogWarning($"No ScriptableObject found at {SCRIPTABLE_OBJ_PATH}");
				}				

				return _scriptableObj;
			}
			set 
			{
				_scriptableObj = value;
			}
		}

		private bool isSearchingTargetNode => (_searchingForConnectedNode == true && _prevSelect != null);

		[MenuItem("Window/Behavior Tree")]
		private static void Initialize()
		{
			var window = GetWindow<BehaviorTreeRenderer>();
			window.titleContent = new GUIContent("Behavior Tree");
			window.Show();
		}

        private Vector2 _scrollPosition;
        private string _treeKey = string.Empty;
        private int _lastFocusedNodeId = -1;
        private double _nextDebugRepaint;
        private Node _dragNode;
        private int _lastRenderedNodeId = -1;
        private BtState _lastRenderedNodeState;
        private string _lastRenderedNodeError;
        private bool _lastRenderedAnimating;
        private float _lastRenderedTickTime = -1f;
        private bool _lastRenderedRecentActivity;

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.update += OnEditorUpdate;
            OnSelectionChanged();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnSelectionChanged()
        {
            var selected = Selection.activeGameObject == null ? null :
                Selection.activeGameObject.GetComponentInParent<AI>();
            if(selected == null || (selected == _target && _root != null)) return;
            SelectTarget(selected);
        }

        private void SelectTarget(AI selected)
        {
            _target = selected;
            var asset = Application.isPlaying ? selected.ActiveTreeAsset ?? selected.TreeAsset :
                selected.TreeAsset;
            scriptableObj = asset != null ? asset :
                Resources.Load<NodeScriptableObject>("NodeScriptableObject");
            _treeKey = Application.isPlaying ? selected.ActiveTreeKey ?? selected.TreeKey :
                selected.TreeKey;
            if(string.IsNullOrWhiteSpace(_treeKey))
                _treeKey = scriptableObj?.GetOnlyTreeKey() ?? selected.gameObject.name;
            _lastFocusedNodeId = -1;
            Load(false);
            Repaint();
        }

        private void OnEditorUpdate()
        {
            if(!Application.isPlaying || _target == null) return;
            if(_target.ActiveTreeAsset != null &&
                (_root == null || scriptableObj != _target.ActiveTreeAsset ||
                    _treeKey != _target.ActiveTreeKey))
            {
                scriptableObj = _target.ActiveTreeAsset;
                _treeKey = _target.ActiveTreeKey;
                _lastFocusedNodeId = -1;
                Load(false);
            }
            if(EditorApplication.timeSinceStartup < _nextDebugRepaint) return;
            var animating = _target.ActiveNodeState == BtState.RUNNING &&
                Time.realtimeSinceStartup - _target.LastNodeTickTime < 0.75f;
            var recentActivity = _target.LastNodeTickTime >= 0f &&
                Time.realtimeSinceStartup - _target.LastNodeTickTime < 0.75f;
            var changed = _lastRenderedNodeId != _target.ActiveNodeId ||
                _lastRenderedNodeState != _target.ActiveNodeState ||
                _lastRenderedNodeError != _target.LastNodeError ||
                _lastRenderedAnimating != animating ||
                _lastRenderedRecentActivity != recentActivity ||
                (recentActivity && _lastRenderedTickTime != _target.LastNodeTickTime);
            if(!animating && !changed) return;
            _lastRenderedNodeId = _target.ActiveNodeId;
            _lastRenderedNodeState = _target.ActiveNodeState;
            _lastRenderedNodeError = _target.LastNodeError;
            _lastRenderedAnimating = animating;
            _lastRenderedRecentActivity = recentActivity;
            _lastRenderedTickTime = _target.LastNodeTickTime;
            _nextDebugRepaint = EditorApplication.timeSinceStartup +
                (animating ? 1d / 30d : 0.1d);
            Repaint();
        }

        private Node GetDebugNode()
        {
            if(!Application.isPlaying || _target == null || _root == null ||
                _target.ActiveTreeAsset != scriptableObj ||
                _target.ActiveTreeKey != GetTreeKey())
                return null;
            if(_target.ActiveNodeId < 0 ||
                _target.ActiveNodeState == BtState.FAILUER)
                return _root;
            return _root.GetAllNodes().FirstOrDefault(node => node.id == _target.ActiveNodeId)
                ?? _root;
        }

        private static void DrawDebugHighlight(Node node, BtState state,
            bool animate, bool rootFallback)
        {
            var r = new Rect(node.rect.x - 5f, node.rect.y - 5f,
                node.rect.width + 10f, node.rect.height + 10f);
            var previous = Handles.color;
            Handles.color = rootFallback ? new Color(0.78f, 0.58f, 0.94f) :
                state == BtState.RUNNING ? Color.yellow :
                state == BtState.SUCCESS ? Color.green : Color.red;
            Handles.DrawAAPolyLine(3f, new[]
            {
                new Vector3(r.x, r.y), new Vector3(r.xMax, r.y),
                new Vector3(r.xMax, r.yMax), new Vector3(r.x, r.yMax),
                new Vector3(r.x, r.y)
            });
            if (animate)
            {
                var phase = (float)EditorApplication.timeSinceStartup;
                var pulse = (Mathf.Sin(phase * 6f) + 1f) * 0.5f;
                var halo = new Rect(r.x - 4f - pulse * 3f, r.y - 4f - pulse * 3f,
                    r.width + 8f + pulse * 6f, r.height + 8f + pulse * 6f);
                Handles.color = new Color(1f, 0.78f, 0.13f, 0.2f + pulse * 0.55f);
                Handles.DrawAAPolyLine(2f + pulse * 2f, new[]
                {
                    new Vector3(halo.x, halo.y), new Vector3(halo.xMax, halo.y),
                    new Vector3(halo.xMax, halo.yMax), new Vector3(halo.x, halo.yMax),
                    new Vector3(halo.x, halo.y)
                });
                Handles.color = Color.yellow;
                Handles.DrawSolidDisc(PointOnBorder(r, Mathf.Repeat(phase * 0.45f, 1f)),
                    Vector3.forward, 4f);
            }
            Handles.color = previous;
        }

        private static Vector3 PointOnBorder(Rect r, float progress)
        {
            var distance = progress * 2f * (r.width + r.height);
            if (distance < r.width) return new Vector3(r.x + distance, r.y);
            distance -= r.width;
            if (distance < r.height) return new Vector3(r.xMax, r.y + distance);
            distance -= r.height;
            if (distance < r.width) return new Vector3(r.xMax - distance, r.yMax);
            distance -= r.width;
            return new Vector3(r.x, r.yMax - distance);
        }

        private void DrawRecentLeafStates(Node activeNode)
        {
            if(!Application.isPlaying || _target == null || _root == null) return;
            foreach(var node in _root.GetAllNodes())
            {
                if(node == activeNode || node.childs.Count != 0 ||
                    !_target.TryGetRecentNodeState(node.id, out var state)) continue;
                var color = state == BtState.RUNNING ? Color.yellow :
                    state == BtState.SUCCESS ? Color.green :
                    new Color(0.65f, 0.65f, 0.65f, 0.85f);
                var rect = node.rect;
                var previous = Handles.color;
                Handles.color = color;
                Handles.DrawAAPolyLine(2f, new[]
                {
                    new Vector3(rect.x - 2f, rect.y - 2f),
                    new Vector3(rect.xMax + 2f, rect.y - 2f),
                    new Vector3(rect.xMax + 2f, rect.yMax + 2f),
                    new Vector3(rect.x - 2f, rect.yMax + 2f),
                    new Vector3(rect.x - 2f, rect.y - 2f)
                });
                Handles.color = previous;
                if(state == BtState.FAILUER) continue;
                var style = new GUIStyle(EditorStyles.miniBoldLabel);
                style.normal.textColor = color;
                GUI.Label(new Rect(rect.x + 5f, rect.yMax - 18f,
                    rect.width - 10f, 16f),
                    state == BtState.RUNNING ? "RUNNING" :
                    "SUCCESS", style);
            }
        }
        private void OnGUI()
        {
            var current = Event.current;
            const float margin = 4f;
            const float rowHeight = 20f;
            const float rowGap = 2f;
            var row = new Rect(margin, margin,
                Mathf.Max(0f, position.width - margin * 2f), rowHeight);
            var selectedTarget = EditorGUI.ObjectField(row, "Target", _target,
                typeof(AI), true) as AI;
            if(selectedTarget != _target)
            {
                if(selectedTarget != null) SelectTarget(selectedTarget);
                else _target = null;
            }
            row.y += rowHeight + rowGap;
            scriptableObj = EditorGUI.ObjectField(row, "Tree Asset", scriptableObj,
                typeof(NodeScriptableObject), false) as NodeScriptableObject;
            row.y += rowHeight + rowGap;
            _treeKey = EditorGUI.TextField(row, "Tree Key", _treeKey);
            row.y += rowHeight + rowGap;
            if(GUI.Button(row, "New Tree Asset")) CreateTreeAsset();
            row.y += rowHeight + rowGap;
            if(GUI.Button(row, "Save")) Save();
            row.y += rowHeight + rowGap;
            if(GUI.Button(row, "Load")) Load();
            row.y += rowHeight + rowGap;
            if(GUI.Button(row, "Import Legacy JSON")) ImportLegacy();
            row.y += rowHeight + rowGap;
            if(GUI.Button(row, "Reset")) Reset();
            row.y += rowHeight + rowGap;
            if(GUI.Button(row, "Print Member")) PrintLog();
            row.y += rowHeight + rowGap;
            var arrange = GUI.Button(row, "Auto Layout");
            row.y += rowHeight + rowGap;
            var debugNode = GetDebugNode();
            var rootFallback = debugNode == _root && debugNode != null &&
                (_target.ActiveNodeId < 0 ||
                    _target.ActiveNodeState == BtState.FAILUER);
            var animating = debugNode != null && !rootFallback &&
                _target.ActiveNodeState == BtState.RUNNING &&
                Time.realtimeSinceStartup - _target.LastNodeTickTime < 0.75f;
            if(Application.isPlaying && _target != null)
            {
                var nodeName = debugNode is ActionNode action &&
                    !string.IsNullOrWhiteSpace(action.MethodName) ? action.MethodName :
                    debugNode?.GetType().Name;
                if(!string.IsNullOrEmpty(_target.LastNodeError))
                {
                    row.height = 36f;
                    EditorGUI.HelpBox(row, _target.LastNodeError, MessageType.Error);
                }
                else
                    EditorGUI.LabelField(row, rootFallback ?
                        "Runtime: ROOT [NO ACTIVE NODE]" :
                        debugNode == null ? "Runtime: no active node" :
                        $"{(animating ? "RUNNING" : "LAST")}  {nodeName}  " +
                        $"[{_target.ActiveNodeState}]", EditorStyles.helpBox);
                row.y += row.height + rowGap;
            }

            var viewport = new Rect(0f, row.y, position.width,
                Mathf.Max(0f, position.height - row.y));
            if(arrange || _layoutRequested)
            {
                _layoutRequested = false;
                AutoLayout(viewport.width);
            }

            var content = GetCanvasBounds(viewport.size);
            if(debugNode != null && debugNode.id != _lastFocusedNodeId)
            {
                _scrollPosition = new Vector2(
                    Mathf.Clamp(debugNode.rect.center.x - viewport.width / 2f, 0f,
                        Mathf.Max(0f, content.width - viewport.width)),
                    Mathf.Clamp(debugNode.rect.center.y - viewport.height / 2f, 0f,
                        Mathf.Max(0f, content.height - viewport.height)));
                _lastFocusedNodeId = debugNode.id;
            }
            else if(debugNode == null)
                _lastFocusedNodeId = -1;
            _mousePos = current.mousePosition - viewport.position + _scrollPosition;
            if(viewport.Contains(current.mousePosition))
            {
                var isSelected = SelectNode();
                if(current.type == EventType.MouseDown)
                {
                    if(current.button == (int)MouseButtonState.Left && isSelected)
                    {
                        if(isSearchingTargetNode) ConnectNode();
                        else if(_mousePos.y <= _curSelect.rect.y + 24f ||
                            _curSelect is RootNode || _curSelect is SelectorNode ||
                            _curSelect is SequenceNode)
                        {
                            _dragNode = _curSelect;
                            current.Use();
                        }
                    }
                    else if(current.button == (int)MouseButtonState.Right)
                    {
                        var menu = new GenericMenu();
                        CreateSubMenu(isSelected, menu);
                        current.Use();
                    }
                }
                else if(current.type == EventType.MouseDrag && _dragNode != null)
                {
                    var rect = _dragNode.rect;
                    _dragNode.SetRect(Mathf.Max(0f, rect.x + current.delta.x),
                        Mathf.Max(0f, rect.y + current.delta.y));
                    current.Use();
                    Repaint();
                }
                else if(current.type == EventType.MouseUp && isSelected)
                    ResetTreeNodesIds();
            }
            if(current.type == EventType.MouseUp) _dragNode = null;

            if(DeleteNode()) return;
            HandleScriptDrag(current, viewport);

            _scrollPosition = GUI.BeginScrollView(viewport, _scrollPosition, content);
            if(isSearchingTargetNode)
            {
                var mouseRect = new Rect(_mousePos.x, _mousePos.y, 10f, 10f);
                DrawArrow(_prevSelect.rect, mouseRect, false);
                Repaint();
            }
            if(_root != null) ConnectChild(_root);
            if(_root != null) _root.DrawBackgroundTree();
            _tempNodes.ForEach(node => node.DrawBackgroundTree());
            if(_root != null) _root.DrawNode();
            _tempNodes.ForEach(node => node.DrawNode());
            DrawRecentLeafStates(debugNode);
            if(debugNode != null)
                DrawDebugHighlight(debugNode, _target.ActiveNodeState,
                    animating, rootFallback);
            GUI.EndScrollView();
        }

        private Rect GetCanvasBounds(Vector2 viewportSize)
        {
            var width = viewportSize.x;
            var height = viewportSize.y;
            if(_root != null)
                foreach(var node in _root.GetAllNodes())
                {
                    width = Mathf.Max(width, node.rect.xMax + 40f);
                    height = Mathf.Max(height, node.rect.yMax + 40f);
                }
            foreach(var temp in _tempNodes)
                foreach(var node in temp.GetAllNodes())
                {
                    width = Mathf.Max(width, node.rect.xMax + 40f);
                    height = Mathf.Max(height, node.rect.yMax + 40f);
                }
            return new Rect(0f, 0f, width, height);
        }
		private static HashSet<string> _runtimeAssemblies;

        private static bool IsUsableNodeType(Type type)
        {
            if(type == null || !typeof(Node).IsAssignableFrom(type) || type.IsAbstract ||
                type.IsGenericType || typeof(RootNode).IsAssignableFrom(type) ||
                type.GetConstructor(Type.EmptyTypes) == null)
                return false;
            if(_runtimeAssemblies == null)
                _runtimeAssemblies = new HashSet<string>(
                    CompilationPipeline.GetAssemblies(AssembliesType.Player)
                        .Select(assembly => assembly.name));
            return _runtimeAssemblies.Contains(type.Assembly.GetName().Name);
        }

        private void HandleScriptDrag(Event current, Rect viewport)
		{
			if(current.type != EventType.DragUpdated && current.type != EventType.DragPerform)
				return;

			if(!viewport.Contains(current.mousePosition))
				return;

			var scripts = DragAndDrop.objectReferences.OfType<MonoScript>()
				.Where(script =>
				{
					var type = script.GetClass();
					return IsUsableNodeType(type);
				})
				.ToArray();

			if(scripts.Length == 0)
				return;

			if((_root != null && _root.FindSelectNode(_mousePos) != null)
					|| _tempNodes.Any(node => node.FindSelectNode(_mousePos) != null))
				return;

			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			if(current.type == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				foreach(var script in scripts)
					OnFileDropped(script);
			}

			current.Use();
		}
        private const float HorizontalGap = 24f;
        private const float VerticalGap = 40f;
        private const float ForestGap = 48f;

        private void AutoLayout(float viewportWidth)
        {
            var roots = new List<Node>();
            if(_root != null) roots.Add(_root);
            roots.AddRange(_tempNodes);
            if(roots.Count == 0) return;

            var widths = new Dictionary<Node, float>();
            var rowHeights = new List<float>();
            foreach(var root in roots)
            {
                MeasureRows(root, 0, rowHeights);
                MeasureWidth(root, widths);
            }

            var rowTops = new float[rowHeights.Count];
            var nextTop = 28f;
            for(var depth = 0; depth < rowHeights.Count; depth++)
            {
                rowTops[depth] = nextTop;
                nextTop += rowHeights[depth] + VerticalGap;
            }

            var totalWidth = roots.Sum(root => widths[root]) + ForestGap * (roots.Count - 1);
            var left = Mathf.Max(24f, (viewportWidth - totalWidth) / 2f);
            foreach(var root in roots)
            {
                PlaceTree(root, left, 0, widths, rowTops);
                left += widths[root] + ForestGap;
            }
            ResetTreeNodesIds();
            _scrollPosition = Vector2.zero;
            Repaint();
        }

        private static void MeasureRows(Node node, int depth, List<float> rowHeights)
        {
            while(rowHeights.Count <= depth) rowHeights.Add(0f);
            rowHeights[depth] = Mathf.Max(rowHeights[depth], node.rect.height);
            foreach(var child in node.childs)
                MeasureRows(child, depth + 1, rowHeights);
        }

        private static float MeasureWidth(Node node, Dictionary<Node, float> widths)
        {
            var childrenWidth = 0f;
            foreach(var child in node.childs)
                childrenWidth += MeasureWidth(child, widths);
            if(node.childs.Count > 1)
                childrenWidth += HorizontalGap * (node.childs.Count - 1);
            var width = Mathf.Max(node.rect.width, childrenWidth);
            widths.Add(node, width);
            return width;
        }

        private static void PlaceTree(Node node, float left, int depth,
            Dictionary<Node, float> widths, float[] rowTops)
        {
            var width = widths[node];
            node.SetRect(left + (width - node.rect.width) / 2f, rowTops[depth]);

            var childrenWidth = node.childs.Sum(child => widths[child]) +
                HorizontalGap * Mathf.Max(0, node.childs.Count - 1);
            var childLeft = left + (width - childrenWidth) / 2f;
            foreach(var child in node.childs)
            {
                PlaceTree(child, childLeft, depth + 1, widths, rowTops);
                childLeft += widths[child] + HorizontalGap;
            }
        }

		private string GetTreeKey()
        {
            var key = string.IsNullOrWhiteSpace(_treeKey) ?
                (_target == null ? null : _target.gameObject.name) : _treeKey.Trim();
            if(string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Enter a Tree Key or assign a Target.");
            return key;
        }

        private void CreateTreeAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Behavior Tree Asset",
                "BehaviorTree", "asset", "Choose where to save the tree asset.");
            if(string.IsNullOrEmpty(path)) return;
            var asset = CreateInstance<NodeScriptableObject>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            scriptableObj = asset;
            Selection.activeObject = asset;
        }

        private void Save()
		{
			if(_root == null || scriptableObj == null)
				return;

			try
			{
				ResetTreeNodesIds();
				var saveList = _root.GetAllNodes();
				var controller = new NodeController(GetTreeKey());
				controller.SetNodeList(saveList);
				controller.Initialize();
				scriptableObj.SetNodeController(controller);
                if(_target != null)
                {
                    Undo.RecordObject(_target, "Assign Behavior Tree");
                    _target.ConfigureTree(scriptableObj, controller.key);
                    EditorUtility.SetDirty(_target);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(_target);
                }
				EditorUtility.SetDirty(scriptableObj);
				AssetDatabase.SaveAssets();
				Debug.Log($"Saved {saveList.Count} nodes");
			}
			catch(Exception exception)
			{
				Debug.LogError($"Failed to save behavior tree: {exception.Message}");
			}
		}
		private void Load(bool reportMissing = true)
		{
			if(scriptableObj == null)
				return;

			try
			{
                var key = GetTreeKey();
                var controller = scriptableObj.GetNodeController(key);
                if(controller == null)
                {
                    Reset();
                    if(reportMissing)
                        Debug.LogWarning($"No saved behavior tree with key '{key}' in asset " +
                            $"'{AssetDatabase.GetAssetPath(scriptableObj)}'. Check Tree Key or save the tree first.");
                    return;
                }
                controller.Initialize();
				var loadedRoot = controller.Root;
				_tempNodes.Clear();
				_prevSelect = null;
				_curSelect = null;
				_searchingForConnectedNode = false;
				_root = loadedRoot;
				ResetTreeNodesIds();
				_layoutRequested = true;
				Debug.Log($"Loaded {_root.GetAllNodes().Count} nodes");
			}
			catch(Exception exception)
			{
				Debug.LogError($"Failed to load behavior tree: {exception.Message}");
			}
		}
		private void ImportLegacy()
		{
			if(TargetIsNull()) return;
			try
			{
				var key = _target.gameObject.name;
				var path = Utils.GetJsonAddress(key);
				if(!Utils.HasJson(path))
					throw new FileNotFoundException("Legacy behavior tree JSON is missing.", path);
				var controller = NodeController.FromLegacyJson(key, Utils.ReadAllText(path));
				controller.Initialize();
				scriptableObj.SetNodeController(controller);
                if(_target != null)
                {
                    Undo.RecordObject(_target, "Assign Behavior Tree");
                    _target.ConfigureTree(scriptableObj, controller.key);
                    EditorUtility.SetDirty(_target);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(_target);
                }
				EditorUtility.SetDirty(scriptableObj);
				AssetDatabase.SaveAssets();
				_tempNodes.Clear();
				_prevSelect = null;
				_curSelect = null;
				_searchingForConnectedNode = false;
				_root = controller.Root;
				ResetTreeNodesIds();
				_layoutRequested = true;
				Debug.Log($"Imported and saved {_root.GetAllNodes().Count} legacy nodes.");
			}
			catch(Exception exception)
			{
				Debug.LogError($"Failed to import legacy tree: {exception.Message}");
			}
		}
		private void SetParentRecursive(Node node)
		{
			foreach(var child in node.childs)
			{
				child.SetParent(node);
				SetParentRecursive(child);
			}
		}

		private void Reset() 
		{			
			_tempNodes.Clear();			
			_prevSelect = null;
			_curSelect = null;
			_root = null;
			_searchingForConnectedNode = false;				
		}
		
		private void PrintLog() 
		{
			Debug.Log($"<color=orange>_tempNodes count</color> : {_tempNodes.Count}");			
			Debug.Log($"<color=orange>_prevSelect</color> : {_prevSelect}");
			Debug.Log($"<color=orange>_curSelect</color> : {_curSelect}");
			Debug.Log($"<color=orange>_root</color> : {_root}");
			Debug.Log($"<color=orange>_isDrawing</color> : {_searchingForConnectedNode}");			
			Debug.Log($"<color=orange>_target</color> : {_target}");
		}

		private bool TargetIsNull()
		{
			if(_target == null)
			{
				Debug.LogError("AI is Null!!");
				return true;
			}

			return false;
		}

		private bool SelectNode()
		{
			_curSelect = _root == null ? null : _root.FindSelectNode(_mousePos);

			if(_curSelect == null)
			{
				//루트에 없었다면 템프리스트에서 탐색
				foreach(var node in _tempNodes)
				{
					var findSelectNode = node.FindSelectNode(_mousePos);
					if(findSelectNode == null)
						continue;

					_curSelect = findSelectNode;
					break;
				}
			}

			return _curSelect != null;
		}

		private void RemoveNode(Node node)
        {
            if(node == null) return;
            if(node == _root)
            {
                _root = null;
                _tempNodes.Clear();
            }
            else
            {
                _tempNodes.Remove(node);
                node.DeleteNode();
            }
            _curSelect = null;
            _prevSelect = null;
            _searchingForConnectedNode = false;
            ResetTreeNodesIds();
            Repaint();
        }

        private bool DeleteNode()
		{
			var e = Event.current;
			if(e.Equals(Event.KeyboardEvent("delete")))
			{
				if(_curSelect == null)
					return false;

				if(_tempNodes.Contains(_curSelect))
					_tempNodes.Remove(_curSelect);

				if(_root == _curSelect) 
				{
					_root = null;
					_tempNodes.Clear();
				}

				_curSelect.DeleteNode();
				_curSelect = null;

				ResetTreeNodesIds();				
				e.Use();

				return true;
			}

			return false;
		}

		private void ConnectChild(Node node)
		{
			foreach(var child in node.childs)
			{
				DrawArrow(node.rect, child.rect);
				ConnectChild(child);
			}
		}

		public static void DrawArrow(Rect start, Rect end, bool isChangePos = true)
		{
			var startPos = new Vector2(start.x + (start.size.x / 2f), start.y + start.size.y);
			var endPos = end.position;

			if(isChangePos == true)
				endPos = new Vector2(end.x + (end.size.x / 2f), end.y - 7);

			start.width = 1;
			start.height = 1;
			end.width = 1;
			end.height = 1;

			Handles.DrawLine(startPos, endPos);

			var font = new GUIStyle
			{
				fontSize = 10,
				richText = true,
			};
						
			var fontPos = new Vector2(endPos.x - 5, endPos.y - 5);
			Handles.Label(fontPos, "<color=#ffffff>●</color>", font);
		}

		private void ConnectNode()
		{
			if(!isSearchingTargetNode || _curSelect == null)
				return;

			if(_prevSelect.AddChild(_curSelect))
			{
				_tempNodes.Remove(_curSelect);
				ResetTreeNodesIds();
			}
			_searchingForConnectedNode = false;
			_prevSelect = null;
			_curSelect = null;
		}
		private void OnFileDropped(MonoScript script)
		{
			var className = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(script));

			Type type = script.GetClass();

			if(!IsUsableNodeType(type))
			{
				Debug.LogError("Invalid Node class!");
				return;
			}

			var node = Activator.CreateInstance(type) as Node;
			CreateNode(_mousePos, node);
		}

		#region SUB_MENU

		private void CreateSubMenu(bool isSelect, GenericMenu menu)
        {
            if(isSelect)
            {
                menu.AddItem(new GUIContent("Connect Node"), false, ContextCallback, ActionState.Connect);
                var selectedNode = _curSelect;
                menu.AddItem(new GUIContent("Delete Node"), false, () => RemoveNode(selectedNode));
                menu.AddItem(new GUIContent("Debug_child"), false, ContextCallback, ActionState.Debug_child);
                menu.AddItem(new GUIContent("Debug_parent"), false, ContextCallback, ActionState.Debug_parent);
            }
            else
            {
                var addPosition = _mousePos;
                menu.AddItem(new GUIContent("Add/Action"), false,
                    () => CreateNode(addPosition, new ActionNode()));
                menu.AddItem(new GUIContent("Add/Condition"), false,
                    () => CreateNode(addPosition, new ConditionNode()));
                menu.AddItem(new GUIContent("Add/Sequence"), false,
                    () => CreateNode(addPosition, new SequenceNode()));
                menu.AddItem(new GUIContent("Add/Selector"), false,
                    () => CreateNode(addPosition, new SelectorNode()));                _curSelect = null;
            }
            menu.ShowAsContext();
        }

        private void ContextCallback(object obj)
		{
			var id = (ActionState)obj;

			switch(id)
			{
				case ActionState.AddNode:
					CreateNode(_mousePos);
					break;

				case ActionState.AddCondition:
				CreateNode(_mousePos, new ConditionNode());
				break;

			case ActionState.AddSequence:
				CreateNode(_mousePos, new SequenceNode());
				break;

			case ActionState.AddSelector:
				CreateNode(_mousePos, new SelectorNode());
				break;

			case ActionState.Debug_child:
					_curSelect.PrintChild();
					break;

				case ActionState.Debug_parent:
					_curSelect.PrintParent();
					break;

				case ActionState.Connect:
					_searchingForConnectedNode = true;
					_prevSelect = _curSelect;
					break;
			}
		}

		private void CreateNode(Vector2 pos, Node node = null)
		{			
			if(_root == null)
			{				
				_root = new RootNode();
				_root.SetRect(pos.x, Mathf.Max(160f, pos.y - 120f));
			}

			if(node == null)
				node = new ActionNode();

			node.SetRect(pos.x, pos.y);
			_tempNodes.Add(node);

			ResetTreeNodesIds();
		}

		#endregion

		private void ResetTreeNodesIds() 
		{
			var nextId = (_root == null) ? 0 : _root.InitializeId() + 1;			
			foreach(var temp in _tempNodes)
			{
				nextId = temp.InitializeId(nextId) + 1;
			}
		}
	}	
}

#endif
