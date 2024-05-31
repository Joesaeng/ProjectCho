using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEEffectDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/AOEEffectData.json";

    [Header("AOEEffectPrefab의 경로는 Resources/Prefabs/AOE 이어야 합니다")]
    public GameObject aOEEffectPrefab;
    public GameObject explosionPrefab;
    string aOEEffectName;
    string explosionName;

    public void WriteData()
    {
        aOEEffectName = aOEEffectPrefab.name;
        explosionName = explosionPrefab.name;
        AOEEffectData newData = new AOEEffectData()
        {
            effectName = aOEEffectName,
            explosionName = explosionName
        };
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}
