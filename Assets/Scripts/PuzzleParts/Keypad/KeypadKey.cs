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
    [SerializeField] Text key;
    #endregion

    public void SendKey()
    {
        transform.GetComponentInParent<KeypadController>().PasswordEntry(sKey);
    }
}