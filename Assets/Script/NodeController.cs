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
		private string _key; 

		public List<Node> nodeList = new List<Node>();
		
		public Node Root => nodeList.Find(m => m.nodeType == BTType.ROOT);
		public string key => _key;

		public NodeController(string key) 
		{
			_key = key;
		}

		public Node GetChild(int id)
		{
			var child = nodeList.Find(m => m.id == id);
			nodeList.Remove(child);

			return child;
		}
	}	

	#endregion
}
