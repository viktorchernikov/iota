using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Valve : MonoBehaviour, IInteractable
{
    #region State
    public bool isOpen { get => _isOpen; private set => _isOpen = value; }
    public bool isHolding { get => _isHolding; private set => _isHolding = value; }
    #endregion

    #region Editor State
    [Header("State")]
    [SerializeField] bool _isOpen = false;
    [SerializeField] bool _isHolding = false;
    [SerializeField] float minState = 0f;
    [SerializeField] float maxState = 100f;
    [SerializeField] float currentState = 0f;
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
    //[SerializeField] Animator _animator;
    //[SerializeField] AudioSource _audioSource;
    #endregion
    #region Sounds
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
        // _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f

        if (Input.GetKeyUp(KeyCode.E) && isHolding && currentState < maxState)
        {
            //_animator.speed = 0;
            //_audioSource.Pause();
            return;
        }
        StartCoroutine(ChangeState());
    }

    IEnumerator ValveSequence()
    {
        if (!isHolding)
        {
            isHolding = true;
            //_audioSource.Stop();
            //_audioSource.time = 0;
            //_audioSource.pitch = 0.95f; // UnityEngine.Random.Range(0.8f, 1.05f);
            //_audioSource.clip = _valveTurnSound;
            if (isOpen)
            {
                //_animator.SetTrigger("OnClose_Start");
                //_animator.SetTrigger("OnClose_End");
            }
            else
            {
                //_animator.SetTrigger("OnOpen_Start");
                //_animator.SetTrigger("OnOpen_End");
            }
            //_audioSource.Play();
        }
        else
        {
            //_animator.speed = 1;
            //_audioSource.Play();
        }

        yield return StartCoroutine(ChangeState());
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

    IEnumerator ChangeState()
    {
        if (isOpen && currentState > minState)
        {
            currentState -= (Time.deltaTime * 100);
            Debug.Log($"Current time: {currentState}");
        }
        if (!isOpen && currentState < maxState)
        {
            currentState += (Time.deltaTime * 100);
            Debug.Log($"Current time: {currentState}");
        }

        yield return null;
    }
}
