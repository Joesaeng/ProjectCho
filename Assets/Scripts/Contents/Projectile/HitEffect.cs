using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    public void Init()
    {
        StartCoroutine(CoDestroy());
    }

    IEnumerator CoDestroy()
    {
        yield return YieldCache.WaitForSeconds(1f);
        Managers.Resource.Destroy(gameObject);
    }
}
