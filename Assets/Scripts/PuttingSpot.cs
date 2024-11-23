using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PuttingSpot : MonoBehaviour, IInteractable, IHoverListener
{
    [SerializeField] private GameObject spotModel;
    [SerializeField] private DefaultPickable objectToPut;
    [SerializeField] private Material materialOnHover;

    public event Action<PuttingSpot> OnPut;

    private bool _hasBeenPutted;
    private MeshRenderer _spotRenderer;
    private MeshFilter _spotMesh;
    private Material[] _materialsMixOnHover;
    private Material[] _objectToPutMaterials;
    private Mesh _objectOnPutMesh;
    
    private void Awake()
    {
        _spotRenderer = spotModel.GetComponent<MeshRenderer>();
        _spotMesh = spotModel.GetComponent<MeshFilter>();
        
        _objectOnPutMesh = objectToPut.GetMeshFilter().sharedMesh;
        _objectToPutMaterials = objectToPut.GetRenderer().materials;

        _materialsMixOnHover = new Material[_objectToPutMaterials.Length + 1];

        for (var i = 0; i < _objectToPutMaterials.Length; i++)
            _materialsMixOnHover[i] = _objectToPutMaterials[i];

        _materialsMixOnHover[_objectToPutMaterials.Length] = materialOnHover;

        spotModel.transform.localScale = objectToPut.GetModelScale();
        _spotRenderer.materials = Array.Empty<Material>();
        _spotMesh.sharedMesh = null;
    }

    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        return InteractableHoverResponse.Put;
    }

    public bool CanInteract(IInteractor interactor)
    {
        return ShouldShowGhost(interactor.self.gameObject) ;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (!ShouldShowGhost(interactor.self.gameObject)) return;

        interactor.self.gameObject.GetComponent<PlayerHoldingModule>().DeleteHoldingObject();
        
        _hasBeenPutted = true;
        
        OnPut?.Invoke(this);

        _spotRenderer.materials = _objectToPutMaterials;
        _spotMesh.sharedMesh = _objectOnPutMesh;
    }

    public void StartHover(GameObject emitter)
    {
        if (!ShouldShowGhost(emitter)) return;
        
        _spotRenderer.materials = _materialsMixOnHover;
        _spotMesh.sharedMesh = _objectOnPutMesh;
    }

    public void EndHover(GameObject emitter)
    {
        if (_hasBeenPutted) return;
        
        _spotRenderer.materials = Array.Empty<Material>();
        _spotMesh.sharedMesh = null;
    }

    private bool ShouldShowGhost(GameObject interactor)
    {
        return !_hasBeenPutted && interactor.TryGetComponent<PlayerHoldingModule>(out var holdingModule) &&
               ReferenceEquals(holdingModule.currentlyHolding, objectToPut);
    }
}

