using BT;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AITest : AI
{
	private Animator _anim;
	
	private bool _isHit;

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

		//임시 테스트용
		if(Input.GetKeyDown(KeyCode.P))
			_isHit = true;

		base.Update();
	}

	protected virtual void OnTriggerEnter(Collider other)
	{

	}

	#endregion

	private void Initialize() 
	{
		_anim = GetComponent<Animator>();
		
		_hp = 5f;
		_isHit = false;
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
			_isStop = true;

			Debug.Log(key);
			_stateCache = NodeState.RUNNING;
			StartAction(key);
						
			GetComponent<BoxCollider>().enabled = false;
			GetComponent<Rigidbody>().useGravity = false;
			
			_anim.SetTrigger(key);
						
			StartCoroutine(DelayState(6.4f, () => 
			{
				EndAction(key); 
				_stateCache = NodeState.SUCCESS;
			}));
		}		

		return _stateCache;
	}

	public override NodeState Hit()
	{
		if(_isHit)
		{
			var key = "Hit";

			if(AlreadyCheck(key) == false)
			{
				StartAction(key);
				Debug.Log(key);
				_stateCache = NodeState.SUCCESS;

				--_hp;
				_anim.SetBool(key, true);

				StartCoroutine(DelayState(1.3f, () =>
				{
					EndAction(key);
					_isHit = false;
					_anim.SetBool(key, false);
					_stateCache = NodeState.FAILUER;
				}));
			}			
		}				

		return _stateCache;
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
