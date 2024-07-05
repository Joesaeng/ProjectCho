using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LoadingRotation : MonoBehaviour
{
    RectTransform _rectUI;
    Vector3 _rotateVector = new Vector3(0f, 0f, -200f);
    private void Awake()
    {
        _rectUI = GetComponent<RectTransform>();
    }
    void Update()
    {
        _rectUI.Rotate(_rotateVector * Time.unscaledDeltaTime);
    }
}
