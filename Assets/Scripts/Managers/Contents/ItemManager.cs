using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RarityWeight : IRandomWeighted
{
    public EquipmentRarity rarity;
    public int weight;

    public int Weight => weight;
}

public class ItemManager
{
    public Dictionary<EquipmentRarity, List<Sprite>> RingIcons = new();
    public Dictionary<EquipmentRarity, List<Sprite>> WeaponIcons = new();

    public List<EquipmentOptionData> equipmentOptionDatas = new();

    public Dictionary<EquipmentRarity, List<EquipmentOptionData>> equipmentOptionsByRarity = new();

    private List<RarityWeight> rarityWeights = new List<RarityWeight>
    {
        new RarityWeight { rarity = EquipmentRarity.Normal, weight = 650 },
        new RarityWeight { rarity = EquipmentRarity.Rare, weight = 250 },
        new RarityWeight { rarity = EquipmentRarity.Epic, weight = 68 },
        new RarityWeight { rarity = EquipmentRarity.Legend, weight = 32 }
    };

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

        EquipmentRarity rarity = Util.GetRandomWeightedSelect<RarityWeight>(rarityWeights).rarity;
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
            EquipmentOptionData option = Util.GetRandomWeightedSelect(validOptions);
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

    private string GenerateItemName(EquipmentType type, EquipmentOptionData lowestWeightOption)
    {
        string itemName = "";
        if (lowestWeightOption != null)
            itemName += lowestWeightOption.prefix + " ";

        itemName += type == EquipmentType.Weapon ? "Staff" : "Ring";
        return itemName;
    }
}
