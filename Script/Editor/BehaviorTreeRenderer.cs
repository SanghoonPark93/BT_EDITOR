#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
		private enum MouseButtonState
		{
			Left = 0,
			Right = 1
		}

		/// <summary>
		/// 노드 새로 생성 후 트리에 속하기 직전까지 임시 보관 용 리스트
		/// </summary>
		private List<EditorNode> _tempNodes = new List<EditorNode>();
		
		private Vector2 _mousePos;

		private EditorNode _prevSelect;
		private EditorNode _curSelect;
		private EditorNode _root;

		private bool _searchingForConnectedNode = false;
		
		private AI _target;

		[MenuItem("Window/Behavior Tree")]
		private static void Initialize()
		{
			var window = GetWindowWithRect(typeof(BehaviorTreeRenderer), new Rect(100, 100, 400, 400));
			window.Show();
		}

		private void OnGUI()
		{
			var curEvent = Event.current;
			_mousePos = curEvent.mousePosition;

			//선택 된 노드가 있는가
			var isSelect = SelectNode();

			switch((MouseButtonState)curEvent.button) 
			{
				case MouseButtonState.Right:
					switch(curEvent.type)
					{
						case EventType.MouseDown:
							var menu = new GenericMenu();
							CreateSubMenu(isSelect, menu);
							curEvent.Use();
							break;
					}
					break;

				case MouseButtonState.Left:
					switch(curEvent.type)
					{
						case EventType.MouseDown:
							if(isSelect)
								ConnectNode();
							break;

						case EventType.MouseUp:
							if(isSelect)
								ResetTreeNodesIds();
							break;
					}
					break;
			}

			if(DeleteNode() == true)
				return;

			//연결할 노드를 탐색중이라면
			if(_searchingForConnectedNode == true && _prevSelect != null)
			{
				var mouseRect = new Rect(curEvent.mousePosition.x, curEvent.mousePosition.y, 10, 10);

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

			_root.DrawArrow();

			BeginWindows();

			_root.DrawWindow();
			_tempNodes.ForEach(node => node.DrawWindow());

			EndWindows();
		}		

		private void Save()
		{
			if(TargetIsNull() == true || _root == null)
				return;

			var controller = new NodeController();
			var fileAddress = Utils.GetJsonAddress(_target.gameObject.name);

			ResetTreeNodesIds();			

			if(Utils.HasJson(fileAddress) == true)
			{
				var load = Utils.ReadAllText(fileAddress);
				controller = JsonUtility.FromJson<NodeController>(load);
			}

			var dataList = _root.GetAllNodes();			
			controller.nodeList = dataList.Distinct().ToList();
			Utils.WriteAllText(fileAddress, JsonUtility.ToJson(controller));
			Debug.Log($"Save Count : {dataList.Count}");
		}

		private void Load()
		{
			if(TargetIsNull() == true)
				return;

			_tempNodes.Clear();

			_root = null;
			var controller = Utils.GetJson<NodeController>(_target.gameObject.name);
						
			if(controller.Root != null)
				_root = new EditorNode(controller, controller.Root);

			if(_root != null) 
			{
				ResetTreeNodesIds();				
				Debug.Log($"Load Count : {_root.GetAllNodes().Count}");
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
					_root = null;

				_curSelect.DeleteNode();
				_curSelect = null;

				ResetTreeNodesIds();				
				e.Use();

				return true;
			}

			return false;
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
			Handles.Label(fontPos, "<color=#ffffff>▼</color>", font);
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

		private void CreateNode(Vector2 pos)
		{
			var newItem = new EditorNode(pos);

			if(_root == null)
			{
				_root = newItem;
			}
			else 
			{
				_tempNodes.Add(newItem);
				ResetTreeNodesIds();	
			}
		}

		#endregion

		private void ResetTreeNodesIds() 
		{
			var nextId = (_root == null) ? 0 : _root.SetId() + 1;

			foreach(var temp in _tempNodes)
			{
				temp.SetId(nextId);
				++nextId;
			}
		}
	}	
}

#endif
