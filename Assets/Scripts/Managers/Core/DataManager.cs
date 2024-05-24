using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Data;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager 
{
    public Dictionary<int, BaseEnemyData> BaseEnemyDataDict { get; private set; } = new Dictionary<int, BaseEnemyData>();

    public void Init()
    {
        BaseEnemyDataDict = LoadJson<Datas<BaseEnemyData>, int, Data.BaseEnemyData>("BaseEnemyData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key,Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<Loader>(textAsset.text);
    }
}
