using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileExplosion : MonoBehaviour
{
    ParticleSystem ParticleSystem { get; set; }
    bool IsLive { get; set; }

    public void Init()
    {
        IsLive = true;
        if(ParticleSystem == null)
            ParticleSystem = GetComponent<ParticleSystem>();
        StartCoroutine(CoStoppedDestroy());
    }

    IEnumerator CoStoppedDestroy()
    {
        while(true)
        {
            yield return null;
            if (ParticleSystem.isStopped)
                Managers.Resource.Destroy(gameObject);
        }
    }
}
