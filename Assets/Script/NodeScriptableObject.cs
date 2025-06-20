using System;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
	[CreateAssetMenu(fileName = "NodeScriptableObject", menuName = "ScriptableObject/BTNode")]
	public class NodeScriptableObject : ScriptableObject
	{
		[SerializeField]
		private List<NodeController> _nodeList = new();

		public NodeController GetNodeController(string key)
		{			
			return _nodeList.Find(m => m.key == key);			
		}

		public void SetNodeController(NodeController controller) 
		{
			if(GetNodeController(controller.key) != null)
				return;

			_nodeList.Add(controller);
		}
	}
}