using BT.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
	#region ENUM

	/// <summary>
	/// 우선순위 낮은 순
	/// </summary>
	public enum BtState 
	{		
		FAILUER,
		SUCCESS,		
		RUNNING		
	}

	public enum BTType
	{
		NONE,
		ROOT,
		CONDITION,
		SEQUENCE,
		SELECTOR,
	}

	#endregion

	#region JSON_DATA

	[Serializable]
	public class NodeController
	{
		[SerializeField]
		private List<Node> _nodeList = new List<Node>();

		[SerializeField]
		private string _key;

		private bool _isInit = false;		
				
		public List<Node> nodeList 
		{
			get 
			{
				Initialize();
				return _nodeList;
			}
		}

		public Node Root => nodeList.Find(m => m.nodeType == BTType.ROOT);
		public string key => _key;

		public NodeController(string key) 
		{
			_key = key;
		}

		private void Initialize() 
		{
			if(_isInit)
				return;

			var count = _nodeList.Count;
			for(var i = 0; i < count; i++) 
			{
				var node = _nodeList[i];
				var type = Utils.GetBTType(node.typeName);
				_nodeList[i] = Activator.CreateInstance(type) as Node;
			}

			_isInit = true;
		}

		public Node GetChild(int id)
		{
			var child = nodeList.Find(m => m.id == id);
			nodeList.Remove(child);

			return child;
		}

		public void SetNodeList(List<Node> nodeList)
		{
			_nodeList = nodeList;
		}
	}	

	#endregion
}
