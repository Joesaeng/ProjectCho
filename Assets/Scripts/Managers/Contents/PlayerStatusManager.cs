using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatus
{
    public int startingSpellId;
    public float damage;
    public float decreaseAttackDelay;
    // 공격력
    // 게임 시작시 가지고 있을 스킬
    // 등등
}
public class PlayerStatusManager
{
    PlayerStatus playerStatus;
    Inventory inventory;
    EquipmentInventory equipmentInventory;

    public Inventory Inventory { get => inventory; set => inventory = value; }
    public EquipmentInventory EquipmentInventory { get => equipmentInventory; set => equipmentInventory = value; }
    public PlayerStatus PlayerStatus { get => playerStatus; set => playerStatus = value; }

    public void Init()
    {
        // Data.InventoryData data = Managers.PlayerData.Data.inventoryData;
        // Inventory = new Inventory(data);

        // TEMP
        List<ItemData> tempDatas = new();

        EquipmentData tempEq = new();
        tempEq.itemName = "Staff";
        tempEq.itemType = ItemType.Equipment;
        tempEq.equipmentType = EquipmentType.Weapon;
        tempEq.equipmentOptions = new()
        {
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.Spell,
                spellId = 0
            },
            new EquipmentsOptionData
            {
                optionType = EquipmentOptionType.BaseDamage,
                value = 5
            }
        };


        InventoryData inventoryData = new();

        tempDatas.Add(tempEq);
        inventoryData.equipmentDatas = tempDatas;
        EquipmentInventory = new EquipmentInventory(inventoryData);

        ApplyEquipmentStatus();
    }

    public void ApplyEquipmentStatus()
    {
        PlayerStatus = equipmentInventory.ApplyEquipmentStatus();
    }

    public void Equip(Equipment equipment) => EquipmentInventory.Equip(equipment);

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
