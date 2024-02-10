using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Flags]
public enum InteractionType 
{
    NONE,
    DETECTOR,
    WEAPON,
    PLAYER,
    AI
}

public interface IObjectType
{
    InteractionType ObjType();
}

public abstract class InteractionObject : MonoBehaviour, IObjectType
{
    public abstract InteractionType ObjType();

    protected List<IObjectType> _interactionList = new();

    protected InteractionType _typeFilter;

    public List<IObjectType> interactionList => _interactionList;

    protected virtual void Awake() 
    {
        _typeFilter = InteractionType.DETECTOR | InteractionType.WEAPON | InteractionType.PLAYER | InteractionType.AI;
    }

    public IObjectType GetObject(InteractionType type)
    {
        var getObject = _interactionList.FirstOrDefault(m => m.ObjType() == type);

        if(getObject != null)
            _interactionList.Remove(getObject);

        return getObject;
    }

    public List<IObjectType> GetObjects(InteractionType type) 
    {
        var getList = _interactionList.FindAll(m => m.ObjType() == type);

        if(getList != null)
            getList.ForEach(m => _interactionList.Remove(m));

        return getList;        
    }

	protected virtual void OnTriggerEnter(Collider other) 
    {
        if(other.TryGetComponent(out IObjectType obj))
        {
            if(_typeFilter.HasFlag(obj.ObjType()) == false)
                return;
            
            TriggerEnter(obj);
        }
    }

    protected virtual void OnTriggerExit(Collider other) 
    {
        if(other.TryGetComponent(out IObjectType obj))
        {
            if(_typeFilter.HasFlag(obj.ObjType()) == false)
                return;

            TriggerExit(obj);
        }
    }

    protected virtual void TriggerEnter(IObjectType other)
    {
        if(_interactionList.Contains(other) == false)
            _interactionList.Add(other);
    }

    protected virtual void TriggerExit(IObjectType other)
    {
        if(_interactionList.Contains(other))
            _interactionList.Remove(other);
    }
}