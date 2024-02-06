using BT;
using System.Collections.Generic;
using UnityEngine;

public abstract class AI : MonoBehaviour
{	
	protected BTNode _btRoot;
	protected NodeState _stateCache;

	protected bool _isStop;

	private List<string> _playList = new();

	#region TestAction

	public abstract NodeState HpCheck();	

	public abstract NodeState Death();
	
	public abstract NodeState Hit();
	
	public abstract NodeState Detector();

	public abstract NodeState Attack();

	public abstract NodeState Move();

	public abstract NodeState Idle();

	#endregion

	protected virtual void Start()
	{
		var controller = Utils.GetJson<NodeController>(gameObject.name);

		if(controller.Root != null)
		{
			_btRoot = new BTNode();
			_btRoot.SetData(controller, controller.Root, this);
		}
	}

	protected virtual void Update()
	{
		if(_isStop)
			return;

		_btRoot.GetState();
	}

	protected bool AlreadyCheck(string key) 
	{
		return _playList.Contains(key);
	}

	protected void StartAction(string key) 
	{
		if(_playList.Contains(key))
			return;

		_playList.Add(key);		
	}

	protected void EndAction(string key) 
	{
		if(_playList.Contains(key) == false)
			return;

		_playList.Remove(key);		
	}
}
