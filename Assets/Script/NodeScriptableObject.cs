using System.Collections.Generic;
using UnityEngine;

namespace BT
{
	[CreateAssetMenu(fileName = "NodeScriptableObject", menuName = "ScriptableObject/BTNode")]
	public class NodeScriptableObject : ScriptableObject
	{
		private Dictionary<string, NodeController> _nodeDict = new();

		public NodeController GetNodeController(string key)
		{
			if(_nodeDict.ContainsKey(key) == false)
				_nodeDict.Add(key, new NodeController());

			return _nodeDict[key];
		}
	}
}