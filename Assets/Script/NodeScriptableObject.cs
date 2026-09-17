using System;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    [CreateAssetMenu(fileName = "NodeScriptableObject", menuName = "ScriptableObject/BTNode")]
    public class NodeScriptableObject : ScriptableObject
    {
        [SerializeField] private List<NodeController> _nodeList = new List<NodeController>();

        public NodeController GetNodeController(string key) =>
            _nodeList?.Find(controller => controller != null && controller.key == key);

        public void SetNodeController(NodeController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (_nodeList == null) _nodeList = new List<NodeController>();
            var index = _nodeList.FindIndex(item => item != null && item.key == controller.key);
            if (index < 0) _nodeList.Add(controller);
            else _nodeList[index] = controller;
        }
    }
}