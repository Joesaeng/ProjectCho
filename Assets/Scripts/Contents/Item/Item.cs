using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Item
{
    private string itemName;
    public string ItemName { get => itemName; set => itemName = value; }
    public abstract void ApplyItem();
    public abstract ItemData ToData();
}

public class EquipmentOption
{
    private EquipmentOptionType optionType;
    private float value;
    private int spellId;

    public EquipmentOptionType OptionType { get => optionType; set => optionType = value; }
    public float Value { get => value; set => this.value = value; }
    public int SpellId { get => spellId; set => spellId = value; }

    public EquipmentOption(EquipmentsOptionData data)
    {
        optionType = data.optionType;
        value = data.value;
        spellId = data.spellId;
    }

    public EquipmentsOptionData ToData()
    {
        return new EquipmentsOptionData
        {
            optionType = this.optionType,
            value = this.value,
            spellId = this.spellId,
        };
    }
}

public class Equipment : Item
{
    public EquipmentRarity rarity;
    public EquipmentType equipmentType;
    public List<EquipmentOption> equipmentOptions;

    public Equipment(EquipmentData data)
    {
        ItemName = data.itemName;
        rarity = data.rarity;
        equipmentType = data.equipmentType;
        equipmentOptions = new();
        for(int i = 0; i < data.equipmentOptions.Count; ++i)
        {
            equipmentOptions.Add(new EquipmentOption(data.equipmentOptions[i]));
        }
    }
    public override void ApplyItem()
    {
        Managers.Player.Equip(this);
    }

    public override ItemData ToData()
    {
        return new EquipmentData
        {
            itemName = ItemName,
            rarity = rarity,
            equipmentType = equipmentType,
            itemType = ItemType.Equipment,
            equipmentOptions = equipmentOptions.Select(option => option.ToData()).ToList()
        };
    }
}

public static class ItemGenerator
{
    public static Item GenerateItem(ItemData data)
    {
        Item item = NewItem(data);

        return item;
    }

    static Item NewItem(ItemData data)
    {
        return data.itemType switch
        {
            ItemType.Equipment => new Equipment(data as EquipmentData),
            _ => throw new System.ArgumentException($"Unknown ItemType: {data.itemType}")
        };
    }

    public static ItemData ToData(Item item)
    {
        return item.ToData();
    }
}
