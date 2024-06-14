using Data;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerDataManager
{
    Data.PlayerData _playerData;
    public Data.PlayerData Data
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
        // _path = Application.persistentDataPath + "/PlayerData";
        // LoadFromJson();

        // TempCode

        

        EquipmentData startingWeaponData = new();
        startingWeaponData.itemName = "Staff";
        startingWeaponData.itemSpriteName = "Normal_Staff_0";
        startingWeaponData.isEquip = true;
        startingWeaponData.equipSlotIndex = 0;
        startingWeaponData.itemType = ItemType.Equipment;
        startingWeaponData.equipmentType = EquipmentType.Weapon;
        startingWeaponData.rarity = EquipmentRarity.Normal;
        startingWeaponData.equipmentOptions = new()
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
        };

        _playerData = new Data.PlayerData();

        List<ItemData> tempEquipmentData = new();
        List<ItemData> tempInventoryData = new();
        tempEquipmentData.Add(startingWeaponData);

        InventoryData inventoryData = new();

        inventoryData.equipmentDatas = tempEquipmentData;
        inventoryData.inventoryItemsDatas = tempInventoryData;

        _playerData.inventoryData = inventoryData;
    }

    // 플레이어 데이터를 UTF-8로 인코딩하여 저장합니다
    public void SaveToJson()
    {
        if (File.Exists(_path))
            File.Delete(_path);

        _playerData.inventoryData = Managers.Player.InventoryToData();

        string json = JsonUtility.ToJson(_playerData);

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

        string encodedJson = System.Convert.ToBase64String(bytes);

        File.WriteAllText(_path, encodedJson);

    }

    // UTF-8로 인코딩된 데이터를 디코딩하여 불러옵니다
    public void LoadFromJson()
    {
        if (!File.Exists(_path))
        {
            _playerData = new Data.PlayerData();

            SaveToJson();
        }

        string jsonData = File.ReadAllText(_path);

        byte[] bytes = System.Convert.FromBase64String(jsonData);

        string decodedJson = System.Text.Encoding.UTF8.GetString(bytes);

        _playerData = JsonUtility.FromJson<Data.PlayerData>(decodedJson);
    }
}
