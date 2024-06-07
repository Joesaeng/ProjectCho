using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<Item> items;
    public List<Item> Items { get => items; set => items = value; }
    public Inventory(Data.InventoryData data)
    {
        items = new();
        for(int i = 0; i < data.itemDatas.Count; ++i)
        {
            items.Add(ItemGenerator.GenerateItem(data.itemDatas[i]));
        }
    }

}
