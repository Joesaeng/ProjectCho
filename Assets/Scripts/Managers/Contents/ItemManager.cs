using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ItemManager
{
    public Dictionary<EquipmentRarity, List<Sprite>> RingIcons = new();
    public Dictionary<EquipmentRarity, List<Sprite>> WeaponIcons = new();

    public List<EquipmentOptionData> equipmentOptionDatas = new();

    public Dictionary<EquipmentRarity, List<EquipmentOptionData>> equipmentOptionsByRarity = new();

    public void Init()
    {
        equipmentOptionDatas = Managers.Data.EquipmentOptionDataDict.Values.ToList();

        foreach (var data in equipmentOptionDatas)
        {
            if (!equipmentOptionsByRarity.ContainsKey(data.requireRarity))
            {
                equipmentOptionsByRarity[data.requireRarity] = new List<EquipmentOptionData>();
            }
            equipmentOptionsByRarity[data.requireRarity].Add(data);
        }

        string ringPath = "Textures/ItemIcons/RingIcon";
        string weaponPath = "Textures/ItemIcons/WeaponIcon";

        LoadIcons(ringPath, RingIcons);
        LoadIcons(weaponPath, WeaponIcons);
    }

    public List<Item> SummonItems(EquipmentType type,int count = 10)
    {
        // Player의 Coin, Dia 보유량 확인
        List<Item> summonItems = new List<Item>();
        for(int i = 0; i < count; i++)
        {
            Item newItem = GenerateRandomItem(type);
            summonItems.Add(newItem);
        }
        Managers.Player.AddItems(summonItems);
        Managers.Achieve.SetAchievementValueByTargetType(AchievementTargetType.Summon, count, summonType: type);
        return summonItems;
    }

    private void LoadIcons(string path, Dictionary<EquipmentRarity, List<Sprite>> iconsDict)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);

        foreach (Sprite sprite in sprites)
        {
            string spriteName = sprite.name;
            string[] parts = spriteName.Split('_');

            if (parts.Length == 3)
            {
                if (System.Enum.TryParse(parts[0], out EquipmentRarity rarity))
                {
                    if (!iconsDict.ContainsKey(rarity))
                    {
                        iconsDict[rarity] = new List<Sprite>();
                    }
                    iconsDict[rarity].Add(sprite);
                }
                else
                {
                    Debug.LogWarning($"Unknown rarity: {parts[0]} in sprite name: {spriteName}");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid sprite name format: {spriteName}");
            }
        }
    }

    public Equipment GenerateRandomItem(EquipmentType equipmentType)
    {
        // 장비의 희귀도를 정한다.
        EquipmentData equipmentData = new EquipmentData();
        equipmentData.itemType = ItemType.Equipment;
        equipmentData.equipmentType = equipmentType;
        equipmentData.equipmentOptions = new();
        // normal : 65, Rare : 25,Epic : 7, Legend : 3
        int rarityInt = Random.Range(0,101);
        EquipmentRarity rarity = EquipmentRarity.Normal;
        if (rarityInt <= 3)
            rarity = EquipmentRarity.Legend;
        else if (rarityInt <= 10)
            rarity = EquipmentRarity.Epic;
        else if (rarityInt <= 32)
            rarity = EquipmentRarity.Rare;

        equipmentData.rarity = rarity;

        List<EquipmentOptionData> validOptions = GetValidOptions(equipmentType, rarity);

        if (validOptions.Count == 0)
        {
            Debug.LogWarning($"No valid options for rarity: {rarity} and type: {equipmentType}");
            return null;
        }

        // 무기의 기본 옵션을 정하는 부분(Spell, BaseDamage)
        if (equipmentType == EquipmentType.Weapon)
        {
            List<EquipmentOptionData> possibleSpells = new();
            EquipmentOptionData baseDamage = null;
            foreach (var option in validOptions)
            {
                if (option.optionType == StatusType.Spell)
                {
                    possibleSpells.Add(option);
                    continue;
                }
                if (option.optionType == StatusType.BaseDamage && option.requireRarity == rarity)
                    baseDamage = option;
            }
            if (possibleSpells.Count == 0 || baseDamage == null)
            {
                Debug.LogError("RandomGenerate Weapon Failed");
                return null;
            }
            equipmentData.equipmentOptions.Add(possibleSpells[Random.Range(0, possibleSpells.Count)]);
            equipmentData.equipmentOptions.Add(baseDamage);
        }

        int optionCount = (int)rarity;
        if (equipmentType == EquipmentType.Ring)
            optionCount += 1;

        for (int i = 0; i < optionCount; ++i)
        {
            EquipmentOptionData option = GetRandomWeightedOption(validOptions);
            equipmentData.equipmentOptions.Add(option);
        }

        EquipmentOptionData lowestWeightOption = equipmentData.equipmentOptions.Find(opt => opt.weight > 0);

        if (lowestWeightOption != null)
            foreach (var option in equipmentData.equipmentOptions)
            {
                if (option.weight > 0 && option.weight < lowestWeightOption.weight)
                {
                    lowestWeightOption = option;
                }
            }

        equipmentData.itemName = GenerateItemName(equipmentType, lowestWeightOption);

        Equipment equipment = ItemGenerator.GenerateItem(equipmentData) as Equipment;
        equipment.ItemIcon = 
            equipmentType == EquipmentType.Weapon ? WeaponIcons[rarity][Random.Range(0, WeaponIcons[rarity].Count)] :
            RingIcons[rarity][Random.Range(0, RingIcons[rarity].Count)];

        equipment.ItemSpriteName = equipment.ItemIcon.name;

        return equipment;
    }

    List<EquipmentOptionData> GetValidOptions(EquipmentType equipmentType, EquipmentRarity rarity)
    {
        List<EquipmentOptionData> validOptions = new List<EquipmentOptionData>();

        foreach (var kvp in equipmentOptionsByRarity)
        {
            if (kvp.Key <= rarity)
            {
                foreach (var option in kvp.Value)
                {
                    if (option.capableOfEquipmentType.Contains(equipmentType))
                    {
                        validOptions.Add(option);
                    }
                }
            }
        }

        return validOptions;
    }

    private EquipmentOptionData GetRandomWeightedOption(List<EquipmentOptionData> options)
    {
        int totalWeight = 0;
        foreach (var option in options)
        {
            totalWeight += option.weight;
        }

        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var option in options)
        {
            if (option.weight == 0)
                continue;

            currentWeight += option.weight;
            if (randomWeight < currentWeight)
            {
                return option;
            }
        }

        return null; // 이론적으로는 여기 도달하지 않아야 함
    }

    private string GenerateItemName(EquipmentType type, EquipmentOptionData lowestWeightOption)
    {
        string itemName = "";
        if (lowestWeightOption != null)
            itemName += lowestWeightOption.prefix + " ";

        itemName += type == EquipmentType.Weapon ? "Staff" : "Ring";
        return itemName;
    }
}
