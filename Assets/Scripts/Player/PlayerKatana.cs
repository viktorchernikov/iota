using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerKatana : MonoBehaviour
{
    [SerializeField] private GameObject _katanaModel;
    [SerializeField] private PlayerKatanaState _state = PlayerKatanaState.Absent;
    // TODO: maybe better to make separate component for player input
    [SerializeField] private KeyCode _keyToHoldKatana = KeyCode.Alpha1;
    
    public event Action<PlayerKatanaState, PlayerKatanaState> OnKatanaStateChanged;
    public PlayerKatanaState State => _state;

    private PlayerInteractionSeeker _seeker;

    private void Awake()
    { 
        _katanaModel.SetActive(false);
        _seeker = GetComponent<PlayerInteractionSeeker>();
        _seeker.OnPlayerInteract += PlayerInteractHandler;
        if (State == PlayerKatanaState.Absent)
            _katanaModel.SetActive(false);
    }

    private void PlayerInteractHandler(GameObject obj)
    {
        if (!obj.CompareTag("katana") || _state != PlayerKatanaState.Absent) return;

        HoldKatana();
    }

    private void Update()
    {  
        PlayerInputPulse();
    }

    private void PlayerInputPulse()
    {
        if (State == PlayerKatanaState.Absent) return;
        if (!Input.GetKeyDown(_keyToHoldKatana)) return;
        
        var handlerToOldState = new Dictionary<PlayerKatanaState,Action>() 
        { 
            { PlayerKatanaState.Hided, HoldKatana },
            { PlayerKatanaState.Holding, HideKatana},
            { PlayerKatanaState.Absent, () => throw new NotImplementedException("Katana state not implemented.")}
        };

        handlerToOldState[_state]();
    }
    
    private void HoldKatana() 
    {
        _katanaModel.SetActive(true);
        var newState = PlayerKatanaState.Holding;
        OnKatanaStateChanged?.Invoke(_state, newState);
        _state = newState;
    }

    private void HideKatana()
    {
        _katanaModel.SetActive(false);
        var newState = PlayerKatanaState.Hided;
        OnKatanaStateChanged?.Invoke(_state, newState);
        _state = newState;
    }
}

public enum PlayerKatanaState
{
    Absent,
    Hided,
    Holding,
}

  
