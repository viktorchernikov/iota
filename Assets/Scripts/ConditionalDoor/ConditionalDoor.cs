using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConditionalDoor : MonoBehaviour
{
    [SerializeReference] private List<DoorCondition> _activators = new List<DoorCondition>();
    private Transform _transform;
    
    private void Awake()
    {
        _activators.ForEach(doorActivator => doorActivator.OnConditionFulfilmentChange += ConditionHandler);
        _transform = GetComponent<Transform>();
    }

    private void ConditionHandler()
    {
        if (_activators.All(activator => activator.GetConditionFulfilment())) Open();
        else Close();
    }

    private void Open()
    {
        _transform.rotation = Quaternion.Euler( _transform.rotation.eulerAngles.x, 90,  _transform.rotation.eulerAngles.z);
    }

    private void Close()
    {
        _transform.rotation = Quaternion.Euler( _transform.rotation.eulerAngles.x, 0,  _transform.rotation.eulerAngles.z);
    }
}
