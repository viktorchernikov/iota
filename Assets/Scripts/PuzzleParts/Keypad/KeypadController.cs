using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class KeypadController : MonoBehaviour, IInteractable
{
    #region State
    public bool IsActive
    {
        get => _isActive;
        private set => _isActive = value;
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => _isBusy = value;
    }

    public float pressedDelay
    {
        get => _pressedDelay;
        private set => _pressedDelay = value;
    }

    public AudioSource audioSource
    {
        get => _audioSource;
        private set => _audioSource = value;
    }

    public AudioClip buttonPressClip
    {
        get => _buttonPressClip;
        private set => _buttonPressClip = value;
    }
    #endregion

    #region Editor State
    [Header("State")]
    [SerializeField] bool _isBusy = false;
    [SerializeField] bool _isActive = false;
    [SerializeField] string password;
    [SerializeField] int passwordLength = 4;
    #endregion
    #region Delays
    [Header("Delays")]
    [SerializeField] float _validDelay = 0.75f;
    [SerializeField] float _pressedDelay = 0.3f;
    #endregion
    #region Parameters
    [Header("Parameters")]
    [SerializeField] bool _isActiveDefault = false;
    #endregion
    #region Unity Events
    [Header("Events")]
    [SerializeField] UnityEvent _onActiveEvent;
    [SerializeField] UnityEvent _onDeactivateEvent;
    #endregion
    #region Components
    [Header("Components")]
    [SerializeField] TextMeshPro passwordText;
    [SerializeField] AudioSource _audioSource;
    #endregion
    #region Sounds
    [Header("Sounds")]
    [SerializeField] AudioClip _buttonPressClip;
    #endregion

    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        return IsActive ? InteractableHoverResponse.None : InteractableHoverResponse.Enable;
    }

    public void PasswordEntry(string number)
    {
        if (number.ToLower() == "enter")
        {
            StartCoroutine(Enter());
            return;
        }
        else if (number.ToLower() == "clear")
        {
            Clear();
            return;
        }

        if (passwordText.text.Length < passwordLength && !IsBusy) passwordText.text += number;
    }

    IEnumerator Enter()
    {
        IsBusy = true;

        if (passwordText.text == password)
        {
            passwordText.color = Color.green;
            _onActiveEvent.Invoke();
            IsActive = true;
        }
        else
        {
            passwordText.color = Color.red;
            _onDeactivateEvent.Invoke();
        }

        yield return new WaitForSeconds(_validDelay);
        if (!IsActive) Clear();

        IsBusy = false;
    }

    public void Clear()
    {
        passwordText.text = "";
        passwordText.color = Color.black;
    }
}
