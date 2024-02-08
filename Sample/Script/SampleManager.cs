using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleManager : MonoSingleton<SampleManager>
{
	[SerializeField]
	private List<AI> _aiList;

	[SerializeField]
	private Weapon _bulletPrefab;

	private List<AITest> _aiObserver = new();

	public static SampleManager Instance
	{
		get
		{
			if(_instance == null)
				CreateInstance("SampleManager");

			return _instance;
		}
	}

	private void Awake()
	{
		_aiList.ForEach(m => m.Initialize("AI"));
	}

	private void OnGUI()
	{
		if(GUI.Button(new Rect(0, 0, 100, 20), "Attack"))
		{
			var camTransfom = Camera.main.transform;
			var bulletPos = new Vector3(camTransfom.position.x, camTransfom.position.y - 6.5f, camTransfom.position.z);
			Instantiate(_bulletPrefab, bulletPos, new Quaternion(0, camTransfom.rotation.y, 0, 0));
		}
	}

	public void RequestClustering(AITest ai, AITest target)
	{
		if(_aiObserver.Contains(ai) && _aiObserver.Contains(target))
		{
			StopCoroutine(nameof(TimeOutClustering));

			var head = ai;
			var child = target;

			if(ai.value < target.value) 
			{
				head = target;
				child = ai;
			}

			ai.ResponseClustering(head, child);
			target.ResponseClustering(head, child);
			return;
		}

		_aiObserver.Add(ai);
		_aiObserver.Add(target);

		StartCoroutine(TimeOutClustering(ai, target));
	}

	private IEnumerator TimeOutClustering(AITest ai, AITest target) 
	{
		yield return new WaitForSeconds(3f);

		ai.ResponseClustering();
		target.ResponseClustering();
	}
}
