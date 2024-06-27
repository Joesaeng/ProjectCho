using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemGenerator
{
    public static Equipment GenerateItem(EquipmentData data)
    {
        Equipment item = NewItem(data);

        return item;
    }

    static Equipment NewItem(EquipmentData data)
    {
        return new Equipment(data);
    }

    public static EquipmentData ToData(Equipment item)
    {
        return item.ToData();
    }
}
