using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory
{
    private HashSet<Item> items;
    public HashSet<Item> Items { get => items; set => items = value; }
    public Inventory(Data.InventoryData data)
    {
        items = new();
        for(int i = 0; i < data.inventoryItemsDatas.Count; ++i)
        {
            items.Add(ItemGenerator.GenerateItem(data.inventoryItemsDatas[i]));
        }
    }

    public void AddItem(Item item)
    {
        items.Add(item);
        Managers.Player.ChangeInventory();
    }

    public void AddItems(List<Item> items)
    {
        Items.AddRange(items);
        Managers.Player.ChangeInventory();
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
        Managers.Player.ChangeInventory();
    }
}
