using BT;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AITest : AI
{
	private Animator _anim;

	[SerializeField]
	private float _hp;

	#region UNITY_EVENT

	protected override void Start()
	{
		base.Start();

		Initialize();		
	}

	protected override void Update()
	{
		if(Input.GetKeyDown(KeyCode.O))
			Debug.Log($"count : {_btRoot.GetAllNodes().Count}");

		base.Update();
	}

	#endregion

	private void Initialize() 
	{
		_anim = GetComponent<Animator>();
		_hp = 5f;
	}

	public override NodeState HpCheck()
	{			
		return (_hp > 0f) ? NodeState.FAILUER : NodeState.SUCCESS;
	}

	public override NodeState Death()
	{
		var key = "Death";

		if(AlreadyCheck(key) == false) 
		{
			Debug.Log(key);

			_isStop = true;

			_stateCache = NodeState.RUNNING;

			_anim.SetTrigger(key);
			
			//사망 애니메이션 시간이 약 6.4f
			StartCoroutine(DelayState(6.4f, ()=>
			{
				EndAction(key);
				_stateCache = NodeState.SUCCESS;
			}));			
		}		

		return _stateCache;
	}

	public override NodeState Hit()
	{
		//Debug.Log("Hit");

		return NodeState.SUCCESS;
	}

	public override NodeState Detector()
	{
		//Debug.Log("Detector");

		return NodeState.SUCCESS;
	}

	public override NodeState Attack()
	{
		//Debug.Log("Attack");

		return NodeState.SUCCESS;
	}

	public override NodeState Move()
	{
		//Debug.Log("Move");

		return NodeState.SUCCESS;
	}

	public override NodeState Idle()
	{
		//Debug.Log("Idle");

		return NodeState.SUCCESS;
	}

	public NodeState Clustering() 
	{
		//Debug.Log("Clustering");

		return NodeState.SUCCESS;
	}

	private IEnumerator DelayState(float delay, UnityAction onEnd) 
	{
		yield return new WaitForSeconds(delay);

		onEnd?.Invoke();		
	}
}
