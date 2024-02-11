using BT;
using BT.Util;
using System.Collections.Generic;
using UnityEngine;

public abstract class AI : MonoBehaviour
{	
	protected BTNode _btRoot;
	protected NodeState _stateCache;

	/// <summary>
	/// 업데이트문을 멈추고싶을경우 true
	/// </summary>
	protected bool _isStop;

	/// <summary>
	/// 업데이트문에 딜레이를 주고싶을 경우 사용
	/// </summary>
	private float _delayUpdate = -1f;
	private float _delayDef;

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

	public virtual void Initialize(string jsonName)
	{
		var controller = Utils.GetJson<NodeController>(jsonName);

		if(controller.Root != null)
		{
			_btRoot = new BTNode();
			_btRoot.SetData(controller, controller.Root, this);
		}
	}

	protected virtual void Update()
	{
		if(_isStop || _btRoot == null)
			return;

		if(_delayUpdate > -1) 
		{
			_delayUpdate -= Time.deltaTime;
			
			if(_delayUpdate > 0)
				return;

			_delayUpdate = _delayDef;
		}

		UpdateBody();
	}

	protected virtual void UpdateBody() 
	{
		_btRoot.GetState();
	}

	protected void SetDelayTime(float delay)
	{
		_delayUpdate = delay;
		_delayDef = delay;
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
