using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour, IInteractable
{
    #region State
    public bool isActive { get => _isActive; private set => _isActive = value; }
    public bool isBusy { get => _isBusy; private set => _isBusy = value; }
    public bool isScaleSensitive = false;
    public float minScale = 0f;
    public float maxScale = 2f;
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
    [SerializeField] float _switchAnimationDelay = 0.4f;
    [SerializeField] float _switchPostDelay = 0.2f;
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
    [SerializeField] AudioClip _leverUpClip;
    [SerializeField] AudioClip _leverDownClip;
    #endregion


    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        if (isBusy)
            return InteractableHoverResponse.None;
        
        return isActive ? InteractableHoverResponse.Disable : InteractableHoverResponse.Enable;
    }
    public bool CanInteract(IInteractor interactor)
    {
        Player player = interactor as Player;

        if (!player) return false;
        
        bool scaleFactor = player.currentScale >= minScale && player.currentScale <= maxScale;
        return !isBusy && (isScaleSensitive ? scaleFactor : true);
    }
    public void OnInteract(IInteractor interactor)
    {
        if (isBusy)
            return;
        StartCoroutine(SwitchSequence());
    }

    IEnumerator SwitchSequence()
    {
        isBusy = true;
        _audioSource.Stop();
        _audioSource.time = 0;
        _audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.05f);
        if (isActive)
        {
            _audioSource.clip = _leverDownClip;
            _animator.SetTrigger("OnDeactivate");
        }
        else
        {
            _audioSource.clip = _leverUpClip;
            _animator.SetTrigger("OnActivate");
        }
        _audioSource.Play();

        yield return new WaitForSeconds(_switchAnimationDelay);

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

        yield return new WaitForSeconds(_switchPostDelay);

        isBusy = false;
    }
}
