using TMPro;
using UnityEngine;

public class ClickableButton : MonoBehaviour, IInteractable, IRestartable
{
    [SerializeField] private TextMeshPro tip;
    private bool _isClicked;

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

    public void RestartState()
    {
        _isClicked = false;
        tip.text = "Was not clicked";
    }
}
