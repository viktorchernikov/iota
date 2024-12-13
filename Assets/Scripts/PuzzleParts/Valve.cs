using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Valve : MonoBehaviour, IInteractable
{
    #region State
    public bool isActive { get => _isActive; private set => _isActive = value; }
    public bool isBusy { get => _isBusy; private set => _isBusy = value; }
    #endregion
    #region Events
    public event Action onActivate;
    public event Action onDeactivate;
    #endregion

    #region Editor State
    [Header("State")]
    [SerializeField] bool _isActive = false;
    [SerializeField] bool _isBusy = false;
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
    [SerializeField] bool _isActiveDefault = false;
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
        if (isBusy)
            return InteractableHoverResponse.None;

        return isActive ? InteractableHoverResponse.Close : InteractableHoverResponse.Open;
    }

    public bool CanInteract(IInteractor interactor)
    {
        Player player = interactor as Player;
        if (!player) return false;

        return !isBusy;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (isBusy)
            return;
        StartCoroutine(ValveSequence());
    }

    IEnumerator ValveSequence()
    {
        isBusy = true;
        _audioSource.Stop();
        _audioSource.time = 0;
        _audioSource.pitch = 0.95f; // UnityEngine.Random.Range(0.8f, 1.05f);
        _audioSource.clip = _valveTurnSound;
        if (isActive)
        {
            _animator.SetTrigger("OnClose");
        }
        else
        {
            _animator.SetTrigger("OnOpen");
        }
        _audioSource.Play();

        yield return new WaitForSeconds(_valveAnimationDelay);

        isActive = !isActive;
        if (isActive)
        {
            _onActivateEvent.Invoke();
            onActivate?.Invoke();
        }
        else
        {
            _onDeactivateEvent.Invoke();
            onDeactivate?.Invoke();
        }

        yield return new WaitForSeconds(_valvePostDelay);

        isBusy = false;
    }
}
