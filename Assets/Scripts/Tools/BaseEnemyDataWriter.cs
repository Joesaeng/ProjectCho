using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/BaseEnemyData.json";

    public GameObject prefab;
    string prefabName;
    public ElementType elementType;
    public float baseHp;
    public float baseMoveSpeed;
    public float baseAttackDamage;
    public float baseAttackDelay;
    public float baseAttackRange;
    public bool isRange;
    public int projectileId;

    public void WriteData()
    {
        prefabName = prefab.name;
        BaseEnemyData newData = new BaseEnemyData()
        {
            prefabName = prefabName,
            elementType = elementType,
            baseHp = baseHp,
            baseMoveSpeed = baseMoveSpeed,
            baseAttackDamage = baseAttackDamage,
            baseAttackDelay = baseAttackDelay,
            baseAttackRange = baseAttackRange,
            isRange = isRange,
            projectileId = projectileId
        };
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}
