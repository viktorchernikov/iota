public readonly struct InteractionContext
{
    public InteractionContext(IInteractor interactor, IInteractable interactable, bool successful, float time)
    {
        this.interactor = interactor;
        this.interactable = interactable;
        this.successful = successful;
        this.time = time;
    }


    /// <summary>
    /// Reference to an interactor
    /// </summary>
    public readonly IInteractor interactor;
    /// <summary>
    /// Reference to the interactable object
    /// </summary>
    public readonly IInteractable interactable;
    /// <summary>
    /// Tells if this interaction was successful
    /// </summary>
    public readonly bool successful;
    /// <summary>
    /// When interaction happenned
    /// </summary>
    public readonly float time;
}
