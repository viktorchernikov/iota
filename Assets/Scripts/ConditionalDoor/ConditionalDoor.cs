using System.Collections.Generic;
using UnityEngine;

public abstract class ConditionalDoor : MonoBehaviour
{
    [SerializeReference] private List<Condition> _conditions = new List<Condition>();
    private List<Condition> _emittedConditions = new List<Condition>();
    
    private void Awake()
    {
        _conditions.ForEach(con => con.AddConditionListener(() => ConditionHandler(con)));
    }

    private void ConditionHandler(Condition condition)
    {
        if (_emittedConditions.Contains(condition)) return;
        
        _emittedConditions.Add(condition);

        if (_emittedConditions.Count == _conditions.Count)
        {
            OnAllConditionEmitted();
        }
    }

    protected virtual void OnAllConditionEmitted() {}
}
