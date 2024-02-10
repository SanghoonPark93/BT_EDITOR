using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Detector : InteractionObject
{
	public override InteractionType ObjType() => InteractionType.DETECTOR;

	private List<IObjectType> _aiList = new();

	private List<IObjectType> _playerList = new();

	public float radius { get; private set; }
	
	public bool Contains(GameObject obj) 
	{
		return (_interactionList.Any(m => (m as MonoBehaviour).gameObject == obj));
	}

	public List<IObjectType> aiList 
	{
		get 
		{
			_aiList.RemoveAll(m => m == null || (m as MonoBehaviour) == null);
			return _aiList;
		}
	}	

	public List<IObjectType> playerList 
	{
		get 
		{			
			_playerList.RemoveAll(m => m == null || (m as MonoBehaviour) == null);
			return _playerList;
		}
	}	
		
	public bool isOn => aiList.Any() || playerList.Any();

	protected override void Awake()
	{
		radius = GetComponent<SphereCollider>().radius;
		
		_typeFilter = InteractionType.PLAYER | InteractionType.AI;
	}

	protected override void TriggerEnter(IObjectType other)
	{
		if(transform.parent.gameObject == (other as MonoBehaviour).gameObject)
			return;

		base.TriggerEnter(other);

		switch(other.ObjType())
		{
			case InteractionType.PLAYER:
				if(_playerList.Contains(other) == false)
					_playerList.Add(other);
				break;

			case InteractionType.AI:
				if(_aiList.Contains(other) == false)
					_aiList.Add(other);
				break;
		}		
	}

	protected override void TriggerExit(IObjectType other)
	{
		base.TriggerExit(other);

		switch(other.ObjType())
		{
			case InteractionType.PLAYER:
				if(_playerList.Contains(other))
					_playerList.Remove(other);
				break;

			case InteractionType.AI:
				if(_aiList.Contains(other))
					_aiList.Remove(other);
				break;
		}
	}

	public IObjectType GetFirstObject(InteractionType type) 
	{		
		switch(type) 
		{
			case InteractionType.PLAYER:
				{
					var firstObj = _playerList.FirstOrDefault();

					if(firstObj != null)
						_playerList.Remove(firstObj);

					return firstObj;
				}				

			case InteractionType.AI:
				{
					var firstObj = _aiList.FirstOrDefault();

					if(firstObj != null)
						_aiList.Remove(firstObj);

					return firstObj;
				}				
		}

		return null;
	}

	public bool IsEndPos(float squareDis)
	{
		return (squareDis <= 50f);
	}

	public void RemoveObj(IObjectType type) 
	{
		if(_aiList.Contains(type))
			_aiList.Remove(type);

		if(_playerList.Contains(type))
			_playerList.Remove(type);

		if(_interactionList.Contains(type))
			_interactionList.Remove(type);
	}
}
