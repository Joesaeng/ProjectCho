using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    TextMeshPro Text;
    Camera MainCamera;
    public void Init(int damage)
    {
        if(MainCamera == null)
            MainCamera = Camera.main;
        transform.localRotation = MainCamera.transform.localRotation;
        if(Text == null)
            Managers.CompCache.GetOrAddComponentCache(gameObject, out Text);
        Text.text = damage.ToString();
        StartCoroutine(CoDestroy());
    }

    IEnumerator CoDestroy()
    {
        yield return YieldCache.WaitForSeconds(1f);
        Managers.Resource.Destroy(gameObject);
    }
}
