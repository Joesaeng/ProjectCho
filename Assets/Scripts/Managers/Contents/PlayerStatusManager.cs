using Data;
using System.Collections;
using System.Collections.Generic;
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
        List<EquipmentData> tempDatas = new();

        EquipmentData tempEq = new();
        tempEq.itemName = "Staff";
        tempEq.itemType = ItemType.Equipment;
        tempEq.equipmentType = EquipmentType.Weapon;
        tempEq.equipmentOptions = new();
        tempEq.equipmentOptions.Add(new EquipmentsOptionData
        {
            optionType = EquipmentOptionType.Spell,
            spellId = 2
        });
        tempEq.equipmentOptions.Add(new EquipmentsOptionData
        {
            optionType = EquipmentOptionType.BaseDamage,
            value = 200
        });
        tempEq.equipmentOptions.Add(new EquipmentsOptionData
        {
            optionType = EquipmentOptionType.IncreaseDamage,
            value = 0.2f
        });
        tempEq.equipmentOptions.Add(new EquipmentsOptionData
        {
            optionType = EquipmentOptionType.DecreaseAttackDelay,
            value = 0.5f
        });


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
}
