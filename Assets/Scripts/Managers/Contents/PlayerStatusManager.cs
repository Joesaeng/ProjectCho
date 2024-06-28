using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerStatus
{
    public int startingSpellId;
    public float damage;
    public Dictionary<StatusType,float> floatOptions = new();
    public Dictionary<StatusType,int> integerOptions = new();

    public void ApplyPlayerStatus(AchievementStatus achievementStatus, EquipmentStatus equipmentStatus)
    {
        startingSpellId = equipmentStatus.startingSpellId;
        damage = equipmentStatus.damage;
        floatOptions = equipmentStatus.floatOptions;
        integerOptions = equipmentStatus.integerOptions;

        foreach(var achievementStatusFloatOption in achievementStatus.floatOptions)
        {
            if (floatOptions.ContainsKey(achievementStatusFloatOption.Key))
                floatOptions[achievementStatusFloatOption.Key] += achievementStatusFloatOption.Value;
            else
                floatOptions[achievementStatusFloatOption.Key] = achievementStatusFloatOption.Value;
        }

        foreach (var achievementStatusintegerOption in achievementStatus.integerOptions)
        {
            if (integerOptions.ContainsKey(achievementStatusintegerOption.Key))
                integerOptions[achievementStatusintegerOption.Key] += achievementStatusintegerOption.Value;
            else
                integerOptions[achievementStatusintegerOption.Key] = achievementStatusintegerOption.Value;
        }
    }
}

public class AchievementStatus
{
    public Dictionary<StatusType,float> floatOptions = new();
    public Dictionary<StatusType,int> integerOptions = new();

    public void ApplyAchievementRewardStatus(AchievementReward achievementReward)
    {
        switch (achievementReward.statusType)
        {
            case StatusType.BaseDamage:
            case StatusType.AddProjectile:
            case StatusType.IncreasePierce:
                if (integerOptions.ContainsKey(achievementReward.statusType))
                    integerOptions[achievementReward.statusType] += achievementReward.integerParam;
                else
                    integerOptions[achievementReward.statusType] = achievementReward.integerParam;
                break;
            case StatusType.IncreaseDamage:
            case StatusType.DecreaseSpellDelay:
            case StatusType.IncreaseEnergySpellDamage:
            case StatusType.IncreaseFireSpellDamage:
            case StatusType.IncreaseWaterSpellDamage:
            case StatusType.IncreaseLightningSpellDamage:
            case StatusType.IncreaseEarthSpellDamage:
            case StatusType.IncreaseWindSpellDamage:
                if (floatOptions.ContainsKey(achievementReward.statusType))
                    floatOptions[achievementReward.statusType] += achievementReward.floatParam;
                else
                    floatOptions[achievementReward.statusType] = achievementReward.floatParam;
                break;

        }
        Managers.Status.ApplyPlayerStatus();
    }
}

public class EquipmentStatus
{
    public int startingSpellId;
    public float damage;
    public Dictionary<StatusType,float> floatOptions = new();
    public Dictionary<StatusType,int> integerOptions = new();

    public void ApplyEquipmentStatus(EquipmentStatus equipmentStatus)
    {
        startingSpellId = equipmentStatus.startingSpellId;
        damage = equipmentStatus.damage;
        floatOptions = equipmentStatus.floatOptions;
        integerOptions = equipmentStatus.integerOptions;

        Managers.Status.ApplyPlayerStatus();
    }
}

public class PlayerStatusManager
{
    PlayerStatus playerStatus;
    AchievementStatus achievementStatus;
    EquipmentStatus equipmentStatus;
    PlayerSpells playerSpells;
    Inventory inventory;
    EquipmentInventory equipmentInventory;

    public PlayerStatus PlayerStatus { get => playerStatus; set => playerStatus = value; }
    public AchievementStatus AchievementStatus { get => achievementStatus; set => achievementStatus = value; }
    public EquipmentStatus EquipmentStatus { get => equipmentStatus; set => equipmentStatus = value; }
    public PlayerSpells PlayerSpells { get => playerSpells; set => playerSpells = value; }
    public Inventory Inventory { get => inventory; set => inventory = value; }
    public EquipmentInventory EquipmentInventory { get => equipmentInventory; set => equipmentInventory = value; }

    public Action OnChangeEquipment;
    public Action OnChangeInventory;
    public Action OnApplyPlayerStatus;

    public void Init()
    {
        playerStatus = new PlayerStatus();
        achievementStatus = new AchievementStatus();
        equipmentStatus = new EquipmentStatus();
        InventoryData data = Managers.PlayerData.InventoryData;
        inventory = new Inventory();
        equipmentInventory = new EquipmentInventory();
        PlayerSpells = new PlayerSpells();
        inventory.Init(data);
        equipmentInventory.Init(data);
        PlayerSpells.Init();
    }

    public void ChangeEquipments()
    {
        OnChangeEquipment?.Invoke();
    }

    public void ChangeInventory()
    {
        OnChangeInventory?.Invoke();
    }

    public void Equip(Equipment equipment, int slotIndex = -1)
    {
        EquipmentInventory.Equip(equipment, slotIndex);
    }

    public void UnEquip(Equipment equipment)
    {
        EquipmentInventory.UnEquip(equipment);
    }

    public bool HasEmptyRingSlots() => EquipmentInventory.HasEmptyRingSlots();

    //public void AddItem(Item item)
    //{
    //    Inventory.AddItem(item);
    //}

    //public void AddItems(List<Item> items)
    //{
    //    Inventory.AddItems(items);
    //}

    //public void RemoveItem(Item item)
    //{
    //    Inventory.RemoveItem(item);
    //}

    public void AddItem(Equipment item)
    {
        Inventory.AddItem(item);
    }

    public void AddItems(List<Equipment> items)
    {
        Inventory.AddItems(items);
    }

    public void RemoveItem(Equipment item)
    {
        Inventory.RemoveItem(item);
    }

    public void AddSpells(int spellId, int count)
    {
        PlayerSpells.AddSpell(spellId, count);
    }

    public void ApplyAchievementRewardStatus(AchievementReward achievementReward)
    {
        AchievementStatus.ApplyAchievementRewardStatus(achievementReward);
    }

    public void ApplyEquipmentStatus(EquipmentStatus equipmentStatus)
    {
        EquipmentStatus.ApplyEquipmentStatus(equipmentStatus);
    }

    public void ApplyPlayerStatus()
    {
        PlayerStatus.ApplyPlayerStatus(AchievementStatus, EquipmentStatus);
        OnApplyPlayerStatus?.Invoke();
    }

    public InventoryData InventoryToData()
    {
        return new InventoryData
        {
            // Inventory.Items가 null일 경우 빈 리스트를 반환
            inventoryItemsDatas = Inventory.Items?.Select(item => item.ToData()).ToList() ?? new List<EquipmentData>(),

            // EquipmentInventory.Equipments가 null일 경우 빈 리스트를 반환
            equipmentDatas = EquipmentInventory.ToData()
        };
    }

    public List<PlayerOwnedSpellData> SpellDataBaseToData()
    {
        return PlayerSpells.SpellDataDictToData();
    }

    public void Clear()
    {
        OnChangeEquipment = null;
        OnChangeInventory = null;
        OnApplyPlayerStatus = null;
        playerSpells.OnChangedSpellData = null;
    }
}
