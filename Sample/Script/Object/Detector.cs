using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Detector : InteractionObject
{
	public override InteractionType ObjType() => InteractionType.DETECTOR;

	private List<IObjectType> _aiList = new();

	private List<IObjectType> _playerList = new();
	
	public List<IObjectType> aiList => _aiList;

	public List<IObjectType> playerList => _playerList;
		
	public bool isOn => _aiList.Any() || _playerList.Any();

	protected override void Awake()
	{
		_typeFilter = InteractionType.PLAYER | InteractionType.AI;
	}

	protected override void TriggerEnterListener(IObjectType other)
	{
		if(transform.parent.gameObject == (other as MonoBehaviour).gameObject)
			return;

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

	protected override void TriggerExitListener(IObjectType other)
	{
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
}
