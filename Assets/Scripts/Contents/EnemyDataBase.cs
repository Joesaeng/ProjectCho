using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDataBase
{
    public Dictionary<int, SetEnemyData> EnemyDataDict { get; private set; } = new();

    public void Init()
    {
        var builder = new DataBuilder<int, BaseEnemyData, SetEnemyData>(data => new SetEnemyData(data));
        foreach (BaseEnemyData enemyData in Managers.Data.BaseEnemyDataDict.Values)
        {
            builder.AddData(enemyData.id, enemyData);
        }
        EnemyDataDict = builder.Build();
    }
}
