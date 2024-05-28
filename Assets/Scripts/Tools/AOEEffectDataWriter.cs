using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEEffectDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/AOEEffectData.json";

    [Header("AOEEffectPrefab의 경로는 Resources/Prefabs/AOE 이어야 합니다")]
    public GameObject aOEEffectPrefab;
    string aOEEffectName;

    public void WriteData()
    {
        aOEEffectName = aOEEffectPrefab.name;
        AOEEffectData newData = new AOEEffectData()
        {
            effectName = aOEEffectName
        };
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}
