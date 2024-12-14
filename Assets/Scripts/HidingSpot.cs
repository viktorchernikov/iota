using System.Collections;
using UnityEngine;

public class HidingSpot : MonoBehaviour, IInteractable
{
    public bool hasPlayer { get => _hasPlayer; private set => _hasPlayer = value; }
    public bool isBusy { get => _isBusy; private set => _isBusy = value; }

    public float delayEnter = 1f;
    public float delayExit = 1f;

    [Header("Teleport points")]
    public Transform exitPoint;
    public Transform hidePoint;

    [Header("State")]
    [SerializeField] bool _hasPlayer;
    [SerializeField] bool _isBusy;

    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        if (isBusy)
            return InteractableHoverResponse.None;

        return hasPlayer ? InteractableHoverResponse.Leave : InteractableHoverResponse.Enter;
    }
    public bool CanInteract(IInteractor interactor)
    {
        Player player = interactor as Player;

        if (!player) return false;

        return !isBusy;
    }
    public void OnInteract(IInteractor interactor)
    {
        Debug.Log("OnInteract1");
        if (isBusy)
            return;
        isBusy = true;
        Player player = (Player)interactor;
        Debug.Log("OnInteract2");

        if (hasPlayer)
        {
            Debug.Log("OnInteract3");
            StartCoroutine(OnPlayerLeave(player));
        }
        else
        {
            Debug.Log("OnInteract4");
            StartCoroutine(OnPlayerEnter(player))  ;
        }
    }

    IEnumerator OnPlayerEnter(Player player)
    {
        hasPlayer = true;
        player.HideInSpot(this);
        yield return new WaitForSeconds(player.hidingTime + delayEnter);
        isBusy = false;
    }
    IEnumerator OnPlayerLeave(Player player)
    {
        hasPlayer = false;
        player.UnhideFromSpot();
        yield return new WaitForSeconds(player.unhidingTime + delayExit);
        isBusy = false;
    }
}
