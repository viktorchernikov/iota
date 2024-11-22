using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuttingSpot : MonoBehaviour, IInteractable, IHoverListener
{
    [SerializeField] private DefaultPickable objectToPut;
    [SerializeField] private bool hasBeenPutted = false;
    [SerializeField] private GameObject model;
    [SerializeField] private Material materialOnHover;

    public event Action<PuttingSpot> OnPut;
    
    private MeshRenderer _renderer;
    private MeshFilter _mesh;
    private Material[] _materialsOnHover;
    private Mesh _meshOnHover;
    private Material[] _materialsOnPut;
    
    private void Awake()
    {
        _renderer = model.GetComponent<MeshRenderer>();
        _mesh = model.GetComponent<MeshFilter>();
        
        _meshOnHover = objectToPut.GetMeshFilter().sharedMesh;
        _materialsOnPut = objectToPut.GetRenderer().materials;

        _materialsOnHover = new Material[_materialsOnPut.Length + 1];

        for (var i = 0; i < _materialsOnPut.Length; i++)
            _materialsOnHover[i] = _materialsOnPut[i];

        _materialsOnHover[_materialsOnPut.Length] = materialOnHover;

        model.transform.localScale = objectToPut.GetModelScale();
    }

    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        return InteractableHoverResponse.Enable;
    }

    public bool CanInteract(IInteractor interactor)
    {
        return ShouldShowGhost(interactor.self.gameObject) ;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (!ShouldShowGhost(interactor.self.gameObject)) return;

        interactor.self.gameObject.GetComponent<PlayerHoldingModule>().DeleteHoldingObject();
        
        hasBeenPutted = true;
        
        OnPut?.Invoke(this);

        _renderer.materials = _materialsOnPut;
        _mesh.sharedMesh = _meshOnHover;
    }

    public void OnHoverStart(GameObject emitter)
    {
        if (!ShouldShowGhost(emitter)) return;
        
        _renderer.materials = _materialsOnHover;
        _mesh.sharedMesh = _meshOnHover;
    }

    public void OnHoverEnd(GameObject emitter)
    {
        if (hasBeenPutted) return;
        
        _renderer.materials = Array.Empty<Material>();
        _mesh.sharedMesh = null;
    }

    private bool ShouldShowGhost(GameObject interactor)
    {
        return !hasBeenPutted && interactor.TryGetComponent<PlayerHoldingModule>(out var holdingModule) &&
               ReferenceEquals(holdingModule.currentlyHolding, objectToPut);
    }
}

