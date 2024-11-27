using UnityEngine;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] GameObject activePoint;
    [SerializeField] GameObject inactivePoint;

    [SerializeField] InteractableHoverResponse[] responseKeys;
    [SerializeField] GameObject[] responseValues;

    private Player _player;
    private PlayerInteractionSeeker _seeker;
    
    private void Start()
    {
        _seeker = Player.local.GetModule<PlayerInteractionSeeker>();
        _player = Player.local.GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        activePoint.SetActive(false);
        inactivePoint.SetActive(false);
        DisableResponseTextes();

        if (_seeker.HoveredObject == null) return;
        
        var res = _seeker.HoveredObject.GetHoverResponse(Player.local);

        if (res == InteractableHoverResponse.None) return;
        
        if (_seeker.HoveredObject.CanInteract(Player.local) && ((IInteractor)_player).CanInteract())
        {
            activePoint.SetActive(true);
            for (var i = 0; i < responseKeys.Length; i++)
            {
                if (responseKeys[i] == res)
                {
                    responseValues[i].SetActive(true);
                }
            }
        }
        else
        {
            inactivePoint.SetActive(true);
        }
    }
    void DisableResponseTextes()
    {
        foreach (var resText in responseValues)
        {
            resText.SetActive(false);
        }
    }
}
