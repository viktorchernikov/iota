using TMPro;
using UnityEngine;

public class ClickableButton : MonoBehaviour, IInteractable
{
    [SerializeField] private TextMeshPro tip;
    private bool _isClicked = false;

    public bool CanInteract(IInteractor interactor)
    {
        return !_isClicked;
    }

    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        return InteractableHoverResponse.Enable;
    }

    public void OnInteract(IInteractor interactor)
    {
        _isClicked = true;
        tip.text = "Clicked";
    }
}
