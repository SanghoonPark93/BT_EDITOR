using BT;
using BT.Util;
using Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AITest : AI, IObjectType
{
	public enum FSMType
	{
		DEATH,
		HIT,
		ATTACK,
		MOVE,
		IDLE
	}

	[SerializeField]
	private float _hp;

	private float _str;

	private bool _isBlock;

	private Animator _anim;

	private NavMeshAgent _navi;

	private Detector _detector;

	private MonoBehaviour _target;

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

	public MonoBehaviour target => _target;

	public FSMType parentState { get; private set; }

	public override void Initialize(string jsonName)
	{
		base.Initialize(jsonName);

		SetDelayTime(0.1f);

		_anim = GetComponentInChildren<Animator>();
		_navi = GetComponent<NavMeshAgent>();
		_detector = GetComponentInChildren<Detector>();
		_hp = 500f;

		_str = Random.Range(1, 5);
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

	protected override void UpdateBody()
	{
		//우두머리가 있을 경우 우두머리의 행동패턴을 따라감
		if(_head != null)
		{
			if(isAlive == false) 
			{
				Death();
				return;
			}

			if(Hit() != NodeState.FAILUER)
				return;

			var parentState = _head.parentState;
			var headTarget = _head.target;

			if(headTarget != null) 
			{
				_target = headTarget;

				var distance = (_target.transform.position - transform.position).sqrMagnitude;

				if(_detector.IsEndPos(distance) == false)
					parentState = FSMType.MOVE;
				else
					StopMove(false);
			}			

			switch(parentState) 
			{
				case FSMType.ATTACK:
					if(_target != null)
						Attack();
					break;

				case FSMType.MOVE:
					if(_target != null)
						Move();
					break;

				case FSMType.IDLE:
					_anim.SetBool("Move", false);
					_anim.SetBool("Hit", false);
					_anim.SetBool("Attack", false);
					Idle();
					break;
			}			

			return;
		}

		if(_isBlock)
			return;

		//헤드이거나 군집을 이루고 있지 않다면 비헤이비어 트리 탐색
		base.UpdateBody();
	}

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

			Utils.EditorLog(key);
			
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

	#region HP

	public override NodeState Hit()
	{
		var isHit = _hitInfoQueue.Any();
		if(isHit)
		{
			var key = "Hit";
			Utils.EditorLog(key);
						
			_stateCache = NodeState.SUCCESS;
			
			var weapon = (_hitInfoQueue.Dequeue() as Weapon);
			CalculateHp(-weapon.damage);					
		}		

		return (isHit) ? _stateCache : NodeState.FAILUER;
	}

	private void CalculateHp(float value) 
	{
		var key = "Hit";

		_hp += value;

		if(value < 0) 
		{
			if(_anim.GetBool(key) == false)
			{				
				_anim.SetBool(key, true);
				_navi.isStopped = true;
				StartCoroutine(DelayState(1.3f, () =>
				{
					_navi.isStopped = false;
					_anim.SetBool(key, false);
					_stateCache = NodeState.FAILUER;
				}));
			}
		}		
	}

	#endregion

	public override NodeState Detector()
	{
		var isOn = _detector.isOn;

		if(isOn)
			Utils.EditorLog("Detector");

		return isOn ? NodeState.RUNNING : NodeState.FAILUER;
	}

	public override NodeState Attack()
	{
		if(_isBlock)
			return NodeState.FAILUER;

		var key = "Attack";
		var isOn = _detector.playerList.Any();
		if(isOn)
		{
			if(_target == null) 
			{
				var findPlayer = _detector.playerList.FirstOrDefault();

				if(findPlayer != null)
					_target = (findPlayer as MonoBehaviour);
			}			

			_stateCache = NodeState.FAILUER;

			if(_target != null)
			{
				var distance = (_target.transform.position - transform.position).sqrMagnitude;

				if(_detector.IsEndPos(distance))
				{
					StopMove(false);
					parentState = FSMType.ATTACK;
					_stateCache = NodeState.SUCCESS;
					
					var dir = _target.transform.position - transform.position;
					transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 100f).ChangeXZ(0f, 0f);

					if(_anim.GetBool(key) == false)
					{
						_anim.SetBool(key, true);
						StartCoroutine(DelayState(1.3f, () => _anim.SetBool(key, false)));
					}
				}			
			}
		}

		return (isOn) ? _stateCache : NodeState.FAILUER;		
	}

	#region MOVE

	public override NodeState Move()
	{
		var hasTarget = (_target != null);

		if(hasTarget && _isBlock == false) 
		{
			Utils.EditorLog("Move");
			parentState = FSMType.MOVE;
						
			_isBlock = true;
			StartCoroutine(StartMove(50f));
		}
		
		return (hasTarget) ? NodeState.SUCCESS : NodeState.FAILUER;
	}

	private void StopMove(bool resetTarget = true)
	{
		StopCoroutine(nameof(StartMove));

		if(resetTarget)
			_target = null;

		_isBlock = false;
		_navi.ResetPath();
		_anim.SetBool("Move", false);
	}

	private IEnumerator StartMove(float gap, UnityAction<bool> onEnd = null)
	{
		var distance = (_target.transform.position - transform.position).sqrMagnitude;
		var key = "Move";

		while(_target != null)
		{
			//디텍터 범위 안에 없거나 도착지점에 도달했다면 반복문 탈출
			if(_head == null && (_detector.Contains(_target.gameObject) == false || _detector.IsEndPos(distance)))
			{				
				StopMove();
				break;
			}

			if(_anim.GetBool(key) == false)
				_anim.SetBool(key, true);

			_navi.SetDestination(_target.transform.position);
			distance = (_target.transform.position - transform.position).sqrMagnitude;

			yield return null;
		}

		onEnd?.Invoke(distance <= gap);
		_anim.SetBool(key, false);
		_navi.ResetPath();
		_isBlock = false;
	}

	#endregion

	public override NodeState Idle()
	{
		Utils.EditorLog("Idle");
		parentState = FSMType.IDLE;
		return NodeState.SUCCESS;
	}


	#region CLUSTERING

	public NodeState Clustering() 
	{
		if(_isBlock == false) 
		{
			var getAI = _detector.aiList.FirstOrDefault();			
			if(getAI != null)
			{
				_target = getAI as MonoBehaviour;
				var distance = (_target.transform.position - transform.position).sqrMagnitude;
				if(_detector.IsEndPos(distance))
				{
					Utils.EditorLog("Clustering");

					_detector.RemoveObj(_target as IObjectType);
					StopMove();

					SampleManager.Instance.RequestClustering(this, getAI as AITest);
					_isBlock = true;
				}				
			}
		}		

		return (_isBlock) ? NodeState.SUCCESS : NodeState.FAILUER;
	}

	public void ResponseClustering(AITest head = null, AITest child = null)
	{
		if(head != null && child != null)
		{
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
				
		_isBlock = false;
	}
	
	#endregion

	private IEnumerator DelayState(float delay, UnityAction onEnd) 
	{
		yield return new WaitForSeconds(delay);

		onEnd?.Invoke();		
	}


}
