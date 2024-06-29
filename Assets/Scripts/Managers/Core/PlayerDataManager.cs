using Data;
using Define;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[Serializable]
public class InventoryData
{
    public List<EquipmentData> inventoryItemsDatas;
    public List<EquipmentData> equipmentDatas;
}

[Serializable]
public class PlayerOwnedSpellData
{
    public int spellId;
    public int spellLevel;
    public int ownCount;
    public bool isEquip;
}

[Serializable]
public class PlayerData
{
    public bool beginner = true;
    public PlayerSettingData settingData;
    public List<PlayerOwnedSpellData> ownedSpellDatas;
    public InventoryData inventoryData;
    public List<int> stageClearList;
    public List<AchievementData> achievementDatas;
    public int coinAmount = 0;
    public int diaAmount = 0;
}

[Serializable]
public class PlayerSettingData
{
    public GameLanguage language = GameLanguage.English;
    public bool bgmOn = true;
    public bool sfxOn = true;
    public int frameRate = ConstantData.FrameRate60;
}

public class PlayerDataManager
{
    PlayerData _playerData;

    #region GetProperty

    public GameLanguage GameLanguage { get => _playerData.settingData.language; set => _playerData.settingData.language = value; }

    public int CoinAmount => _playerData.coinAmount;
    public int DiaAmount => _playerData.diaAmount;
    public List<int> StageClearList => _playerData.stageClearList;
    public List<PlayerOwnedSpellData> OwnedSpellDatas => _playerData.ownedSpellDatas;
    public List<AchievementData> AchievementDatas => _playerData.achievementDatas;
    public InventoryData InventoryData => _playerData.inventoryData;

    public bool BgmOn { get => _playerData.settingData.bgmOn; set => _playerData.settingData.bgmOn = value; }
    public bool SfxOn { get => _playerData.settingData.sfxOn; set => _playerData.settingData.sfxOn = value; }

    public int FrameRate { get => _playerData.settingData.frameRate; set => _playerData.settingData.frameRate = value; }

    #endregion

    public Action<int> OnChangeCoinAmount;
    public Action<int> OnChangeDiaAmount;

    public PlayerData NewPlayerData() // 게임 최초 실행시의 데이터 설정
    {
        List<PlayerOwnedSpellData> startingSpellDatas = new()
        {
            new PlayerOwnedSpellData(){spellId = 0,isEquip = true,ownCount = 0, spellLevel = 0},
            new PlayerOwnedSpellData(){spellId = 1,isEquip = true,ownCount = 0, spellLevel = 0},
            new PlayerOwnedSpellData(){spellId = 2,isEquip = true,ownCount = 0, spellLevel = 0},
            new PlayerOwnedSpellData(){spellId = 3,isEquip = true,ownCount = 0, spellLevel = 0},
            new PlayerOwnedSpellData(){spellId = 4,isEquip = true,ownCount = 0, spellLevel = 0},
            new PlayerOwnedSpellData(){spellId = 5,isEquip = true,ownCount = 0, spellLevel = 0},
        };

        EquipmentData startingWeaponData = new()
        {
            itemName = "Staff",
            itemSpriteName = "Normal_Staff_0",
            isEquip = true,
            equipSlotIndex = 0,
            itemType = ItemType.Equipment,
            equipmentType = EquipmentType.Weapon,
            rarity = EquipmentRarity.Normal,
            equipmentOptions = new()
        {
            new EquipmentOptionData
            {
                optionType = StatusType.Spell,
                intParam1 = 0
            },
            new EquipmentOptionData
            {
                optionType = StatusType.BaseDamage,
                floatParam1 = 10
            }
        }
        };

        PlayerData newPlayerData = new();

        List<EquipmentData> newEquipmentData = new();
        List<EquipmentData> newInventoryData = new();

        newEquipmentData.Add(startingWeaponData);

        InventoryData inventoryData = new()
        {
            equipmentDatas = newEquipmentData,
            inventoryItemsDatas = newInventoryData
        };

        newPlayerData.ownedSpellDatas = startingSpellDatas;
        newPlayerData.inventoryData = inventoryData;
        newPlayerData.achievementDatas = new List<AchievementData>();
        newPlayerData.stageClearList = new();

        newPlayerData.coinAmount = 1000;
        newPlayerData.diaAmount = 1000;
        newPlayerData.settingData = new PlayerSettingData();

        return newPlayerData;
    }
    public void AddClearStage(int stageNum)
    {
        _playerData.stageClearList.Add(stageNum);
        SaveToFirebase();
    }

    private void ChangeCoins(int value)
    {
        _playerData.coinAmount = value;
        OnChangeCoinAmount?.Invoke(CoinAmount);
        SaveToFirebase();
    }

    public void IncreaseCoins(int value)
    {
        int coinAmount = CoinAmount + value;

        ChangeCoins(coinAmount);
    }

