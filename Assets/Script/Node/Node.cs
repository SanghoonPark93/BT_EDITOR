using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BT
{
	[Serializable]
	public partial class Node
	{
		public List<int> childIds = new List<int>();

		protected List<Node> _childs = new List<Node>();

		public int id { get; protected set; }
		public BTType nodeType { get; protected set; }		

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

	//Write editor-related code here.
	public partial class Node 
	{
#if UNITY_EDITOR

		public Rect rect { get; protected set; }

		public void CopyInfo(int id, BTType nodeType, Rect rect)
		{
			this.id = id;
			this.nodeType = nodeType;
			this.rect = rect;
		}

		public void SetId(int id) 
		{
			this.id = id;
		}

		public void SetNodeType(BTType nodeType) 
		{
			this.nodeType = nodeType;
		}

		public void SetRect(Rect rect) 
		{
			this.rect = rect;
		}

#endif
		public void DrawWindow()
		{
#if UNITY_EDITOR
			DrawDescription();
#else
			Debug.Log("This function should only be called in the Unity Editor.");
#endif
		}

		public virtual void DrawDescription()
		{
			SetRect(GUI.Window(id, rect, (id) =>
			{
				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField($"{nodeType}");
				EditorGUILayout.EndVertical();

				GUI.DragWindow();
			}, id.ToString()));
		}
	}
}