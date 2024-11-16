using System;
using UnityEngine;

public interface IInteractor
{
    /// <summary>
    /// Fired when interacted with some interactable
    /// </summary>
    public event Action<InteractionContext> onInteract;


    /// <summary>
    /// Tells if interactor can interact
    /// </summary>
    public virtual bool CanInteract()
    {
        return false;
    }
    /// <summary>
    /// Interacts with some interactable
    /// </summary>
    public virtual InteractionContext Interact(IInteractable interactable)
    {
        bool success = CanInteract() && interactable.CanInteract(this);
        var context = new InteractionContext(
            interactor: this,
            interactable: interactable,
            successful: success,
            time: Time.time);

        if (success)
        {
            OnInteract(context);
        }

        return context;
    }

    /// <summary>
    /// Called when player interacted
    /// </summary>
    protected virtual void OnInteract(InteractionContext context) { }
}
