using System;
using UnityEngine;

public class PlayerInteractionSeeker : PlayerModule
{
    [SerializeField] private float maxSeekDistance = 3f;
    [SerializeField] private LayerMask tracedLayers = default(LayerMask);
    
    public event Action<IInteractable, IInteractable> OnHoveredChange;
    public event Action<GameObject> OnPlayerInteract; 
    public IInteractable HoveredObject { get => _hoveredObject; }
    
    private GameObject _currentHoveredGameObject;
    private IInteractable _hoveredObject;
    private PlayerGroundMotor _groundMotor;

    public void Awake()
    {
        _groundMotor = GetComponent<PlayerGroundMotor>();
    }
    
    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
        
        if (!Physics.Raycast(parent.usedCamera.forwardRay, out var hit, maxSeekDistance * parent.currentScale,
                tracedLayers.value))
        {
            _hoveredObject = null;
            return;
        }
        var obj = hit.collider.gameObject;

        if (!obj.TryGetComponent(out IInteractable interactable))
        {
            _hoveredObject = null;
            return;
        }


        if (interactable == _hoveredObject) return;
        
        _currentHoveredGameObject = obj;

        var oldHover = _hoveredObject;
        _hoveredObject = interactable;
        OnHoveredChange?.Invoke(oldHover, interactable);
    }
    
    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        if (_hoveredObject == null) return;
        if (!_hoveredObject.CanInteract(parent) || !GetInput() || !_groundMotor.grounded) return;
        
        _hoveredObject.OnInteract(parent);
        OnPlayerInteract?.Invoke(_currentHoveredGameObject);
    }

    bool GetInput()
    {
        return Input.GetKeyDown(KeyCode.E);
    }
   
}