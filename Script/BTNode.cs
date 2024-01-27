using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BT
{
	#region ENUM

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
		public string nodeName;
		public string checkName;
		public string eventName;
		public BTState nodeType;
		public ActionType actionType;
		public Rect rect;
		public List<int> childIds = new List<int>();
	}

	#endregion

	public class BTNode : Node
	{
#if UNITY_EDITOR

		public string name;

#endif
		private delegate bool Check();
		private delegate void NodeAction(bool isCheck);

		private Check _isCheck;
		private NodeAction _action;		
		private List<BTNode> _childs = new List<BTNode>();				

		public BTNode(NodeController controller, Node data, AI ai)
		{
#if UNITY_EDITOR
			name = data.nodeName;
#endif
			id = data.id;			
			nodeType = data.nodeType;

			switch(data.actionType)
			{
				case ActionType.IDLE:
					_action = ai.Idle;
					break;

				case ActionType.ATTACK:
					_action = ai.Attack;
					_isCheck = ai.IsAttack;
					break;

				case ActionType.MOVE:
					_action = ai.Move;
					break;

				case ActionType.HIT:
					_isCheck = ai.IsHit;
					break;

				case ActionType.DEATH:
					_action = ai.Death;
					_isCheck = ai.IsDeath;
					break;

				case ActionType.CUSTOM:
					if(string.IsNullOrEmpty(data.checkName) == false)
						_isCheck = (Check)Delegate.CreateDelegate(typeof(Check), ai, GetType().GetMethod(data.checkName));

					if(string.IsNullOrEmpty(data.eventName) == false)
						_action = (NodeAction)Delegate.CreateDelegate(typeof(NodeAction), ai, GetType().GetMethod(data.checkName));
					break;
			}

			foreach(var id in data.childIds) 
			{
				var child = controller.GetChild(id);

				if(child == null)
					continue;
								
				var node = new BTNode(controller, child, ai);
				_childs.Add(node);
			}
		}

		public void SetChilds(List<BTNode> childs)
		{
			_childs = childs;
		}

		public bool ActiveSelf()
		{
			var active = false;
			switch(nodeType)
			{
				case BTState.ROOT:
				case BTState.NONE:
					active = (_isCheck == null) ? false : _isCheck.Invoke();
					break;

				case BTState.SELECTOR:
					foreach(var child in _childs)
					{
						var check = child.ActiveSelf();
						if(check == true)
						{
							active = (_isCheck == null) ? true : (bool)_isCheck.Invoke();
							child.CallAction(active);
							return active;
						}
					}
					active = false;
					break;

				case BTState.SEQUENCE:
					foreach(var child in _childs)
					{
						var check = child.ActiveSelf();
						if(check == false)
						{
							child.CallAction(check);
							return false;
						}
					}
					active = (_isCheck == null) ? true : _isCheck.Invoke();
					break;
			}

			return active;
		}

		public void CallAction(bool check)
		{
			_action?.Invoke(check);
		}
	}
}
