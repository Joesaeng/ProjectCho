using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentInventory
{
    private Dictionary<EquipmentType, Equipment> equipments;

    public Dictionary<EquipmentType, Equipment> Equipments { get => equipments; set => equipments = value; }

    public EquipmentInventory(Data.InventoryData data)
    {
        equipments = new();
        for(int i = 0; i < data.equipmentDatas.Count; ++i)
        {
            Equipment equipment = ItemGenerator.GenerateItem(data.equipmentDatas[i]) as Equipment;
            Equip(equipment);
        }
    }

    public void Equip(Equipment equipment)
    {
        if(equipments.ContainsKey(equipment.equipmentType))
        {
            UnEquip(equipments[equipment.equipmentType]);
        }
        equipments.Add(equipment.equipmentType, equipment);
        ApplyEquipmentStatus();
    }

    public void UnEquip(Equipment equipment)
    {
        equipments.Remove(equipment.equipmentType);
        Managers.Player.AddItem(equipment);
        ApplyEquipmentStatus();
    }

    public PlayerStatus ApplyEquipmentStatus()
    {
        PlayerStatus playerStatus = new();
        int spellId = 0;
        float baseDamage = 0;
        float increaseDamage = 0;
        float decreaseAttackDelay = 0;

        foreach(Equipment equipment in equipments.Values)
        {
            for(int i = 0; i < equipment.equipmentOptions.Count; ++i)
            {
                switch (equipment.equipmentOptions[i].OptionType)
                {
                    case EquipmentOptionType.Spell:
                        spellId = equipment.equipmentOptions[i].SpellId;
                        break;
                    case EquipmentOptionType.BaseDamage:
                        baseDamage = equipment.equipmentOptions[i].Value;
                        break;
                    case EquipmentOptionType.IncreaseDamage:
                        increaseDamage += equipment.equipmentOptions[i].Value;
                        break;
                    case EquipmentOptionType.DecreaseAttackDelay:
                        decreaseAttackDelay += equipment.equipmentOptions[i].Value;
                        break;
                    default:
                        throw new System.ArgumentException($"Unknown OptionType: {equipment.equipmentOptions[i].OptionType}");
                }
            }
        }

        playerStatus.startingSpellId = spellId;
        playerStatus.damage = baseDamage * (1 + increaseDamage);
        playerStatus.decreaseAttackDelay = decreaseAttackDelay;
        return playerStatus;
    }
}

