namespace BT
{
	public class SelectorNode : Node
	{
		public SelectorNode()
		{
			nodeType = BTType.SELECTOR;
		}

		public override BtState GetState()
		{
			var state = BtState.FAILUER;
			var isEnd = false;

			foreach(var child in _childs)
			{
				if(isEnd)
				{
					isFirstTurn = true;
					continue;
				}

				var childState = child.GetState();

				if(childState != BtState.FAILUER)
				{
					state = childState;
					isEnd = true;
				}
			}

			return state;
		}
	}
}