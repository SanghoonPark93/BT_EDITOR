using UnityEngine;

public class Player : InteractionObject
{
	[SerializeField] InteractionType _type;
	public override InteractionType ObjType() => _type;
}
