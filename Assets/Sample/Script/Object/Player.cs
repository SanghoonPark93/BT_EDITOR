using Core;
using UnityEngine;

namespace BT.Sample
{
	public class Player : InteractionObject
	{
		[SerializeField]
		private Transform _point;

		[SerializeField]
		private Weapon _bulletPrefab;

		[SerializeField] InteractionType _type;
		public override InteractionType ObjType() => _type;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, 1);
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				var bulletPos = transform.position.OpY(1.5f).OpZ(1f);
				var bullet = Instantiate(_bulletPrefab, _point.position, _point.rotation);
			}
		}
	}
}
