using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class KeypadController : MonoBehaviour
{
    #region State
    public bool IsActive
    {
        get => _isActive;
        private set => _isActive = value;
    }
    #endregion
    #region Events
    //public event Action onActive;
    //public event Action onDeactive;
    #endregion

    #region Editor State
    [Header("State")]
    [SerializeField] bool _isActive = false;
    [SerializeField] string password;
    #endregion
    #region Delays
    [Header("Delays")]
    //[SerializeField] float _switchAnimationDelay = 0.4f;
    [SerializeField] float _validDelay = 0.75f;
    #endregion
    #region Parameters
    [Header("Parameters")]
    [SerializeField] bool _isActiveDefault = false;
    #endregion
    #region Unity Events
    //[Header("Events")]
    //[SerializeField] UnityEvent _onActiveEvent;
    //[SerializeField] UnityEvent _onDeactivateEvent;
    #endregion
    #region Components
    [Header("Components")]
    [SerializeField] TextMeshPro passwordText;
    #endregion

    // Start is called before the first frame update
    //void Start()
    //{
    //    passwordText.text = "";
    //}
    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        //if (IsPressed)
        //    return InteractableHoverResponse.None;

        return IsActive ? InteractableHoverResponse.Disable : InteractableHoverResponse.Enable;
    }

    public void PasswordEntry(string number)
    {
        if (number == "Clear")
        {
            Clear();
            return;
        }
        else if (number == "Enter")
        {
            StartCoroutine(Enter());
            return;
        }

        Debug.Log(number);
        passwordText.text += number;
    }

    //private void Enter()
    //{
    //    if (passwordText.text == password)
    //    {
    //        door.lockedByPassword = false;

    //        if (audioSource != null)
    //            audioSource.PlayOneShot(correctSound);

    //        passwordText.color = Color.green;
    //        StartCoroutine(WaitAndClear());
    //    }
    //    else
    //    {
    //        if (audioSource != null)
    //            audioSource.PlayOneShot(wrongSound);

    //        passwordText.color = Color.red;
    //        StartCoroutine(WaitAndClear());
    //    }
    //}

    IEnumerator Enter()
    {
        if (passwordText.text == password)
        {
            passwordText.color = Color.green;
            IsActive = true;
        }
        else
        {
            passwordText.color = Color.red;
        }

        yield return new WaitForSeconds(_validDelay);
        if (!IsActive) Clear();
    }

    public void Clear()
    {
        passwordText.text = "";
        passwordText.color = Color.white;
    }
}
