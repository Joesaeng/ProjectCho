using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/BaseEnemyData.json";

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
        BaseEnemyData newData = new BaseEnemyData()
        {
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
