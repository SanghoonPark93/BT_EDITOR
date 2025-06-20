#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BT.Util;
using System.IO;
using System;

namespace BT
{
	public enum ActionState
	{
		AddNode,
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

		[MenuItem("Window/Behavior Tree")]
		private static void Initialize()
		{
			var window = GetWindow<BehaviorTreeRenderer>();
			window.titleContent = new GUIContent("Behavior Tree");
			window.Show();
		}

		private void OnGUI()
		{
			var curEvent = Event.current;
			_mousePos = curEvent.mousePosition;
			
			var localWindowRect = new Rect(Vector2.zero, position.size);

			//선택 된 노드가 있는가
			var isSelect = SelectNode();			
			switch(curEvent.type) 
			{
				case EventType.DragUpdated:
					if(localWindowRect.Contains(_mousePos))
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Event.current.Use();
					}
					break;

				case EventType.DragPerform:					
					if(localWindowRect.Contains(_mousePos))
					{
						if((_root != null && _root.rect.Contains(_mousePos)) || _tempNodes.Any(m => m.rect.Contains(_mousePos)))
							return;

						DragAndDrop.AcceptDrag();

						foreach(var obj in DragAndDrop.objectReferences)
						{
							if(obj is MonoScript script)
								OnFileDropped(script);
						}

						Event.current.Use();
					}
					break;

				case EventType.MouseDown:
					{
						switch((MouseButtonState)curEvent.button) 
						{
							case MouseButtonState.Left:
								if(isSelect)
									ConnectNode();
								break;

							case MouseButtonState.Right:
								var menu = new GenericMenu();
								CreateSubMenu(isSelect, menu);
								curEvent.Use();
								break;
						}
					}
					break;

				case EventType.MouseUp:
					{
						if(isSelect)
							ResetTreeNodesIds();
					}
					break;
			}

			if(DeleteNode() == true)
				return;

			//연결할 노드를 탐색중이라면
			if(_searchingForConnectedNode == true && _prevSelect != null)
			{
				var mouseRect = new Rect(_mousePos.x, _mousePos.y, 10, 10);

				DrawArrow(_prevSelect.rect, mouseRect, false);
				Repaint();
			}			

			_target = EditorGUILayout.ObjectField("Target", _target, typeof(AI), true) as AI;

			if(GUILayout.Button("Save"))
				Save();

			if(GUILayout.Button("Load"))
				Load();

			if(GUILayout.Button("Reset"))
				Reset();

			if(GUILayout.Button("Print Member"))
				PrintLog();

			if(_root == null)
				return;
						
			ConnectChild(_root);
			BeginWindows();

			_root.DrawNode();
			_tempNodes.ForEach(node => node.DrawNode());

			EndWindows();
		}		

		private void Save()
		{
			if(TargetIsNull() || _root == null || scriptableObj == null)
				return;

			var controller = scriptableObj.GetNodeController(_target.gameObject.name);
			if(controller == null) 
			{
				controller = new NodeController(_target.gameObject.name);
				scriptableObj.SetNodeController(controller);
			}

			var saveList = _root.GetAllNodes().Distinct().ToList();
			controller.SetNodeList(saveList);

			EditorUtility.SetDirty(scriptableObj);
			AssetDatabase.SaveAssets();

			Debug.Log($"Save Done : {saveList.Count} nodes");
		}

		private void Load()
		{
 			if(TargetIsNull() || scriptableObj == null)
				return;

			_tempNodes.Clear();
			_root = null;

			var controller = scriptableObj.GetNodeController(_target.gameObject.name);
			if(controller != null && controller.Root != null)
			{				
				_root = controller.Root;
				ResetTreeNodesIds();
				Debug.Log($"Load Done : {_root.GetAllNodes().Count} nodes");			
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
			if(_root == null)
				return false;

			//루트에서 한번 탐색
			_curSelect = _root.FindSelectNode(_mousePos);

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
			//연결할 노드를 탐색중인가
			if(_searchingForConnectedNode == true && _curSelect != null)
			{
				if((_prevSelect != _curSelect) && (_curSelect.parent == null))
				{
					//선택 된 노드를 자식에 추가
					_prevSelect.AddChild(_curSelect);

					if(_tempNodes.Contains(_curSelect))
						_tempNodes.Remove(_curSelect);

					ResetTreeNodesIds();
				}

				//연결 노드 탐색 중지
				_searchingForConnectedNode = false;
				_curSelect = null;
			}
		}

		private void OnFileDropped(MonoScript script)
		{
			var className = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(script));

			Type type = script.GetClass();

			if(type == null || !typeof(Node).IsAssignableFrom(type))
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
			if(isSelect == true)
			{
				menu.AddItem(new GUIContent("Connect Node"), false, ContextCallback, ActionState.Connect);
				menu.AddItem(new GUIContent("Debug_child"), false, ContextCallback, ActionState.Debug_child);
				menu.AddItem(new GUIContent("Debug_parent"), false, ContextCallback, ActionState.Debug_parent);
			}
			else
			{
				menu.AddItem(new GUIContent("Add Node"), false, ContextCallback, ActionState.AddNode);
				_curSelect = null;
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
				var root = new RootNode();
				_root.SetRect(0f, 0f);
			}

			if(node == null)
				node = new Node();

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
				temp.SetId(nextId);
				++nextId;
			}
		}
	}	
}

#endif
