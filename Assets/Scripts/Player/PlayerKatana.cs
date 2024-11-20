using System;
using UnityEngine;

public class PlayerKatana : MonoBehaviour
{
    [SerializeField] private GameObject _katanaModel;
    [SerializeField] private PlayerKatanaState _katanaState = PlayerKatanaState.Absent;
    // TODO: maybe better to make separate component for player input
    [SerializeField] private KeyCode _keyToHoldKatana = KeyCode.Alpha1;
    
    public event Action<PlayerKatanaState, PlayerKatanaState> OnKatanaStateChanged;
    public PlayerKatanaState KatanaState => _katanaState;

    private PlayerInteractionSeeker _seeker;

    private void Awake()
    { 
        _katanaModel.SetActive(false);
        _seeker = GetComponent<PlayerInteractionSeeker>();
        _seeker.OnPlayerInteract += PlayerInteractHandler;
    }

    private void PlayerInteractHandler(GameObject obj)
    {
        if (!obj.CompareTag("katana") || _katanaState != PlayerKatanaState.Absent) return;
        
        var oldState = _katanaState;
        _katanaState = PlayerKatanaState.Hided;
        OnKatanaStateChanged?.Invoke(oldState, _katanaState);
    }

    private void Update()
    {  
        PlayerInputPulse();
    }

    private void PlayerInputPulse()
    {
        if (KatanaState == PlayerKatanaState.Absent) return;
        if (!Input.GetKeyDown(_keyToHoldKatana)) return;

        _katanaState = _katanaState switch
        {
            PlayerKatanaState.Hided => HoldKatana(),
            PlayerKatanaState.Holding => HideKatana(),
            _ => throw new NotImplementedException("Katana state not implemented.")
        };
    }
    
    private PlayerKatanaState HoldKatana() {
        _katanaModel.SetActive(true);
        var newState = PlayerKatanaState.Holding;
        OnKatanaStateChanged?.Invoke(PlayerKatanaState.Hided, newState);
        return newState;
    }

    private PlayerKatanaState HideKatana()
    {
        _katanaModel.SetActive(false);
        var newState = PlayerKatanaState.Hided;
        OnKatanaStateChanged?.Invoke(PlayerKatanaState.Holding, newState);
        return newState;
    }
}

public enum PlayerKatanaState
{
    Absent,
    Hided,
    Holding,
}

  
