using BT.Util;
using System;
using System.Collections.Generic;
using System.Linq;
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

	[Serializable]
	public class NodeData
	{
		public int id;
		public string typeName;
		public Rect rect;
		public List<int> childIds = new();
	}

	[Serializable]
	public class NodeController
	{
		[SerializeField]
		private string _key;

		[SerializeField]
		private List<NodeData> _flatNodeDataList = new();

		private Dictionary<int, Node> _nodeMap = new(); // id → Node
		private Node _root;

		public Node Root => _root;
		public string key => _key;

		public NodeController(string key)
		{
			_key = key;
		}

		public void SetNodeList(List<Node> nodeList)
		{
			_flatNodeDataList.Clear();

			foreach(var node in nodeList)
			{
				var data = new NodeData
				{
					id = node.id,
					typeName = node.typeName,
					rect = node.rect,
					childIds = node.childs.Select(c => c.id).ToList()
				};

				_flatNodeDataList.Add(data);
			}
		}

		public void Initialize()
		{
			if(_nodeMap == null)
				_nodeMap = new Dictionary<int, Node>();

			_nodeMap.Clear();

			foreach(var data in _flatNodeDataList)
			{
				var type = Utils.GetBTType(data.typeName);
				var node = Activator.CreateInstance(type) as Node;
				node.SetId(data.id);
				node.SetRect(data.rect.x, data.rect.y);
				_nodeMap[data.id] = node;
			}

			foreach(var data in _flatNodeDataList)
			{
				var parent = _nodeMap[data.id];
				foreach(var childId in data.childIds)
				{
					if(_nodeMap.TryGetValue(childId, out var child))
					{
						parent.AddChild(child);
						child.SetParent(parent);
					}
				}
			}

			_root = _nodeMap.Values.FirstOrDefault(n => n.nodeType == BTType.ROOT);
		}

		public List<Node> GetAllNodes() => _nodeMap.Values.ToList();
		//[SerializeField]
		//private List<Node> _nodeList = new List<Node>();

		//[SerializeField]
		//private string _key;

		//private bool _isInit = false;		

		//public List<Node> nodeList 
		//{
		//	get 
		//	{
		//		Initialize();
		//		return _nodeList;
		//	}
		//}

		//public Node Root => nodeList.Find(m => m.nodeType == BTType.ROOT);
		//public string key => _key;

		//public NodeController(string key) 
		//{
		//	_key = key;
		//}

		//private void Initialize() 
		//{
		//	if(_isInit)
		//		return;

		//	var count = _nodeList.Count;
		//	for(var i = 0; i < count; i++) 
		//	{
		//		var oldNode = _nodeList[i];
		//		var type = Utils.GetBTType(oldNode.typeName);
		//		var newNode = Activator.CreateInstance(type) as Node;

		//		// 저장된 데이터 복사
		//		newNode.SetId(oldNode.id);
		//		newNode.SetRect(oldNode.rect.x, oldNode.rect.y);
		//		oldNode.childs.ForEach(m => newNode.AddChild(m));// 이 시점엔 아직 parent 설정은 안 됨

		//		_nodeList[i] = newNode;
		//		//var node = _nodeList[i];
		//		//var type = Utils.GetBTType(node.typeName);
		//		//_nodeList[i] = Activator.CreateInstance(type) as Node;
		//	}

		//	_isInit = true;
		//}

		//public Node GetChild(int id)
		//{
		//	var child = nodeList.Find(m => m.id == id);
		//	nodeList.Remove(child);

		//	return child;
		//}

		//public void SetNodeList(List<Node> nodeList)
		//{
		//	_nodeList = nodeList;
		//}
	}
}
