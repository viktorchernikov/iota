using UnityEngine;

public class ItemToTake : MonoBehaviour, IInteractable 
{
   public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
   {
      return InteractableHoverResponse.Take;
   }

   public bool CanInteract(IInteractor interactor)
   {
      return true;
   }

   public void OnInteract(IInteractor interactor)
   {
      gameObject.SetActive(false);
   }
}
