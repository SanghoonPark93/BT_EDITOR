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
		
	#region TestAction

	public abstract NodeState HpCheck(bool isFirstTurn);	

	public abstract NodeState Death(bool isFirstTurn);
	
	public abstract NodeState Hit(bool isFirstTurn);
	
	public abstract NodeState Detector(bool isFirstTurn);

	public abstract NodeState Attack(bool isFirstTurn);

	public abstract NodeState Move(bool isFirstTurn);

	public abstract NodeState Idle(bool isFirstTurn);

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
}
