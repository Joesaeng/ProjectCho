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

    public void SetEnemyStatusByStageData(int curStage, StageData data)
    {
        WaveData stageData = data.stageDatas[curStage];
        foreach (var enemyId in stageData.waveEnemyIds)
        {
            if (EnemyDataDict.TryGetValue(enemyId, out SetEnemyData setData))
            {
                setData.AttackDamage = Managers.Data.BaseEnemyDataDict[setData.Id].baseAttackDamage * stageData.damageCoefficient;
                setData.Hp = Managers.Data.BaseEnemyDataDict[setData.Id].baseHp * stageData.hpCoefficient;
            }
        }
    }
}
