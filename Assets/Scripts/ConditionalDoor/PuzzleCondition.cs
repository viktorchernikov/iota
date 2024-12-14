using UnityEngine;
using UnityEngine.Events;

public class PuzzleCondition : MonoBehaviour
{
    public bool wasFulfilled = false;
    public UnityEvent onFulfilmentChange;
    public UnityEvent onFulfilled;
    public UnityEvent onUnfulfilled;

    public virtual bool GetConditionFulfilment()
    {
        return wasFulfilled;
    }
    public void Fulfill()
    {
        if (wasFulfilled) return;

        wasFulfilled = true;
        onFulfilled?.Invoke();
        onFulfilmentChange?.Invoke();
    }
    public void Unfulfill()
    {
        if (!wasFulfilled) return;

        wasFulfilled = false;
        onUnfulfilled?.Invoke();
        onFulfilmentChange?.Invoke();
    }
    public void InvertFulfillment()
    {
        SetConditionFulfilment(!wasFulfilled);
    }
    public void SetConditionFulfilment(bool value)
    {
        if (wasFulfilled == value) return;
        if (value)
        {
            Fulfill();
        }
        else
        {
            Unfulfill();
        }
    }
}
