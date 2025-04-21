#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BT
{
	public class EditorNode
	{
 		private readonly float DEFAULT_WIDTH = 150f;
		private readonly float DEFAULT_HEIGHT = 100f;

		public EditorNode parent { get; private set; }
				
		private Node _nodeData = new();

		protected List<EditorNode> _childs = new List<EditorNode>();

		public Node data => _nodeData;

		/// <summary>
		/// 트리 구조에 포함 된 상태
		/// </summary>	
		public void SetData(NodeController controller, Node node, EditorNode parent = null)
		{
			_nodeData.CopyInfo(node.id, node.nodeType, node.rect);			
			
			this.parent = parent;

			foreach (var id in node.childIds)
			{
				var childData = controller.GetChild(id);

				if (childData == null)
					continue;

				var child = new EditorNode();
				child.SetData(controller, childData, this);

				_childs.Add(child);
			}
		}

		/// <summary>
		/// 아직 트리구조에 포함 되지 않은 상태
		/// </summary>	
		public void SetData(Vector2 pos, Node node)
		{
			var rect = new Rect(pos.x, pos.y, DEFAULT_WIDTH, DEFAULT_HEIGHT);
			if(node == null)
			{
				_nodeData.CopyInfo(0, BTType.NONE, rect);
			}
			else 
			{
				node.SetRect(rect);
				_nodeData = node;
			}			
		}

		public void DrawWindow()
		{
			if(_nodeData == null)
				return;

			_nodeData.DrawWindow();

			foreach (var child in _childs)
			{
				if (child is EditorNode editorChild)
					editorChild.DrawWindow();
			}
		}

		public void GetWindowData(int id)
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField($"{nameof(_nodeData)}_{_nodeData.nodeType}");
			EditorGUILayout.EndVertical();

			GUI.DragWindow();
		}

		public void DrawArrow()
		{
			foreach (var child in _childs)
			{
				BehaviorTreeRenderer.DrawArrow(_nodeData.rect, child.data.rect);
				if (child is EditorNode editorChild)
					editorChild.DrawArrow();
			}
		}

		public void DeleteNode()
		{
			if (parent != null)
			{
				parent.RemoveChild(this);
				parent = null;
			}

			_childs.Clear();
		}

		private bool RemoveChild(EditorNode node)
		{
			if (_childs.Contains(node) == false)
			{
				foreach (var child in _childs)
				{
					if (child is EditorNode editorChild)
					{
						if (editorChild.RemoveChild(node))
							return true;
					}
				}

				return false;
			}

			_childs.Remove(node);
			return true;
		}

		public void AddChild(EditorNode node)
		{
			if (_childs.Contains(node) == true)
				return;

			node.parent = this;
			_nodeData.AddChild(node.data);
			_childs.Add(node);			
		}

		public EditorNode FindSelectNode(Vector2 mousePos)
		{
			if (_nodeData.rect.Contains(mousePos))
				return this;

			foreach (var child in _childs)
			{
				if (child is EditorNode editorChild)
				{
					var findSelect = editorChild.FindSelectNode(mousePos);
					if (findSelect != null)
						return findSelect;
				}
			}

			return null;
		}

		/// <summary>
		/// 파라미터 id를 시작으로 자신과 자식 노드들의 id를 세팅해준다
		/// </summary>
		/// <param name="id">자신에게 부여 받은 id</param>
		/// <returns>마지막 자식의 id</returns>
		public int SetId(int id = 0)
		{
			_nodeData.SetId(id);
			var lastId = id;

			//동일 뎁스에서 좌측에 있을 수록 우선순위가 높은 노드
			_childs = _childs.OrderBy(m => m.data.rect.x).ToList();
			_nodeData.childIds.Clear();
			foreach (var child in _childs)
			{
				if (child is EditorNode editorChild)
				{
					var curId = lastId + 1;
					_nodeData.childIds.Add(curId);
					lastId = editorChild.SetId(curId);
				}
			}

			return lastId;
		}

		public void PrintChild()
		{
			Debug.Log($"child count : {_childs.Count}");
			_childs.ForEach(m => Debug.Log($"child id : {m.data.id}"));
		}

		public void PrintParent()
		{
			if (parent != null)
				Debug.Log(parent.data.id);
		}
	}
}
#endif