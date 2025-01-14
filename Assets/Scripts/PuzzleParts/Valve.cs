using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Valve : MonoBehaviour, IInteractable
{
    #region State
    public float value
    {
        get => _value;
        private set
        {
            _value = Mathf.Clamp01(value);
            if (_value == 1)
            {
                isTurningRight = false;
            }
            else if (_value == 0)
            {
                isTurningRight = true;
            }
        }
    }
    public bool isCooldowned { get => _isCooldowned; private set => _isCooldowned = value; }
    public bool isTurningRight { get => _isTurningRight; private set => _isTurningRight = value; }
    #endregion

    #region Editor State
    [Header("State")]
    [SerializeField] float _value = 0.5f;
    [SerializeField] bool _isTurningRight = true;
    [SerializeField] bool _isCooldowned = false;
    #endregion
    #region Unity Events
    [Header("Events")]
    [SerializeField] UnityEvent _onValueChange;
    #endregion
    #region Parameters
    [Header("Parameters")]
    [SerializeField] float _turnSpeed = 3f;
    [SerializeField] float _cooldownDuration = 1.5f;
    [SerializeField] Transform _valveTransform;
    #endregion


    public InteractableHoverResponse GetHoverResponse(IInteractor interactor)
    {
        if (isCooldowned)
        {
            return InteractableHoverResponse.None;
        }
        return InteractableHoverResponse.Enable;
    }
    public bool CanInteract(IInteractor interactor)
    {
        Player player = interactor as Player;

        if (!player) return false;

        return !isCooldowned;
    }
    public void OnInteract(IInteractor interactor)
    {
        value += _turnSpeed * (isTurningRight ? 1 : -1) * Time.deltaTime;
        _onValueChange?.Invoke();
        if (value == 1 || value == 0)
        {
            isCooldowned = true;
            StartCoroutine(ResetCooldown());
        }
    }

    private void LateUpdate()
    {
        _valveTransform.localEulerAngles = Vector3.left * 360 * value;
    }
    IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(_cooldownDuration);
        isCooldowned = false;
    }
}
