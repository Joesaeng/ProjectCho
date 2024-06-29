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

    public void StartTimerUnscaled(float delay, Action callback)
    {
        StartCoroutine(TimerCoroutineUnscaled(delay, callback));
    }

    private IEnumerator TimerCoroutineUnscaled(float delay, Action callback)
    {
        yield return new WaitForSecondsRealtime(delay);
        callback?.Invoke();
    }

    private IEnumerator TimerCoroutine(float delay, Action callback)
    {
        yield return YieldCache.WaitForSeconds(delay);
        callback?.Invoke();
    }

    public void Clear()
    {
        StopAllCoroutines();
    }
}
