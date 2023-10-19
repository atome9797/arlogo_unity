using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenShotFlash : MonoBehaviour
{
    public float duration = 0.3f;

    [SerializeField]
    Image _image;
    private float _currentAlpha = 1f;

    private void Update()
    {
        if(_currentAlpha > 0f)
        {
            Color col = _image.color;
            col.a = _currentAlpha;
            _image.color = col;

            _currentAlpha -= Time.deltaTime / duration;
        }
        else
        {
            _image.gameObject.SetActive(false);
        }
    }

    public void Show()
    {
        _currentAlpha = 1f;
        _image.gameObject.SetActive(true);
    }
}
