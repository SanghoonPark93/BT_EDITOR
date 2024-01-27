using BT;
using UnityEngine;

public class AI : MonoBehaviour
{
	private readonly float AttackDis = 5f;
	private readonly float DetectDis = 10f;
		
	private BTNode _btRoot;

	public int hp;
	public float playerDis;
	public bool isHit;
	public bool isUnItem;
	public bool isCheckPoint;

	protected virtual void Start()
	{
		var controller = Utils.GetJson<NodeController>(gameObject.name);
		
		if(controller.Root != null)
			_btRoot = new BTNode(controller, controller.Root, this);
	}

	#region DEFAULT_ACTION

	#endregion

	#region TestAction
	public bool IsDeath()
	{
		return (hp <= 0);
	}

	public void Death(bool isDeath)
	{
		if(isDeath == true)
			Debug.Log("사망");
	}

	public bool IsAttack()
	{
		return (playerDis <= AttackDis);
	}

	public void Attack(bool isAttack)
	{
		if(isAttack == true)
			Debug.Log("공격");
	}

	public bool IsHit()
	{
		if(isHit == true)
			Debug.Log("피격");

		return isHit;
	}

	public bool IsDetect()
	{
		var isDetect = (playerDis <= DetectDis);

		if(isDetect == true)
			Debug.Log("감지");

		return isDetect;
	}

	public void Move(bool isChase)
	{
		if(isChase == true)
			Debug.Log("추격");
	}

	public void Idle(bool isIdle)
	{
		if(isIdle == true)
			Debug.Log("대기");
	}

	public bool IsUnItem()
	{
		return isUnItem;
	}

	public void Pharming(bool isUnItem)
	{
		if(isUnItem == false)
			Debug.Log("파밍");
	}

	public bool IsCheckPoint()
	{
		return isCheckPoint;
	}

	public void MoveToCkPoint(bool isCheckPoint)
	{
		if(isCheckPoint == false)
			Debug.Log("거점이동");
	}
	#endregion

	private void Update()
	{
		//if(Input.GetKeyDown(KeyCode.O))
		//	Debug.Log($"count : {_behaviorTree.Count}");

		if(Input.GetKeyDown(KeyCode.P))
		{
//			foreach(var node in _behaviorTree)
//			{
//				var check = node.ActiveSelf();
//				node.CallAction(check);

//				if(check == true)
//				{
//#if UNITY_EDITOR
//					Debug.Log($"curNode : {node.name}");
//#endif
//					break;
//				}
//			}
		}
	}
}
