using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    TextMeshPro Text;
    public void Init(int damage)
    {
        transform.localRotation = Quaternion.Euler(60, 0, 0);
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
