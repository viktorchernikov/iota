using System;

public interface IDoorActivationCondition
{
    public event Action OnConditionFulfilmentChange;
    public bool GetConditionFulfilment();
}
