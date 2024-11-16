public interface IInteractable
{
    public virtual InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        return InteractableHoverResponse.None;
    }
    public virtual bool CanInteract(IInteractor interactor)
    {
        return false;
    }
    /// <summary>
    /// Called when player interacted
    /// </summary>
    public virtual void OnInteract(IInteractor interactor) { }
}

public enum InteractableHoverResponse
{
    None,
    Enable,
    Disable,
    Pick,
    DropThrow,
    Drop,
    Jumpin
}