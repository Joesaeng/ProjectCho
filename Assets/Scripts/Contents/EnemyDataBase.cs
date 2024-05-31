using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDataBase
{
    public Dictionary<int, SetEnemyData> EnemyDataDict { get; } = new();

    public void Init()
    {
        foreach (BaseEnemyData enemyData in Managers.Data.BaseEnemyDataDict.Values)
        {
            EnemyDataDict.Add(enemyData.id, new SetEnemyData(enemyData));
        }
    }
}
