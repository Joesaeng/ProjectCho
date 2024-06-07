using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item
{
    private string itemName;
    public string ItemName { get => itemName; set => itemName = value; }
    public abstract void ApplyItem();
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
            Debug.Log($"{i}번 옵션");
            equipmentOptions.Add(new EquipmentOption(data.equipmentOptions[i]));
        }
    }
    public override void ApplyItem()
    {
        Managers.Status.EquipmentInventory.Equip(this);
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
}
