using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimerManager : MonoBehaviour
{
    public void StartTimer(float delay, Action callback)
    {
        StartCoroutine(TimerCoroutine(delay, callback));
    }

    private IEnumerator TimerCoroutine(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
