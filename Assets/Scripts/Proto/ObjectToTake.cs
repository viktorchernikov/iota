using UnityEngine;

public class ObjectToTake : MonoBehaviour, IInteractable 
{
   public bool CanInteract(IInteractor interactor)
   {
      return true;
   }

   public void OnInteract(IInteractor interactor)
   {
      gameObject.SetActive(false);
   }
}
