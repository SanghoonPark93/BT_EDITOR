using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleManager : MonoSingleton<SampleManager>
{
	[SerializeField]
	private List<AI> _aiList;	

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

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.P))
			Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None: CursorLockMode.Locked;
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

		_aiObserver.RemoveAll(m => m == ai);
		_aiObserver.RemoveAll(m => m == target);

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
