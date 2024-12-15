using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 0.1f;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutCo());
    }

    private IEnumerator FadeOutCo()
    {
        var newColor = _image.color;
        
        while (_image.color.a < 1f)
        {
            newColor.a += fadeSpeed * Time.deltaTime;
            _image.color = newColor;
            yield return null;
        }
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInCo());
    }
    
    private IEnumerator FadeInCo()
    {
        var newColor = _image.color;

        while (_image.color.a > 0f)
        {
            newColor.a -= fadeSpeed * Time.deltaTime;
            _image.color = newColor;
            yield return null;
        }
    }
}
