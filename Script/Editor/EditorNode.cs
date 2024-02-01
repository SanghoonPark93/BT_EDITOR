#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BT;

public class EditorNode : BTNode
{	
	private readonly float DEFAULT_WIDTH = 100f;
	private readonly float DEFAULT_HEIGHT = 50f;	
	private readonly float LEAF_HEIGHT = 70f;
	private readonly float CUSTOM_HEIGHT = 90f;
	
	public EditorNode parent { get; private set; }

	/// <summary>
	/// 트리 구조에 포함 된 상태
	/// </summary>	
	public void SetData(NodeController controller, Node node, EditorNode parent = null) 
	{
		id = node.id;			
		actionName = node.actionName;
		nodeType = node.nodeType;
		actionType = node.actionType;
		rect = node.rect;
		rect.width = DEFAULT_WIDTH;
		this.parent = parent;

		foreach(var id in node.childIds) 
		{
			var childData = controller.GetChild(id);

			if(childData == null)
				continue;

			var child = new EditorNode();
			child.SetData(controller, childData, this);

			_childs.Add(child);			
		}
	}

	/// <summary>
	/// 아직 트리구조에 포함 되지 않은 상태
	/// </summary>	
	public void SetData(Vector2 pos) 
	{		
		actionName = "Event Method";
		rect = new Rect(pos.x, pos.y, DEFAULT_WIDTH, DEFAULT_HEIGHT);
		id = 0;
	}

	public void DrawWindow()
	{
		rect = GUI.Window(id, rect, GetWindowData, id.ToString());
		foreach(var child in _childs)
		{
			if(child is EditorNode editorChild)
				editorChild.DrawWindow();
		}		
	}

	public void GetWindowData(int id)
	{
		EditorGUILayout.BeginVertical();
				
		nodeType = (BTState)EditorGUILayout.EnumPopup(nodeType);

		switch(nodeType) 
		{
			case BTState.ROOT:
			case BTState.SEQUENCE:
			case BTState.SELECTOR:
				rect.height = DEFAULT_HEIGHT;
				break;

			default:
				actionType = (ActionType)EditorGUILayout.EnumPopup(actionType);

				switch(actionType) 
				{
					case ActionType.CUSTOM:						
						actionName = EditorGUILayout.TextField(actionName);
						rect.height = CUSTOM_HEIGHT;						
						break;

					default:
						rect.height = LEAF_HEIGHT;
						break;
				}				
				break;
		}		
		
		SetDefautType();

		EditorGUILayout.EndVertical();

		GUI.DragWindow();		
	}

	public void DrawArrow()
	{
		foreach(var child in _childs)
		{
			BehaviorTreeRenderer.DrawArrow(rect, child.rect);
			if(child is EditorNode editorChild)
				editorChild.DrawArrow();
		}	
	}

	public void DeleteNode()
	{
		if(parent != null)
		{
			parent.RemoveChild(this);
			parent = null;
		}

		_childs.Clear();
	}
	
	private bool RemoveChild(EditorNode node)
	{
		if(_childs.Contains(node) == false) 
		{
			foreach(var child in _childs)
			{
				if(child is EditorNode editorChild) 
				{
					if(editorChild.RemoveChild(node))
						return true;
				}
			}

			return false;
		}

		_childs.Remove(node);
		return true;
	}

	public void AddChild(EditorNode node)
	{
		if(_childs.Contains(node) == true)
			return;

		node.parent = this;
		base.AddChild(node);		
	}

	public EditorNode FindSelectNode(Vector2 mousePos)
	{
		if(rect.Contains(mousePos))
			return this;

		foreach(var child in _childs)
		{
			if(child is EditorNode editorChild)
			{
				var findSelect = editorChild.FindSelectNode(mousePos);
				if(findSelect != null)
					return findSelect;
			}
		}

		return null;
	}

	private void SetDefautType()
	{
		if(parent == null)
		{
			nodeType = BTState.ROOT;
		}
		else
		{
			if(nodeType == BTState.ROOT)
				nodeType = BTState.NONE;
		}
	}

	/// <summary>
	/// 파라미터 id를 시작으로 자신과 자식 노드들의 id를 세팅해준다
	/// </summary>
	/// <param name="id">자신에게 부여 받은 id</param>
	/// <returns>마지막 자식의 id</returns>
	public int SetId(int id = 0)
	{
		this.id = id;

		var lastId = this.id;

		//동일 뎁스에서 좌측에 있을 수록 우선순위가 높은 노드
		_childs = _childs.OrderBy(m => m.rect.x).ToList();

		foreach(var child in _childs)
		{
			if(child is EditorNode editorChild)
				lastId = editorChild.SetId(lastId + 1);
		}

		return lastId;
	}

	public void PrintChild()
	{
		Debug.Log($"child count : {_childs.Count}");
		_childs.ForEach(m => Debug.Log($"child id : {m.id}"));
	}

	public void PrintParent()
	{
		if(parent != null)
			Debug.Log(parent.id);
	}
}

#endif