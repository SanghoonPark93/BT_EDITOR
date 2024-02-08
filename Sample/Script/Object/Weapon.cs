using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour, IObjectType
{
	private float _speed;

	public InteractionType ObjType() => InteractionType.WEAPON;
	
	public float damage { get; private set; }

	private void Awake()
	{
		_speed = 50f;
		damage = 1f;

		StartCoroutine(DelayDestroy());
	}

	private void Update()
	{
		transform.Translate(Vector3.forward * _speed * Time.deltaTime);
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.TryGetComponent(out IObjectType obj))
		{
			if(obj.ObjType() == InteractionType.DETECTOR)
				return;

			DestroyObj();
		}
	}

	private IEnumerator DelayDestroy() 
	{
		yield return new WaitForSeconds(10f);

		DestroyObj();
	}

	private void DestroyObj() 
	{
		StopAllCoroutines();
		Destroy(gameObject);
	}
}
