using UnityEngine;

public class Button : DoorCondition
{
    [SerializeField] private KeyCode keyToActivate;
    private Renderer _rend;
    
    private void Awake()
    {
        _rend = GetComponentInChildren<Renderer>();
        _rend.material.color = Color.red;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(keyToActivate)) return;
        
        SetConditionFulfilment(!GetConditionFulfilment());
            
        _rend.material.color = GetConditionFulfilment() ? Color.green: Color.red;
    }
}
