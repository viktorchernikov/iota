using UnityEngine;

public class DefaultPickable : MonoBehaviour, IPickupable
{
    [SerializeField] private float _holdingDistance = 3;
    
    public bool IsPicked { get; private set; } = false;

    public Transform self => transform;
    public float holdingDistance => _holdingDistance;
    
    private Rigidbody _rigidbody;

    public void Throw(Vector3 lookDir, float force)
    {
        IsPicked = false;
    }

    public bool CanInteract(IInteractor interactor)
    {
        return !IsPicked;
    }

    public void DropItself()
    {
        IsPicked = false;
    }

    public void OnInteract(IInteractor interactor)
    {
        IsPicked = true;
    }
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
}
   
