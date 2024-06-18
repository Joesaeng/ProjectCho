using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
