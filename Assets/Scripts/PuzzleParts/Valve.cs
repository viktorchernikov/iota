using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Valve : MonoBehaviour, IInteractable
{
    #region State
    public bool isOpen { get => _isOpen; private set => _isOpen = value; }
    public bool isHolding { get => _isHolding; private set => _isHolding = value; }
    public bool isStarted { get => _isStarted; private set => _isStarted = value; }
    #endregion

    #region Editor State
    [Header("State")]
    [SerializeField] bool _isOpen = false;
    [SerializeField] bool _isHolding = false;
    [SerializeField] bool _isStarted = false;
    #endregion
    #region Unity Events
    [Header("Events")]
    [SerializeField] UnityEvent _onActivateEvent;
    [SerializeField] UnityEvent _onDeactivateEvent;
    #endregion
    #region Delays
    [Header("Delays")]
    [SerializeField] float _valveAnimationDelay = 0.4f;
    [SerializeField] float _valvePostDelay = 0.2f;
    #endregion
    #region Parameters
    [Header("Parameters")]
    [SerializeField] bool _isOpenDefault = false;
    #endregion
    #region Components
    [Header("Components")]
    [SerializeField] Animator _animator;
    [SerializeField] AudioSource _audioSource;
    #endregion
    #region
    [Header("Sounds")]
    [SerializeField] AudioClip _valveTurnSound;
    #endregion

    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        if (isHolding)
            return InteractableHoverResponse.None;

        return isOpen ? InteractableHoverResponse.Close : InteractableHoverResponse.Open;
    }

    public bool CanInteract(IInteractor interactor)
    {
        Player player = interactor as Player;
        if (!player) return false;

        return !isHolding;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (isHolding)
            return;
        StartCoroutine(ValveSequence());
    }

    IEnumerator ValveSequence()
    {
        isHolding = true;
        _audioSource.Stop();
        _audioSource.time = 0;
        _audioSource.pitch = 0.95f; // UnityEngine.Random.Range(0.8f, 1.05f);
        _audioSource.clip = _valveTurnSound;
        if (isOpen)
        {
            _animator.SetTrigger("OnClose");
        }
        else
        {
            _animator.SetTrigger("OnOpen");
        }
        _audioSource.Play();

        yield return new WaitForSeconds(_valveAnimationDelay);

        isOpen = !isOpen;
        if (isOpen)
        {
            _onActivateEvent.Invoke();
        }
        else
        {
            _onDeactivateEvent.Invoke();
        }

        yield return new WaitForSeconds(_valvePostDelay);

        isHolding = false;
    }
}
