using System;
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

#if UNITY_EDITOR

	public class BehaviorTree : EditorWindow
	{
		private enum MouseButtonState
		{
			Left = 0,
			Right = 1
		}

		private List<int> _uniqueList = new List<int>();
		private List<NodeItem> _nodes = new List<NodeItem>();

		private Vector2 _mousePos;

		private NodeItem _prevSelect;
		private NodeItem _curSelect;

		private bool _isDrawing = false;
		private bool _isLoaded = false;

		private AI _target;

		[MenuItem("Window/Behavior Tree")]
		private static void Initialize()
		{
			var window = GetWindowWithRect(typeof(BehaviorTree), new Rect(100, 100, 400, 400));
			window.Show();
		}

		private void OnGUI()
		{
			if(_isLoaded && _nodes.Any() == false)
				Load();

			if(DeleteNode() == true)
				return;

			var e = Event.current;
			_mousePos = e.mousePosition;

			if(e.button == (int)MouseButtonState.Right && e.type == EventType.MouseDown)
			{
				var isSelect = SelectNode();
				var menu = new GenericMenu();

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
				e.Use();
			}
			else if(e.button == (int)MouseButtonState.Left && e.type == EventType.MouseDown)
			{
				var isSelect = SelectNode();

				if(_isDrawing == true && _curSelect != null)
				{
					if((isSelect == false) || (_prevSelect == _curSelect) || (_curSelect.parent != null))
					{
						_isDrawing = false;
						_curSelect = null;
						return;
					}

					_prevSelect.AddChild(_curSelect);
					_isDrawing = false;
				}
			}

			if(_isDrawing == true && _curSelect != null)
			{
				var mouseRect = new Rect(e.mousePosition.x, e.mousePosition.y, 10, 10);

				DrawArrow(_curSelect.rect, mouseRect, false);
				Repaint();
			}

			_nodes.ForEach(node => node.DrawArrow());

			BeginWindows();

			var idx = 0;
			foreach(var node in _nodes)
			{
				node.rect = GUI.Window(idx, node.rect, DrawNodeWindow, idx.ToString());
				++idx;
			}

			_target = EditorGUILayout.ObjectField("Target", _target, typeof(AI), true) as AI;
			if(GUILayout.Button("Save"))
				Save();

			if(GUILayout.Button("Load"))
				Load();

			EndWindows();
		}

		private void Save()
		{
			if(TargetIsNull() == true || _nodes.Any() == false)
				return;

			var save = new BTData();
			var fileAddress = Utils.GetJsonAddress(_target.gameObject.name);

			if(Utils.HasJson(fileAddress) == true)
			{
				var load = Utils.ReadAllText(fileAddress);
				save = JsonUtility.FromJson<BTData>(load);
			}

			var dataList = new List<NodeData>();
			foreach(var node in _nodes)
			{
				var nodeData = new NodeData()
				{
					childIDs = node.childs.Select(m => m.id).ToList(),
					nodeName = node.nodeName,
					checkName = node.checkName,
					eventName = node.eventName,
					bt = node.bt,
					rect = node.rect,
					id = node.id
				};
				dataList.Add(nodeData);
			}

			save.dataList = dataList;
			Utils.WriteAllText(fileAddress, JsonUtility.ToJson(save));
			Debug.Log($"Save Count : {_nodes.Count}");
		}

		private void Load()
		{
			if(TargetIsNull() == true)
				return;

			while(_nodes.Any() == true)
			{
				_nodes[0].DeleteNode();
				_nodes.Remove(_nodes[0]);
			}

			var data = GetBTData(_target.gameObject.name);

			_uniqueList.Clear();
			_uniqueList = data.dataList.Select(m => m.id).ToList();

			LoadNodes(data.dataList);

			_isLoaded = true;

			Debug.Log($"Load Count : {_nodes.Count}");
		}

		public static BTData GetBTData(string fileName)
		{
			var fileAddress = Utils.GetJsonAddress(fileName);

			if(Utils.HasJson(fileAddress) == false)
			{
				Debug.LogError($"File is NULL!!\n{fileAddress}");
				return null;
			}

			var load = Utils.ReadAllText(fileAddress);
			return JsonUtility.FromJson<BTData>(load);
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

		#region NodeManager
		private void CreateNode(Vector2 pos)
		{
			var id = 0;

			while(_uniqueList.Contains(id) == true)
				++id;
			_uniqueList.Add(id);

			var node = new NodeItem();
			node.SetData(pos, id);
			_nodes.Add(node);
		}

		private void LoadNodes(List<NodeData> nodeDatas)
		{
			foreach(var nodeData in nodeDatas)
			{
				var node = new NodeItem();
				node.SetData(nodeData);
				_nodes.Add(node);
			}

			foreach(var nodeData in nodeDatas)
			{
				if(nodeData.childIDs.Any() == false)
					continue;

				var mine = _nodes.Find(m => m.id == nodeData.id);
				var childs = new List<NodeItem>();

				foreach(var id in nodeData.childIDs)
				{
					var child = _nodes.Find(m => m.id == id);
					childs.Add(child);
					child.SetParent(mine);
				}
				mine.SetChilds(childs);
			}
		}

		private bool DeleteNode()
		{
			var e = Event.current;
			if(e.Equals(Event.KeyboardEvent("delete")))
			{
				if(_curSelect == null)
					return false;

				_uniqueList.Remove(_curSelect.id);
				_curSelect.DeleteNode();
				_nodes.Remove(_curSelect);
				_curSelect = null;
				e.Use();

				return true;
			}

			return false;
		}

		private bool SelectNode()
		{
			var isSelect = false;

			foreach(var node in _nodes)
			{
				if(node.rect.Contains(_mousePos) == false)
					continue;

				isSelect = true;
				_curSelect = node;
				break;
			}

			return isSelect;
		}

		private void DrawNodeWindow(int id)
		{
			_nodes[id].DrawWindow();
			GUI.DragWindow();
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
					_isDrawing = true;
					_prevSelect = _curSelect;
					break;
			}
		}

		public static void DrawCurve(Rect start, Rect end, Color color, float force)
		{
			var startPos = new Vector3(start.x + start.width, start.y, 0);
			var endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
			var startTan = startPos + Vector3.right * force;
			var endTan = endPos + Vector3.left * force;

			Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 2); // 2는 선의 두께
		}

		public static void DrawLine(Rect start, Rect end, bool isOriginPos = true)
		{
			var startPos = new Vector2(start.x + (start.size.x / 2f), start.y + (start.size.y / 2f));
			var endPos = end.position;

			if(isOriginPos == true)
				endPos = new Vector2(end.x + (end.size.x / 2f), end.y + (end.size.y / 2f));

			start.width = 1;
			start.height = 1;
			end.width = 1;
			end.height = 1;

			Handles.DrawLine(startPos, endPos);
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
		#endregion
	}

#endif
	[Serializable]
	public class BTData
	{
		public List<NodeData> dataList = new List<NodeData>();
	}

	[Serializable]
	public class NodeData
	{
		public List<int> childIDs = new List<int>();
		public string nodeName;
		public string checkName;
		public string eventName;
		public BTState bt;
		public Rect rect;
		public int id;
	}
}
