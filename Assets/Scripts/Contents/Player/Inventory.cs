using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory
{
    //private HashSet<Item> items = new();
    //public HashSet<Item> Items { get => items; set => items = value; }

    private List<Equipment> items = new();
    public List<Equipment> Items { get => items; set => items = value; }

    public void Init(InventoryData data)
    {
        if (data.inventoryItemsDatas != null)
            for (int i = 0; i < data.inventoryItemsDatas.Count; ++i)
            {
                items.Add(ItemGenerator.GenerateItem(data.inventoryItemsDatas[i]));
            }
    }

    //public void AddItem(Item item)
    //{
    //    items.Add(item);
    //    Managers.Player.ChangeInventory();
    //}

    //public void AddItems(List<Item> items)
    //{
    //    Items.AddRange(items);
    //    Managers.Player.ChangeInventory();
    //}

    //public void RemoveItem(Item item)
    //{
    //    items.Remove(item);
    //    Managers.Player.ChangeInventory();
    //}

    public void AddItem(Equipment item)
    {
        items.Add(item);
        SortItemsByRarity();
        Managers.Status.ChangeInventory();
    }

    public void AddItems(List<Equipment> items)
    {
        Items.AddRange(items);
        SortItemsByRarity();
        Managers.Status.ChangeInventory();
    }

    public void RemoveItem(Equipment item)
    {
        items.Remove(item);
        Managers.Status.ChangeInventory();
    }

    public void SortItemsByRarity()
    {
        items.Sort((item1, item2) => item2.rarity.CompareTo(item1.rarity));
    }
}
