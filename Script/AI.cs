using BT;
using UnityEngine;

public abstract class AI : MonoBehaviour
{		
	protected BTNode _btRoot;
	
	#region TestAction
	
	public abstract NodeState Death();

	public abstract NodeState Attack();

	public abstract NodeState Hit();

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
		_btRoot.GetState();
	}
}
