using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BT;

public enum BTState
{
    Root,
    None,
    Sequence,
    Selector
}

public class NodeItem : ScriptableObject
{
    public List<NodeItem>   childs = new List<NodeItem>();
    public string           nodeName;
    public string           checkName;
    public string           eventName;
    public BTState          bt;
    public NodeItem         parent;
    public Rect             rect;
    public int              id;

    public void SetData(Vector2 pos, int id)
    {
        nodeName    = "New Node";
        checkName   = "Check Method";
        eventName   = "Event Method";
        rect        = new Rect(pos.x, pos.y, 100, 110);
        this.id     = id;
    }

    public void SetData(NodeData node)
    {
        nodeName    = node.nodeName;
        checkName   = node.checkName;
        eventName   = node.eventName;
        bt          = node.bt;
        rect        = node.rect;
        id          = node.id;
    }

    public void SetParent(NodeItem parent) 
    {
        this.parent = parent;
    }

    public void SetChilds(List<NodeItem> childs) 
    {
        this.childs = childs;
    }

    public void DrawWindow()
    {
        EditorGUILayout.BeginVertical();
        nodeName      = EditorGUILayout.TextField(nodeName);
        bt            = (BTState)EditorGUILayout.EnumPopup(bt);
        checkName     = EditorGUILayout.TextField(checkName);
        eventName     = EditorGUILayout.TextField(eventName);
        EditorGUILayout.EndVertical();

        Setting();
    }

    public void DrawArrow()
    {
        if(childs.Any() == true)
        {
            childs.ForEach(node => BehaviorTree.DrawArrow(rect, node.rect));
        }
    }

    public void DeleteNode()
    {
        if(parent != null)
        {
            parent.RemoveChild(this);
            parent = null;
        }

        childs.Clear();
        //Destroy(this);
        DestroyImmediate(this);
    }

    public void RemoveChild(NodeItem node)
    {
        if(childs.Contains(node) == false)
            return;

        childs.Remove(node);
    }

    public void AddChild(NodeItem node)
    {
        if(childs.Contains(node) == true)
            return;

        node.parent = this;
        childs.Add(node);
    }

    private void Setting()
    {
        if(childs.Any() == false)
        {
            bt = BTState.None;
        }
        else
        {
            if(parent == null)
            {
                bt = BTState.Root;
            }
            else
            {
                if(bt == BTState.Root)
                    bt = BTState.None;
            }
        }
    }

    public void PrintChild() 
    {
        Debug.Log($"child count : {childs.Count}");
        childs.ForEach(m => Debug.Log(m.nodeName));
    }

    public void PrintParent() 
    {
        Debug.Log(parent.nodeName);
    }
}
