using BT;
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

	public FSMType parentState { get; private set; }

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

	protected override void UpdateBody()
	{
		//��θӸ��� ���� ��� ��θӸ��� �ൿ������ ����
		if(_head != null && _isBlock == false)
		{
			if(isAlive == false) 
			{
				Death();
				return;
			}

			if(Hit() != NodeState.FAILUER)
				return;

			switch(_head.parentState) 
			{
				case FSMType.ATTACK:
					break;

				case FSMType.MOVE:
					break;

				case FSMType.IDLE:
					break;
			}			

			return;
		}

		//����̰ų� ������ �̷�� ���� �ʴٸ� �����̺�� Ʈ�� Ž��
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

	#region HP

	public override NodeState Hit()
	{
		var isHit = _hitInfoQueue.Any();
		if(isHit)
		{
			var key = "Hit";
			Debug.Log(key);
						
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
				StartCoroutine(DelayState(1.3f, () =>
				{
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

		var isOn = _detector.playerList.Any();
		if(isOn)
		{
			var findPlayer = _detector.playerList.FirstOrDefault();

			if(findPlayer != null)
				_target = (findPlayer as MonoBehaviour);

			if(_target != null) 
			{
				var distance = (_target.transform.position - transform.position).sqrMagnitude;

				if(_detector.IsEndPos(distance) == false)
					return NodeState.FAILUER; //Ÿ�� ������ ����	

				StopMove();

				parentState = FSMType.ATTACK;
				var key = "Attack";

				if(_anim.GetBool(key) == false)
				{
					_anim.SetBool(key, true);
					StartCoroutine(DelayState(1.3f, () => _anim.SetBool(key, false)));
				}
			}			
		}

		return (isOn) ? _stateCache : NodeState.FAILUER;		
	}

	public override NodeState Move()
	{
		var hasTarget = (_target != null);

		if(hasTarget && _isBlock == false) 
		{
			Debug.Log("Move");
			parentState = FSMType.MOVE;
						
			StartCoroutine(StartMove(50f));
			_isBlock = true;
		}
		
		return (hasTarget) ? NodeState.SUCCESS : NodeState.FAILUER;
	}

	public override NodeState Idle()
	{
		Debug.Log("Idle");
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
					Debug.Log("Clustering");
					StopMove();
					_detector.RemoveObj(_target as IObjectType);
					SampleManager.Instance.RequestClustering(this, getAI as AITest);
					_isBlock = true;
				}				
			}
		}		

		return (_isBlock) ? NodeState.SUCCESS : NodeState.FAILUER;
	}

	public void ResponseClustering(AITest head = null, AITest child = null)
	{
		if(head == null && child == null)
		{
			_isBlock = false;
			return;
		}

		if(head == this) // ���� �����
		{
			if(_child == null)
				_child = new List<AITest>();

			_child.Add(child);
		}
		else // ���� �ڽ��̶��
		{
			_head = head;

			//���� �������� ���� ��ü�̸� ���ķδ� ���� ��ü�� �����Ϳ� ����
			_detector.gameObject.SetActive(false);

			//MoveTo(_head.transform, 50f);			
		}

		_str += 1f;
	}
	
	#endregion

	private IEnumerator DelayState(float delay, UnityAction onEnd) 
	{
		yield return new WaitForSeconds(delay);

		onEnd?.Invoke();		
	}

	private void StopMove() 
	{
		StopCoroutine(nameof(StartMove));

		_target = null;

		_navi.ResetPath();

		_anim.SetBool("Move", false);
	}

	private IEnumerator StartMove(float gap, UnityAction<bool> onEnd = null)
	{		 
		var distance = (_target.transform.position - transform.position).sqrMagnitude;
		var key = "Move";
				
		while(_target != null)
		{
			//������ ���� �ȿ� ���ų� ���������� �����ߴٸ� �ݺ��� Ż��
			if(_detector.Contains(_target.gameObject) == false || _detector.IsEndPos(distance))
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
}
