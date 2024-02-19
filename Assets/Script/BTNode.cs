using System;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
	#region ENUM

	/// <summary>
	/// 우선순위 낮은 순
	/// </summary>
	public enum NodeState 
	{		
		FAILUER,
		SUCCESS,		
		RUNNING		
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
		HP_CHECK,
		DEATH,
		HIT,
		DETECTOR,
		ATTACK,
		MOVE,
		IDLE,
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
		private delegate NodeState NodeAction(bool isFirstTurn);
		
		private NodeAction _action;		
		protected List<BTNode> _childs = new List<BTNode>();

		public bool isFirstTurn { get; private set; }

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
			isFirstTurn = true;

			switch(data.actionType)
			{
				case ActionType.HP_CHECK:
					_action = ai.HpCheck;
					break;

				case ActionType.DEATH:
					_action = ai.Death;
					break;

				case ActionType.HIT:
					_action = ai.Hit;
					break;

				case ActionType.DETECTOR:
					_action = ai.Detector;
					break;

				case ActionType.ATTACK:
					_action = ai.Attack;					
					break;

				case ActionType.MOVE:
					_action = ai.Move;
					break;				

				case ActionType.IDLE:
					_action = ai.Idle;
					break;

				case ActionType.CUSTOM:
					if(string.IsNullOrEmpty(data.actionName) == false)
						_action = (NodeAction)Delegate.CreateDelegate(typeof(NodeAction), ai, ai.GetType().GetMethod(data.actionName));
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
			var isEnd = false;
			switch(nodeType)
			{
				case BTState.ROOT:
				case BTState.SELECTOR:
					foreach(var child in _childs)
					{
						if(isEnd) 
						{
							isFirstTurn = true;
							continue;
						}

						var childState = child.GetState();

						if(childState != NodeState.FAILUER) 
						{
							state = childState;
							isEnd = true;							
						}
					}					
					break;

				case BTState.SEQUENCE:					
					foreach(var child in _childs)
					{
						if(isEnd) 
						{
							isFirstTurn = true;
							continue;
						}

						var childState = child.GetState();

						if((int)state < (int)childState)
							state = childState;

						if(childState == NodeState.FAILUER)
							isEnd = true;
					}
					break;

				case BTState.NONE:
					if(_action != null)
						state = _action.Invoke(isFirstTurn);

					if(state == NodeState.FAILUER)
					{
						isFirstTurn = true; // 초기화
					}
					else 
					{
						foreach(var child in _childs)
						{
							child.GetState();
						}
					}
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
