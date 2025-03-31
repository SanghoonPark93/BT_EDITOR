namespace BT
{
	public class ConditionNode : Node
    {
		public ConditionNode()
		{
			nodeType = BTType.CONDITION;
		}

		public virtual bool CheckCondition() 
		{
			return true;
		}

		public override BtState GetState()
		{			
			return CheckCondition() ? BtState.SUCCESS : BtState.FAILUER;
		}
	}
}