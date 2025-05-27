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
	}

	//Write editor-related code here.
	public partial class Node 
	{
#if UNITY_EDITOR

		protected virtual float width => 150f;
		protected virtual float height => 100f;

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

		public void SetRect(float x, float y) 
		{			
			this.rect = new Rect(x, y, width, height);
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

		public List<Node> GetAllNodes()
		{
			var list = new List<Node>();
			list.Add(this);

			_childs.ForEach(child => list.AddRange(child.GetAllNodes()));

			return list;
		}

#endif

		#region GUI
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