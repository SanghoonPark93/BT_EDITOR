using BT;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AITest : AI, IObjectType
{
	[SerializeField]
	private float _hp;

	private float _str;

	private bool _isBlockClustering;

	private Animator _anim;

	private NavMeshAgent _navi;

	private Detector _detector;
	
	private Queue<IObjectType> _hitInfoQueue = new();

	[SerializeField]
	private AITest _head;

	[SerializeField]
	private List<AITest> _child;

	public InteractionType ObjType() => InteractionType.AI;

	public float str => _str;

	public float hp => _hp;

	public float value => _hp + _str;

	public bool isAlive => _hp > 0;

	#region UNITY_EVENT

	public override void Initialize(string jsonName)
	{
		base.Initialize(jsonName);

		SetDelayTime(0.1f);

		_anim = GetComponentInChildren<Animator>();
		_navi = GetComponent<NavMeshAgent>();
		_detector = GetComponentInChildren<Detector>();
		_hp = 5f;

		_str = Random.Range(1, 5);
	}

	protected override void Update()
	{
		//우두머리가 있을 경우 우두머리의 행동패턴을 따라감
		if(_head != null) 
		{
			_navi.SetDestination(_head.transform.position);
			return;
		}

		base.Update();
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.TryGetComponent(out IObjectType obj))
		{
			switch(obj.ObjType()) 
			{
				case InteractionType.WEAPON:
					if(_hitInfoQueue.Contains(obj) == false)
						_hitInfoQueue.Enqueue(obj);
					break;
			}
		}
	}

	#endregion

	public override NodeState HpCheck()
	{			
		return (isAlive) ? NodeState.FAILUER : NodeState.SUCCESS;
	}

	public override NodeState Death()
	{
		var key = "Death";

		var alreadyCheck = AlreadyCheck(key);
		if(alreadyCheck == false) 
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

		return (alreadyCheck) ? _stateCache : NodeState.FAILUER;
	}

	public override NodeState Hit()
	{
		var isHit = _hitInfoQueue.Any();
		if(isHit)
		{
			var key = "Hit";
			Debug.Log(key);

			_stateCache = NodeState.SUCCESS;

			var weapon = (_hitInfoQueue.Dequeue() as Weapon);
			_hp -= weapon.damage;

			if(_anim.GetBool(key) == false) 
			{
				_anim.SetBool(key, true);
				StartCoroutine(DelayState(1.3f, () =>
				{					
					_anim.SetBool(key, false);
					_stateCache = NodeState.FAILUER;
				}));
			}			
		}		

		return (isHit) ? _stateCache : NodeState.FAILUER;
	}

	public override NodeState Detector()
	{
		var isOn = _detector.isOn;

		if(isOn)
			Utils.EditorLog("Detector");

		return isOn ? NodeState.RUNNING : NodeState.FAILUER;
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
		if(_isBlockClustering == false) 
		{
			var getAI = _detector.GetFirstObject(InteractionType.AI);
			
			if(getAI != null)
			{
				Debug.Log("Clustering");
				SampleManager.Instance.RequestClustering(this, getAI as AITest);
				_isBlockClustering = true;
			}
		}		

		return (_isBlockClustering) ? NodeState.SUCCESS : NodeState.FAILUER;
	}

	private IEnumerator DelayState(float delay, UnityAction onEnd) 
	{
		yield return new WaitForSeconds(delay);

		onEnd?.Invoke();		
	}

	public void ResponseClustering(AITest head = null, AITest child = null) 
	{
		_isBlockClustering = false;

		if(head == null && child == null)
			return;

		if(head == this) // 내가 헤드라면
		{
			if(_child == null)
				_child = new List<AITest>();

			_child.Add(child);			
		}
		else // 내가 자식이라면
		{
			_head = head;

			//내가 군집에서 하위 객체이면 이후로는 상위 객체의 디텍터에 의존
			_detector.gameObject.SetActive(false);
		}

		_str += 1f;
	}
}
