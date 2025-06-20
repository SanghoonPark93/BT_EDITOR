namespace BT
{
	public class RootNode : SelectorNode
	{
		protected override float width => 100f;
		protected override float height => 50f;

		public RootNode()
		{
			_typeName = "RootNode";
			_nodeType = BTType.ROOT;
		}
	}
}