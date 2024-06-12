using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Item
{
    private string itemName;
    private string itemSpriteName;
    public string ItemName { get => itemName; set => itemName = value; }
    public string ItemSpriteName { get => itemSpriteName; set => itemSpriteName = value; }

    public abstract void ApplyItem();
    public abstract ItemData ToData();
}

public class EquipmentOption
{
    private EquipmentOptionType optionType;
    private int intParam1;
    private float floatParam1;

    public EquipmentOptionType OptionType { get => optionType; set => optionType = value; }
    public int IntParam1 { get => intParam1; set => intParam1 = value; }
    public float FloatParam1 { get => floatParam1; set => this.floatParam1 = value; }

    public EquipmentOption(EquipmentsOptionData data)
    {
        optionType = data.optionType;
        intParam1 = data.intParam1;
        floatParam1 = data.floatParam1;
    }

    public EquipmentsOptionData ToData()
    {
        return new EquipmentsOptionData
        {
            optionType = this.optionType,
            intParam1 = this.intParam1,
            floatParam1 = this.floatParam1,
        };
    }
}

public class Equipment : Item
{
    public EquipmentRarity rarity;
    public EquipmentType equipmentType;
    public List<EquipmentOption> equipmentOptions;
    public bool isEquip;
    public int equipSlotIndex;

    public Equipment(EquipmentData data)
    {
        ItemName = data.itemName;
        ItemSpriteName = data.itemSpriteName;
        rarity = data.rarity;
        equipmentType = data.equipmentType;
        isEquip = data.isEquip;
        equipSlotIndex = data.equipSlotIndex;
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
            itemSpriteName = ItemSpriteName,
            rarity = rarity,
            equipmentType = equipmentType,
            isEquip = isEquip,
            equipSlotIndex = equipSlotIndex,
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
