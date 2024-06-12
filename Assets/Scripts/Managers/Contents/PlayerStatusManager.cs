using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatus
{
    public int startingSpellId;
    public float damage;
    public Dictionary<EquipmentOptionType,float> floatOptions = new();
    public Dictionary<EquipmentOptionType,int> integerOptions = new();
}
public class PlayerStatusManager
{
    PlayerStatus playerStatus;
    Inventory inventory;
    EquipmentInventory equipmentInventory;

    public Inventory Inventory { get => inventory; set => inventory = value; }
    public EquipmentInventory EquipmentInventory { get => equipmentInventory; set => equipmentInventory = value; }
    public PlayerStatus PlayerStatus { get => playerStatus; set => playerStatus = value; }

    public Action OnChangeEquipment;

    public void Init()
    {
        Data.InventoryData data = Managers.PlayerData.Data.inventoryData;
        inventory = new Inventory(data);
        equipmentInventory = new EquipmentInventory(data);
    }

    public void ChangeEquipments()
    {
        OnChangeEquipment?.Invoke();
    }

    public void Equip(Equipment equipment,int slotIndex = -1) => EquipmentInventory.Equip(equipment,slotIndex);

    public void UnEquip(Equipment equipment) => EquipmentInventory.UnEquip(equipment);

    public void AddItem(Item item) => Inventory.AddItem(item);

    public void RemoveItem(Item item) => Inventory.RemoveItem(item);

    public InventoryData InventoryToData()
    {
        return new InventoryData
        {
            inventoryItemsDatas = Inventory.Items.Select(item => item.ToData()).ToList(),
            equipmentDatas = EquipmentInventory.Equipments.Select(item => item.Value.ToData()).ToList()
        };
    }
}