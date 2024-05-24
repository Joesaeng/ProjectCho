using Data;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager 
{
    // public Dictionary<int, Data.BaseUnit> BaseUnitDict { get; private set; } = new Dictionary<int, Data.BaseUnit>();

    public void Init()
    {
        // BaseUnitDict = LoadJson<Data.BaseUnitDatas, int, Data.BaseUnit>("BaseUnits").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key,Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<Loader>(textAsset.text);
    }
}
