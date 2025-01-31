using System;
using UnityEngine;
using UnityEngine.Serialization;

public class VentilationCover : MonoBehaviour, IInteractable
{
    
    [Header("Random falling")] [SerializeField]
    private float impulseForce = 2f;
    
    private bool _isOpened;
    private Rigidbody _rb;
    private BoxCollider _collider;
    [SerializeField] WaterDependant waterDependency;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
    }

    public bool CanInteract(IInteractor interactor)
    {
        return !_isOpened;
    }

    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        if (waterDependency.inWater)
        {
            return InteractableHoverResponse.WaterBlocked;
        }
        return InteractableHoverResponse.Enable;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (waterDependency.inWater)
        {
            return;
        }
        _rb.isKinematic = false;
        _rb.AddForce(transform.forward, ForceMode.Impulse);
        _isOpened = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}
