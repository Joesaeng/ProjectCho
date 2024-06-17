using Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerDataManager
{
    PlayerData _playerData;
    public PlayerData Data
    {
        get
        {
            return _playerData;
        }
        set { _playerData = value; }
    }

    string _path;

    public void Init()
    {
        _path = Application.persistentDataPath + "/PlayerData";
        LoadFromJson();
    }

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

        int weaponSpellId = startingWeaponData.equipmentOptions
            .Where(data => data.optionType == StatusType.Spell)
            .Select(data => data.intParam1)
            .FirstOrDefault();

        newPlayerData.weaponSpellId = weaponSpellId;
        newPlayerData.ownedSpellDatas = startingSpellDatas;
        newPlayerData.inventoryData = inventoryData;
        newPlayerData.achievementDatas = new List<AchievementData>();
        newPlayerData.stageClearList = new();

        return newPlayerData;
    }

    // 플레이어 데이터를 UTF-8로 인코딩하여 저장합니다
    public void SaveToJson()
    {
        if (File.Exists(_path))
            File.Delete(_path);

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
}
