using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Data;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager 
{
    public Dictionary<int, BaseEnemyData>   BaseEnemyDataDict { get; private set; } = new Dictionary<int, BaseEnemyData>();
    public Dictionary<int, BaseSpellData>   BaseSpellDataDict { get; private set; } = new Dictionary<int, BaseSpellData>();
    public Dictionary<int, ProjectileData>  ProjectileDataDict { get; private set; } = new Dictionary<int, ProjectileData>();
    public Dictionary<int, AOEEffectData>   AOEEffectDataDict { get; private set; } = new Dictionary<int, AOEEffectData>();

    public void Init()
    {
        BaseEnemyDataDict = LoadJson<Datas<BaseEnemyData>, int, Data.BaseEnemyData>("BaseEnemyData").MakeDict();
        BaseSpellDataDict = LoadJson<Datas<BaseSpellData>, int, Data.BaseSpellData>("BaseSpellData").MakeDict();
        ProjectileDataDict = LoadJson<Datas<ProjectileData>, int, Data.ProjectileData>("ProjectileData").MakeDict();
        AOEEffectDataDict = LoadJson<Datas<AOEEffectData>, int, Data.AOEEffectData>("AOEEffectData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Resources.Load<TextAsset>($"Data/{path}");
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load JSON file at path: Data/{path}");
            return default(Loader);
        }

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        return JsonConvert.DeserializeObject<Loader>(textAsset.text, settings);
    }
}
