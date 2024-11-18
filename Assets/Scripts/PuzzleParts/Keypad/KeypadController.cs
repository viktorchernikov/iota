using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeypadController : MonoBehaviour
{
    #region Events
    //public event Action onActive;
    //public event Action onDeactive;
    #endregion

    #region Editor State
    [Header("State")]
    [SerializeField] string password;
    [SerializeField] Text passwordText;
    #endregion
    #region Delays
    [Header("Delays")]
    //[SerializeField] float _switchAnimationDelay = 0.4f;
    [SerializeField] float _pressDelay = 0.75f;
    #endregion
    #region Unity Events
    //[Header("Events")]
    //[SerializeField] UnityEvent _onActiveEvent;
    //[SerializeField] UnityEvent _onDeactivateEvent;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        passwordText.text = "";
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
            Enter();
            return;
        }

        passwordText.text += number;
    }

    public void Clear()
    {
        passwordText.text = "";
        passwordText.color = Color.white;
    }

    private void Enter()
    {
        if (passwordText.text == password)
        {
            //door.lockedByPassword = false;

            //if (audioSource != null)
            //    audioSource.PlayOneShot(correctSound);

            passwordText.color = Color.green;
            StartCoroutine(WaitAndClear());
        }
        else
        {
            //if (audioSource != null)
            //    audioSource.PlayOneShot(wrongSound);

            passwordText.color = Color.red;
            StartCoroutine(WaitAndClear());
        }
    }

    IEnumerator WaitAndClear()
    {
        yield return new WaitForSeconds(_pressDelay);
        Clear();
    }
}
