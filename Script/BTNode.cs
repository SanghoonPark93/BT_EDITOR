using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BT
{
	#region ENUM

	public enum NodeState 
	{		
		RUNNING,		
		SUCCESS,		
		FAILUER
	}

	public enum BTState
	{
		NONE,
		ROOT,
		SEQUENCE,
		SELECTOR
	}

	public enum ActionType
	{
		NONE,
		IDLE,
		ATTACK,
		MOVE,
		HIT,
		DEATH,
		CUSTOM
	}

	#endregion

	#region JSON_DATA

	[Serializable]
	public class NodeController
	{
		public List<Node> nodeList = new List<Node>();
		
		public Node Root => nodeList.Find(m => m.nodeType == BTState.ROOT);

		public Node GetChild(int id)
		{
			var child = nodeList.Find(m => m.id == id);
			nodeList.Remove(child);

			return child;
		}
	}

	[Serializable]
	public class Node
	{
		public int id;		
		public string actionName;
		public BTState nodeType;
		public ActionType actionType;
		public List<int> childIds = new List<int>();

#if UNITY_EDITOR

		public Rect rect;

#endif
	}

	#endregion

	public class BTNode : Node
	{
		private delegate NodeState NodeAction();
		
		private NodeAction _action;		
		protected List<BTNode> _childs = new List<BTNode>();

		protected void AddChild(BTNode child) 
		{
			if(_childs.Contains(child))
				return;

			childIds.Add(child.id);
			_childs.Add(child);
		}

		public void RemoveChild(BTNode child) 
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

			switch(data.actionType)
			{
				case ActionType.IDLE:
					_action = ai.Idle;
					break;

				case ActionType.ATTACK:
					_action = ai.Attack;					
					break;

				case ActionType.MOVE:
					_action = ai.Move;
					break;

				case ActionType.HIT:
					_action = ai.Hit;
					break;

				case ActionType.DEATH:
					_action = ai.Death;					
					break;

				case ActionType.CUSTOM:					

					if(string.IsNullOrEmpty(data.actionName) == false)
						_action = (NodeAction)Delegate.CreateDelegate(typeof(NodeAction), ai, GetType().GetMethod(data.actionName));
					break;
			}

			foreach(var id in data.childIds) 
			{
				var child = controller.GetChild(id);

				if(child == null)
					continue;

				var node = new BTNode();
				node.SetData(controller, child, ai);

				AddChild(node);				
			}
		}

		public NodeState GetState()
		{
			var state = NodeState.FAILUER;
			switch(nodeType)
			{
				case BTState.ROOT:
				case BTState.SELECTOR:
					foreach(var child in _childs)
					{
						var childState = child.GetState();

						if(state != NodeState.FAILUER)
							state = childState;
					}					
					break;

				case BTState.SEQUENCE:
					foreach(var child in _childs)
					{
						var childState = child.GetState();

						if(childState == NodeState.FAILUER)
							break;
					}
					break;

				case BTState.NONE:
					if(_action != null)
						state = _action.Invoke();
					break;
			}

			return state;
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
