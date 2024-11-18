using System;
using System.Collections.Generic;
using UnityEngine;


public abstract class Condition : MonoBehaviour
{
    private List<Action> handlers = new List<Action>();
    
    public void AddConditionListener(Action handler)
    {
        handlers.Add(handler);
    }

    protected void EmitCondition()
    {
        handlers.ForEach(handler => handler());
    }
}
