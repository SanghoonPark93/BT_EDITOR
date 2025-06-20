using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BT
{
	[Serializable]
	public partial class Node
	{
		protected string _typeName = "NODE";

		protected List<int> _childIds = new List<int>();

		[NonSerialized]
		protected List<Node> _childs = new List<Node>();

		[SerializeField]
		protected BTType _nodeType = BTType.NONE;

		public Node parent { get; private set; }

		public int id { get; protected set; }

		public BTType nodeType => _nodeType;

		public List<Node> childs => _childs;

		public string typeName => _typeName;

		public virtual BtState GetState()
		{
			return BtState.SUCCESS;
		}			

		public void SetData(NodeController controller, Node data)
		{
			id = data.id;
			_nodeType = data.nodeType;			

			foreach(var id in data._childIds)
			{
				var child = controller.GetChild(id);

				if(child == null)
					continue;

				var node = new Node();
				node.SetData(controller, child);

				AddChild(node);
			}
		}

		/// <summary>
		/// 파라미터 id를 시작으로 자신과 자식 노드들의 id를 세팅해준다
		/// </summary>
		/// <param name="id">자신에게 부여 받은 id</param>
		/// <returns>마지막 자식의 id</returns>
		public int InitializeId(int id = 0)
		{
			SetId(id);
			var lastId = id;

			//동일 뎁스에서 좌측에 있을 수록 우선순위가 높은 노드
			_childs = _childs.OrderBy(m => m.rect.x).ToList();
			_childIds.Clear();

			foreach(var child in _childs)
			{
				var curId = lastId + 1;
				_childIds.Add(curId);
				lastId = child.InitializeId(curId);
			}

			return lastId;
		}
	}

	//Write editor-related code here.
	public partial class Node 
	{
#if UNITY_EDITOR

		protected virtual float width => 150f;
		protected virtual float height => 100f;

		public Rect rect { get; protected set; }

		public void SetId(int id) 
		{
			this.id = id;
		}

		public void SetRect(float x, float y) 
		{			
			this.rect = new Rect(x, y, width, height);
		}

		public virtual void AddChild(Node child)
		{
			if(_childs.Contains(child))
				return;

			_childIds.Add(child.id);
			_childs.Add(child);
		}

		public void RemoveChild(Node child)
		{
			if(_childs.Contains(child) == false)
				return;

			_childIds.Remove(child.id);
			_childs.Remove(child);
		}

		public List<Node> GetAllNodes()
		{
			var list = new List<Node>();
			list.Add(this);

			_childs.ForEach(child => list.AddRange(child.GetAllNodes()));

			return list;
		}

		public Node FindSelectNode(Vector2 mousePos)
		{
			if(rect.Contains(mousePos))
				return this;

			foreach(var child in _childs)
			{
				var findSelect = child.FindSelectNode(mousePos);
				if(findSelect != null)
					return findSelect;
			}

			return null;
		}

		public void DeleteNode()
		{
			if(parent != null)
			{
				parent.RemoveChild(this);
				parent = null;
			}

			_childs.Clear();
		}

		public void PrintChild()
		{
			Debug.Log($"child count : {_childs.Count}");
			_childs.ForEach(m => Debug.Log($"child id : {m.id}"));
		}

		public void PrintParent()
		{
			if(parent != null)
				Debug.Log(parent.id);
		}

#endif

		#region GUI

		public void DrawNode()
		{
			DrawWindow();
			_childs.ForEach(m => m.DrawNode());			
		}

		public void DrawWindow()
		{
#if UNITY_EDITOR
			DrawDescription();
#else
			Debug.Log("This function should only be called in the Unity Editor.");
#endif
		}

		/// <summary>
		/// This function should only be called in the Unity Editor.
		/// </summary>
		public virtual void DrawDescription()
		{
			var rect = GUI.Window(id, this.rect, (id) =>
			{
				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField($"{nodeType}");
				EditorGUILayout.EndVertical();

				GUI.DragWindow();
			}, id.ToString());

			SetRect(rect.x, rect.y);
		}
		#endregion
	}
}