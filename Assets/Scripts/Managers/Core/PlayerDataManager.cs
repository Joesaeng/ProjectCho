using Data;
using Define;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class InventoryData
{
    public List<ItemData> inventoryItemsDatas;
    public List<ItemData> equipmentDatas;
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
    public GameLanguage gameLanguage = GameLanguage.Korean;
    public bool bgmOn = true;
    public bool sfxOn = true;
    public List<PlayerOwnedSpellData> ownedSpellDatas;
    public InventoryData inventoryData;
    public List<int> stageClearList;
    public List<AchievementData> achievementDatas;
    public int coinAmount = 0;
    public int diaAmount = 0;
}

public class PlayerDataManager
{
    PlayerData _playerData;

    #region GetProperty

    public GameLanguage GameLanguage => _playerData.gameLanguage;

    public int CoinAmount => _playerData.coinAmount;
    public int DiaAmount => _playerData.diaAmount;
    public List<int> StageClearList => _playerData.stageClearList;
    public List<PlayerOwnedSpellData> OwnedSpellDatas => _playerData.ownedSpellDatas;
    public List<AchievementData> AchievementDatas => _playerData.achievementDatas;
    public InventoryData InventoryData => _playerData.inventoryData;

    public bool BgmOn => _playerData.bgmOn;
    public bool SfxOn => _playerData.sfxOn;

    #endregion
    string _path;

    public void Init()
    {
        _path = Application.persistentDataPath + "/PlayerData";
        LoadFromJson();
    }

    public Action<int> OnChangeCoinAmount;
    public Action<int> OnChangeDiaAmount;

    public PlayerData NewPlayerData() // 게임 최초 실행시의 데이터 설정
    {
        List<PlayerOwnedSpellData> startingSpellDatas = new()
        {
            new PlayerOwnedSpellData(){spellId = 0,isEquip = true},
            new PlayerOwnedSpellData(){spellId = 1,isEquip = true},
            new PlayerOwnedSpellData(){spellId = 2,isEquip = true},
            new PlayerOwnedSpellData(){spellId = 3,isEquip = true},
            new PlayerOwnedSpellData(){spellId = 4,isEquip = true},
            new PlayerOwnedSpellData(){spellId = 5,isEquip = true},
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
                intParam1 = 5
            },
            new EquipmentOptionData
            {
                optionType = StatusType.BaseDamage,
                floatParam1 = 10
            }
        }
        };

        PlayerData newPlayerData = new();

        List<ItemData> newEquipmentData = new();
        List<ItemData> newInventoryData = new();
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

        return newPlayerData;
    }
    public void AddClearStage(int stageNum)
    {
        _playerData.stageClearList.Add(stageNum);
    }

    public void ChangeCoinAmount(int value)
    {
        _playerData.coinAmount = value;
        OnChangeCoinAmount?.Invoke(_playerData.coinAmount);
    }

    public void ChangeDiaAmount(int value)
    {
        _playerData.diaAmount = value;
        OnChangeDiaAmount?.Invoke(_playerData.diaAmount);
    }
    // 플레이어 데이터를 UTF-8로 인코딩하여 저장합니다
    public void SaveToJson()
    {
        if (File.Exists(_path))
            File.Delete(_path);
        _playerData.beginner = false;

        _playerData.inventoryData = Managers.Player.InventoryToData();
        _playerData.ownedSpellDatas = Managers.Player.SpellDataBaseToData();

        JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        string json = JsonConvert.SerializeObject(_playerData, settings);

        // byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        // 
        // string encodedJson = System.Convert.ToBase64String(bytes);

        File.WriteAllText(_path, json);

    }

    // UTF-8로 인코딩된 데이터를 디코딩하여 불러옵니다
    public void LoadFromJson()
    {
        if (!File.Exists(_path))
        {
            _playerData = NewPlayerData();
            Managers.Player.Init();
            SaveToJson();
        }

        string jsonData = File.ReadAllText(_path);

        JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        // byte[] bytes = System.Convert.FromBase64String(jsonData);
        // 
        // string decodedJson = System.Text.Encoding.UTF8.GetString(bytes);

        _playerData = JsonConvert.DeserializeObject<PlayerData>(jsonData, settings);
    }

    public void Clear()
    {
        OnChangeCoinAmount = null;
        OnChangeDiaAmount = null;
    }
}
