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
            EndHoveringOnHover();
            return;
        }
        var obj = hit.collider.gameObject;
        
        if (!obj.TryGetComponent(out IInteractable interactable))
        {
            EndHoveringOnHover();
            return;
        }

        if (interactable == _hoveredObject) return;
        
        EndHoveringOnHover();
        
        _currentHoveredGameObject = obj;

        var oldHover = _hoveredObject;
        _hoveredObject = interactable;
        
        if (obj.TryGetComponent<IHoverListener>(out var newHoverListener))
            newHoverListener.OnHoverStart(this.gameObject);
        
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

    private void EndHoveringOnHover()
    {
        if (_currentHoveredGameObject is not null && _currentHoveredGameObject.TryGetComponent<IHoverListener>(out var oldHoverListener))
            oldHoverListener.OnHoverEnd(this.gameObject);
            
        _hoveredObject = null;
        _currentHoveredGameObject = null;
    }
}