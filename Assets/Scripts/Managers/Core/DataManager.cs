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

public class DataManager : MonoBehaviour
{
    public Dictionary<int, BaseEnemyData>       BaseEnemyDataDict       { get; private set; } = new Dictionary<int, BaseEnemyData>();
    public Dictionary<int, BaseSpellData>       BaseSpellDataDict       { get; private set; } = new Dictionary<int, BaseSpellData>();
    public Dictionary<int, ProjectileData>      ProjectileDataDict      { get; private set; } = new Dictionary<int, ProjectileData>();
    public Dictionary<int, AOEEffectData>       AOEEffectDataDict       { get; private set; } = new Dictionary<int, AOEEffectData>();
    public Dictionary<int, SpellUpgradeDatas>   UpgradeDataDict         { get; private set; } = new Dictionary<int, SpellUpgradeDatas>();
    public Dictionary<int, StageData>           StageDataDict           { get; private set; } = new Dictionary<int, StageData>();
    public Dictionary<int, EquipmentOptionData> EquipmentOptionDataDict { get; private set; } = new Dictionary<int, EquipmentOptionData>();
    public Dictionary<int, AchievementData>     AchievementDataDict     { get; private set; } = new Dictionary<int, AchievementData>();
    public Dictionary<string, LanguageData>     LanguageDataDict        { get; private set; } = new Dictionary<string, LanguageData>();

    public void Init()
    {
        BaseEnemyDataDict = LoadJson<Datas<BaseEnemyData>, int, Data.BaseEnemyData>("BaseEnemyData").MakeDict();
        BaseSpellDataDict = LoadJson<Datas<BaseSpellData>, int, Data.BaseSpellData>("BaseSpellData").MakeDict();
        ProjectileDataDict = LoadJson<Datas<ProjectileData>, int, Data.ProjectileData>("ProjectileData").MakeDict();
        AOEEffectDataDict = LoadJson<Datas<AOEEffectData>, int, Data.AOEEffectData>("AOEEffectData").MakeDict();
        UpgradeDataDict = LoadJson<Datas<SpellUpgradeDatas>, int, Data.SpellUpgradeDatas>("SpellUpgradeData").MakeDict();
        StageDataDict = LoadJson<Datas<StageData>, int, Data.StageData>("StageData").MakeDict();
        EquipmentOptionDataDict = LoadJson<Datas<EquipmentOptionData>, int, Data.EquipmentOptionData>("EquipmentOptionData").MakeDict();
        AchievementDataDict = LoadJson<Datas<AchievementData>, int, Data.AchievementData>("AchievementData").MakeDict();
        LanguageDataDict = LoadJson<LanguageDatas, string, Data.LanguageData>("LanguageData").MakeDict();

        
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Resources.Load<TextAsset>($"Data/{path}");
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load JSON file at path: Data/{path}");
            return default;
        }

        JsonSerializerSettings settings = new()
        {
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        return JsonConvert.DeserializeObject<Loader>(textAsset.text, settings);
    }

    #region 구글시트 데이터 로드
    // StartCoroutine(LoadAllData(onComplete));

    private IEnumerator LoadAllData(Action onComplete)
    {
        List<IEnumerator> coroutines = new List<IEnumerator>
        {
            LoadGoogleSheetData<Datas<BaseEnemyData>, int, Data.BaseEnemyData>("BaseEnemyData", loader =>
                BaseEnemyDataDict = loader.MakeDict()),
            LoadGoogleSheetData<Datas<BaseSpellData>, int, Data.BaseSpellData>("BaseSpellData", loader =>
                BaseSpellDataDict = loader.MakeDict()),
            LoadGoogleSheetData<Datas<ProjectileData>, int, Data.ProjectileData>("ProjectileData", loader =>
                ProjectileDataDict = loader.MakeDict()),
            LoadGoogleSheetData<Datas<AOEEffectData>, int, Data.AOEEffectData>("AOEEffectData", loader =>
                AOEEffectDataDict = loader.MakeDict()),
            LoadGoogleSheetData<Datas<SpellUpgradeDatas>, int, Data.SpellUpgradeDatas>("SpellUpgradeData", loader =>
                UpgradeDataDict = loader.MakeDict()),
            LoadGoogleSheetData<Datas<StageData>, int, Data.StageData>("StageData", loader =>
                StageDataDict = loader.MakeDict()),
            LoadGoogleSheetData<Datas<EquipmentOptionData>, int, Data.EquipmentOptionData>("EquipmentOptionData", loader =>
                EquipmentOptionDataDict = loader.MakeDict()),
            LoadGoogleSheetData<Datas<AchievementData>, int, Data.AchievementData>("AchievementData", loader =>
                AchievementDataDict = loader.MakeDict()),
            LoadGoogleSheetData<LanguageDatas, string, Data.LanguageData>("LanguageData", loader =>
                LanguageDataDict = loader.MakeDict())
        };

        foreach (var coroutine in coroutines)
        {
            yield return StartCoroutine(coroutine);
        }

        onComplete?.Invoke();
    }

    

    Loader LoadGoogleSheetData<Loader, Key, Value>(string sheetName) where Loader : ILoader<Key, Value>
    {
        string json = ConvertData.ConvertSheetDataToJson(sheetName);
        JsonSerializerSettings settings = new()
        {
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };
        Debug.Log($"{sheetName}.loaded");
        return JsonConvert.DeserializeObject<Loader>(json, settings);
    }

    private IEnumerator LoadGoogleSheetData<Loader, Key, Value>(string sheetName, Action<Loader> onLoadComplete) where Loader : ILoader<Key, Value>
    {
        yield return ConvertData.ConvertSheetDataToJson(sheetName, (json) =>
        {
            if (json != null)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new StringEnumConverter() }
                };
                Debug.Log($"{sheetName} loaded");
                var loader = JsonConvert.DeserializeObject<Loader>(json, settings);
                onLoadComplete.Invoke(loader);
            }
            else
            {
                Debug.LogError($"Failed to load data for sheet: {sheetName}");
            }
        });
    }
    #endregion
}
