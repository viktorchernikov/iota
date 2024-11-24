using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeypadKey : MonoBehaviour, IInteractable
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

    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        if (IsPressed)
            return InteractableHoverResponse.None;

        return controller.IsActive ? InteractableHoverResponse.None : InteractableHoverResponse.Enable;
    }

    public bool CanInteract(IInteractor interactor)
    {
        Player player = interactor as Player;
        if (!player) return false;

        return !IsPressed;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (IsPressed)
            return;
        StartCoroutine(OnPressedSequence());
    }

    IEnumerator OnPressedSequence()
    {
        IsPressed = true;

        if (!controller.IsActive)
        {
            controller.audioSource.Stop();
            controller.audioSource.time = 0;
            controller.audioSource.pitch = Random.Range(0.4f, 0.5f);

            controller.audioSource.clip = controller.buttonPressClip;
            //controller.animator.SetTrigger("OnPress");
            controller.audioSource.Play();

            controller.PasswordEntry(sKey);

            yield return new WaitForSeconds(controller.pressedDelay);
        }

        IsPressed = false;
    }
}