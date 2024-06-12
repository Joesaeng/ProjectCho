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
        _playerData = new Data.PlayerData();

        List<ItemData> tempEquipmentData = new();
        List<ItemData> tempInventoryData = new();

        EquipmentData tempEq = new();
        tempEq.itemName = "Staff";
        tempEq.itemSpriteName = "Staff_0";
        tempEq.isEquip = true;
        tempEq.equipSlotIndex = 0;
        tempEq.itemType = ItemType.Equipment;
        tempEq.equipmentType = EquipmentType.Weapon;
        tempEq.rarity = EquipmentRarity.Normal;
        tempEq.equipmentOptions = new()
        {
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.Spell,
                intParam1 = 0
            },
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.BaseDamage,
                floatParam1 = 5
            }
        };

        EquipmentData tempEq1 = new();
        tempEq1.itemName = "Ring0";
        tempEq1.itemSpriteName = "Ring_0";
        tempEq1.isEquip = true;
        tempEq1.equipSlotIndex = 0;
        tempEq1.itemType = ItemType.Equipment;
        tempEq1.equipmentType = EquipmentType.Ring;
        tempEq1.rarity = EquipmentRarity.Normal;
        tempEq1.equipmentOptions = new()
        {
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.IncreaseDamage,
                floatParam1 = 0.1f
            },

        };


        InventoryData inventoryData = new();

        tempEquipmentData.Add(tempEq);
        tempEquipmentData.Add(tempEq1);
        for (int i = 0; i < 30; ++i)
        {
            EquipmentData t = new();
            t.itemName = "Ring1";
            t.itemSpriteName = "Ring_1";
            t.itemType = ItemType.Equipment;
            t.equipmentType = EquipmentType.Ring;
            t.rarity = EquipmentRarity.Normal;
            t.equipmentOptions = new()
        {
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.IncreaseDamage,
                floatParam1 = 0.1f
            }
        };
            tempInventoryData.Add(t);
        }
        for (int i = 0; i < 25; ++i)
        {
            EquipmentData t = new();
            t.itemName = "Staff";
            t.itemSpriteName = "Staff_0";
            t.itemType = ItemType.Equipment;
            t.equipmentType = EquipmentType.Weapon;
            t.rarity = EquipmentRarity.Rare;
            t.equipmentOptions = new()
        {
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.Spell,
                intParam1 = 0
            },
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.BaseDamage,
                floatParam1 = 10
            },
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.IncreaseDamage,
                floatParam1 = 0.1f
            },
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.DecreaseSpellDelay,
                floatParam1 = 0.1f
            },
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.IncreaseDamage,
                floatParam1 = 0.1f
            },
        };
            tempInventoryData.Add(t);
        }

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
