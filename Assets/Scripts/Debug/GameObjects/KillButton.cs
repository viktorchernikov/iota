using UnityEngine;

public class KillButton : MonoBehaviour, IInteractable
{
    public bool CanInteract(IInteractor interactor)
    {
        return true;
    }

    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        return InteractableHoverResponse.Enable;
    }

    public void OnInteract(IInteractor interactor)
    {
        Player.local.PrepareToDie(transform.position);
        Player.local.Die();
        LevelRestart.local.Restart();
        
    }
}
