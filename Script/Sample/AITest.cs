using BT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : AI
{
	public override NodeState Attack()
	{
		Debug.Log("Attack");

		return NodeState.SUCCESS;		
	}

	public override NodeState Death()
	{
		Debug.Log("Death");

		return NodeState.SUCCESS;
	}

	public override NodeState Hit()
	{
		Debug.Log("Hit");

		return NodeState.SUCCESS;
	}

	public override NodeState Idle()
	{
		Debug.Log("Idle");

		return NodeState.SUCCESS;
	}

	public override NodeState Move()
	{
		Debug.Log("Move");

		return NodeState.SUCCESS;
	}

	protected override void Update() 
	{
		if(Input.GetKeyDown(KeyCode.O))
			Debug.Log($"count : {_btRoot.GetAllNodes().Count}");

		if(Input.GetKeyDown(KeyCode.P))
			base.Update();
	}
}
