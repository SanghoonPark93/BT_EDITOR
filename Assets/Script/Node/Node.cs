using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BT
{
	[Serializable]
	public class Node
	{
		public List<int> childIds = new List<int>();

		protected List<Node> _childs = new List<Node>();

		public int id { get; protected set; }
		public BTType nodeType { get; protected set; }
		public bool isFirstTurn { get; protected set; }

#if UNITY_EDITOR

		public Rect rect { get; protected set; }

		public virtual void DrawWindow() 
		{
			SetValue(rect: GUI.Window(id, rect, (id) =>
			{
				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField($"{nodeType}");
				EditorGUILayout.EndVertical();

				GUI.DragWindow();
			}, id.ToString()));
		}

#endif

		public void SetValue(int id = 0, BTType nodeType = BTType.NONE, Rect rect = new Rect()) 
		{
			if(id != 0)
			{
				this.id = id;
			}
			else if(nodeType != BTType.NONE)
			{
				this.nodeType = nodeType;
			}
			else if(rect != Rect.zero) 
			{
				this.rect = rect;
			}
		}

		public void CopyInfo(int id, BTType nodeType, Rect rect) 
		{
			this.id = id;
			this.nodeType = nodeType;
			this.rect = rect;
		}

		public virtual BtState GetState()
		{
			return BtState.SUCCESS;
		}

		public virtual void AddChild(Node child)
		{
			if(_childs.Contains(child))
				return;

			childIds.Add(child.id);
			_childs.Add(child);
		}

		public void RemoveChild(Node child)
		{
			if(_childs.Contains(child) == false)
				return;

			childIds.Remove(child.id);
			_childs.Remove(child);
		}

		public void SetData(NodeController controller, Node data, AI ai)
		{
			id = data.id;
			nodeType = data.nodeType;
			isFirstTurn = true;

			foreach(var id in data.childIds)
			{
				var child = controller.GetChild(id);

				if(child == null)
					continue;

				var node = new Node();
				node.SetData(controller, child, ai);

				AddChild(node);
			}
		}

		public List<Node> GetAllNodes()
		{
			var list = new List<Node>();
			list.Add(this);

			_childs.ForEach(child => list.AddRange(child.GetAllNodes()));

			return list;
		}
	}
}