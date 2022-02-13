using System.Collections.Generic;
using System.Reflection;

public class BTNode 
{
#if UNITY_EDITOR
    public string          name;
#endif
    private List<BTNode>    _childs = new List<BTNode>();
    private object[]        _param = { false };
    private int             _id;
    private MethodInfo      _isActive;
    private MethodInfo      _action;
    private AI              _ai;

    public BTState          bt;
    public bool             rootChild;
    public int              id => _id;

    public BTNode(NodeData node, AI ai) 
    {
#if UNITY_EDITOR
        name        = node.nodeName;
#endif
        _id         = node.id;
        bt          = node.bt;
        _ai         = ai;

        if(string.IsNullOrEmpty(node.checkName) == false)
            _isActive   = _ai.GetType().GetMethod(node.checkName);

        if(string.IsNullOrEmpty(node.eventName) == false)
            _action     = _ai.GetType().GetMethod(node.eventName);        
    }

    public void SetChilds(List<BTNode> childs)
    {
        _childs = childs;
    }

    public bool ActiveSelf() 
    {
        var active = false;
        switch(bt)
        {
            case BTState.Root:
            case BTState.None:
                active = (_isActive != null) ? (bool)_isActive.Invoke(_ai, null) : false;
                break;

            case BTState.Selector:
                foreach(var child in _childs)
                {
                    var  check = child.ActiveSelf();
                    if(check == true)
                    {
                        active = (_isActive != null) ? (bool)_isActive.Invoke(_ai, null) : true;
                        child.Action(active);
                        return active;
                    }
                }
                active = false;
                break;

            case BTState.Sequence:
                foreach(var child in _childs)
                {
                    var check = child.ActiveSelf();
                    if(check == false)
                    {
                        child.Action(check);
                        return false;
                    }
                }
                active = (_isActive != null) ? (bool)_isActive.Invoke(_ai, null) : true;
                break;
        }

        return active;
    }

    public void Action(bool check) 
    {
        _param[0] = check;
        _action?.Invoke(_ai, _param);
    }
}