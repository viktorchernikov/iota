using System;
using UnityEngine;

public class DoorCondition : MonoBehaviour, IDoorActivationCondition
{
    public event Action OnConditionFulfilmentChange;

    private bool _isActivated;
    
    public bool GetConditionFulfilment()
    {
        return _isActivated;
    }

    protected void SetConditionFulfilment(bool value)
    {
        _isActivated = value;

        OnConditionFulfilmentChange?.Invoke();
    }
}
    
