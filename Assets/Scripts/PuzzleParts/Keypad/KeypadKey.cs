using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeypadKey : MonoBehaviour
{
    #region State
    public bool IsPressed
    {
        get => _isPressed;
        private set => _isPressed = value;
    }
    #endregion

    #region Editor State
    [Header("State")]
    [SerializeField] bool _isPressed = false;
    [SerializeField] string sKey;
    #endregion
    #region Components
    [Header("Components")]
    [SerializeField] Text key;
    [SerializeField] KeypadController controller;
    #endregion

    //public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    //{
    //    if (IsPressed)
    //        return InteractableHoverResponse.None;

    //    return controller.IsActive ? InteractableHoverResponse.Disable : InteractableHoverResponse.Enable;
    //}

    public bool CanInteract(IInteractor interactor)
    {
        Player player = interactor as Player;
        if (!player) return false;

        return true;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (IsPressed)
            return;
        OnPressedKey();
    }

    public void OnPressedKey()
    {
        IsPressed = true;
        controller.PasswordEntry(sKey);

        IsPressed = false;
    }
}