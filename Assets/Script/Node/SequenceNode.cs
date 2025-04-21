namespace BT
{
	public class SequenceNode : Node
	{
		public SequenceNode()
		{
			nodeType = BTType.SEQUENCE;
		}

		public override BtState GetState()
		{
			var state = BtState.FAILUER;
			var isEnd = false;

			foreach(var child in _childs)
			{
				if(isEnd)
					continue;

				var childState = child.GetState();

				if((int)state < (int)childState)
					state = childState;

				if(childState == BtState.FAILUER)
					isEnd = true;
			}

			return state;
		}
	}
}