    public void DecreaseCoins(int value)
    {
        int coinAmount = CoinAmount - value;

        if (coinAmount < 0)
            Debug.LogWarning("Not enough Coins");
        ChangeCoins(coinAmount);
    }

    private void ChangeDia(int value)
    {
        _playerData.diaAmount = value;
        OnChangeDiaAmount?.Invoke(DiaAmount);
        SaveToFirebase();
    }

    public void IncreaseDia(int value)
    {
        int diaAmount = DiaAmount + value;

        ChangeDia(diaAmount);
    }

    public void DecreaseDia(int value)
    {
        int diaAmount = DiaAmount - value;

        if (diaAmount < 0)
            Debug.LogWarning("Not enough Coins");
        ChangeDia(diaAmount);
    }

    public bool HasEnoughCoins(int cost)
    {
        return CoinAmount >= cost;
    }

    public bool HasEnoughDia(int cost)
    {
        return DiaAmount >= cost;
    }

    public void NewPlayerLogin()
    {
        _playerData = NewPlayerData();
        Managers.InitManagersAfterLoadingPlayerData();
        SaveToFirebase();
    }

    public void SaveToFirebase()
    {
        _playerData.beginner = false;

        _playerData.inventoryData = Managers.Status.InventoryToData();
        _playerData.ownedSpellDatas = Managers.Status.SpellDataBaseToData();
        _playerData.achievementDatas = Managers.Achieve.ToData();

        string jsonData;
        try
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                // Converters = { new ItemDataConverter()},
            };

            jsonData = JsonConvert.SerializeObject(_playerData, settings);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to serialize PlayerData: " + ex.Message);
            return;
        }

        FirebaseManager.Instance.SavePlayerData(jsonData);
    }

    public void OnPlayerDataLoadedToFirebase(PlayerData playerData)
    {
        if (playerData != null)
        {
            // 플레이어 데이터를 성공적으로 로드한 경우 처리 로직을 추가합니다.
            Debug.Log("Player data loaded successfully.");
            _playerData = playerData;
            Managers.InitManagersAfterLoadingPlayerData();
            LoadedPlayerDataCollectionNullCheck();
            Managers.Scene.LoadSceneWithLoadingScene(Scene.Lobby);
        }
        else
        {
            // 데이터가 없는 경우 기본 데이터로 초기화할 수 있습니다.
            Debug.Log("No player data found, initializing with default data.");
            _playerData = NewPlayerData();
            Managers.InitManagersAfterLoadingPlayerData();
            SaveToFirebase();
            Managers.Scene.LoadSceneWithLoadingScene(Scene.Lobby);
        }
    }

    void LoadedPlayerDataCollectionNullCheck()
    {
        if (_playerData.ownedSpellDatas == null)
            _playerData.ownedSpellDatas = new();
        if (_playerData.inventoryData == null)
        {
            _playerData.inventoryData = new();
            _playerData.inventoryData.inventoryItemsDatas = new();
            _playerData.inventoryData.equipmentDatas = new();
        }
        else
        {
            if (_playerData.inventoryData.inventoryItemsDatas == null)
                _playerData.inventoryData.inventoryItemsDatas = new();
            if (_playerData.inventoryData.equipmentDatas == null)
                _playerData.inventoryData.equipmentDatas = new();
        }
        if (_playerData.stageClearList == null)
            _playerData.stageClearList = new();
        if (_playerData.achievementDatas == null)
            _playerData.achievementDatas = new();
        if (_playerData.settingData == null)
            _playerData.settingData = new();
    }

    //public void SaveToJson()
    //{
    //    if (File.Exists(_path))
    //        File.Delete(_path);
    //    _playerData.beginner = false;

    //    _playerData.inventoryData = Managers.Player.InventoryToData();
    //    _playerData.ownedSpellDatas = Managers.Player.SpellDataBaseToData();
    //    _playerData.achievementDatas = Managers.Achieve.ToData();

    //    JsonSerializerSettings settings = new()
    //    {
    //        TypeNameHandling = TypeNameHandling.All
    //    };

    //    string json = JsonConvert.SerializeObject(_playerData, settings);

    //    // byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
    //    // 
    //    // string encodedJson = System.Convert.ToBase64String(bytes);

    //    File.WriteAllText(_path, json);

    //}

    //public void LoadFromJson()
    //{
    //    if (!File.Exists(_path))
    //    {
    //        _playerData = NewPlayerData();
    //        return;
    //    }

    //    string jsonData = File.ReadAllText(_path);

    //    JsonSerializerSettings settings = new()
    //    {
    //        TypeNameHandling = TypeNameHandling.All
    //    };

    //    // byte[] bytes = System.Convert.FromBase64String(jsonData);
    //    // 
    //    // string decodedJson = System.Text.Encoding.UTF8.GetString(bytes);

    //    _playerData = JsonConvert.DeserializeObject<PlayerData>(jsonData, settings);
    //}

    public void Clear()
    {
        OnChangeCoinAmount = null;
        OnChangeDiaAmount = null;
    }
}
