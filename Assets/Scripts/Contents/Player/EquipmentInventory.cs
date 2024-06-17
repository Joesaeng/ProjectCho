using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentInventory
{
    private Dictionary<EquipmentType, Equipment> equipments;
    private List<RingSlot> ringSlots;

    public Dictionary<EquipmentType, Equipment> Equipments { get => equipments; set => equipments = value; }
    public List<RingSlot> RingSlots { get => ringSlots; }

    public EquipmentInventory(Data.InventoryData data)
    {
        equipments = new Dictionary<EquipmentType, Equipment>();
        ringSlots = new List<RingSlot>(ConstantData.MaxRingSlots);
        for (int i = 0; i < ConstantData.MaxRingSlots; i++)
        {
            ringSlots.Add(new RingSlot());
        }

        for (int i = 0; i < data.equipmentDatas.Count; ++i)
        {
            Equipment equipment = ItemGenerator.GenerateItem(data.equipmentDatas[i]) as Equipment;
            Equip(equipment, equipment.equipSlotIndex);
        }
    }

    public void Equip(Equipment equipment, int slotIndex = -1)
    {
        equipment.isEquip = true;
        if (equipment.equipmentType == EquipmentType.Weapon)
        {
            // 무기는 하나만 장착 가능
            if (equipments.ContainsKey(EquipmentType.Weapon))
            {
                UnEquip(equipments[EquipmentType.Weapon]);
            }
            equipments[EquipmentType.Weapon] = equipment;
        }
        else if (equipment.equipmentType == EquipmentType.Ring)
        {
            if (slotIndex >= 0 && slotIndex < ConstantData.MaxRingSlots)
            {
                EquipRing(equipment, slotIndex);
            }
            else
            {
                // 빈 슬롯에 장착
                foreach (var slot in ringSlots)
                {
                    if (slot.IsEmpty)
                    {
                        slot.EquipRing(equipment);
                        break;
                    }
                }
            }
        }
        Managers.Player.RemoveItem(equipment);
        ApplyEquipmentStatus();
    }

    public void UnEquip(Equipment equipment)
    {
        equipment.isEquip = false;
        if (equipment.equipmentType == EquipmentType.Weapon)
        {
            if (equipments.ContainsKey(equipment.equipmentType))
            {
                Managers.Player.AddItem(equipments[equipment.equipmentType]);
                equipments.Remove(equipment.equipmentType);
            }
        }
        else if (equipment.equipmentType == EquipmentType.Ring)
        {
            foreach (var slot in ringSlots)
            {
                if (slot.Ring == equipment)
                {
                    Managers.Player.AddItem(slot.UnEquipRing());
                    break;
                }
            }
        }
        ApplyEquipmentStatus();
    }

    public bool HasEmptyRingSlots()
    {
        foreach (var slot in ringSlots)
        {
            if (slot.IsEmpty)
            {
                return true;
            }
        }
        return false;
    }

    private void EquipRing(Equipment ring, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < ConstantData.MaxRingSlots)
        {
            if (!ringSlots[slotIndex].IsEmpty)
            {
                // 이미 장착된 반지가 있으면 이를 인벤토리로 돌려놓음
                UnEquip(ringSlots[slotIndex].Ring);
            }
            ringSlots[slotIndex].EquipRing(ring);
            ring.equipSlotIndex = slotIndex;
        }
        else
        {
            throw new System.ArgumentOutOfRangeException("Invalid ring slot index");
        }
    }

    public void ApplyEquipmentStatus()
    {
        PlayerStatus playerStatus = new PlayerStatus();
        float baseDamage = 0;

        foreach (var equipment in equipments.Values)
        {
            ApplyEquipmentOptions(equipment, playerStatus);
        }

        foreach (var slot in ringSlots)
        {
            if (!slot.IsEmpty)
            {
                ApplyEquipmentOptions(slot.Ring, playerStatus);
            }
        }

        if (playerStatus.integerOptions.ContainsKey(StatusType.Spell))
        {
            playerStatus.startingSpellId = playerStatus.integerOptions[StatusType.Spell];
        }

        if (playerStatus.floatOptions.ContainsKey(StatusType.BaseDamage))
        {
            baseDamage = playerStatus.floatOptions[StatusType.BaseDamage];
        }

        playerStatus.damage = baseDamage;
        if (playerStatus.floatOptions.TryGetValue(StatusType.IncreaseDamage, out float increaseDamage))
            playerStatus.damage *= (1 + increaseDamage);

        Managers.Player.PlayerStatus = playerStatus;
        Managers.Player.ChangeEquipments();
    }

    private void ApplyEquipmentOptions(Equipment equipment, PlayerStatus playerStatus)
    {
        foreach (var option in equipment.equipmentOptions)
        {
            switch (option.OptionType)
            {
                case StatusType.Spell:
                case StatusType.AddProjectile:
                case StatusType.IncreasePierce:
                    if (playerStatus.integerOptions.ContainsKey(option.OptionType))
                    {
                        playerStatus.integerOptions[option.OptionType] += option.IntParam1;
                    }
                    else
                    {
                        playerStatus.integerOptions[option.OptionType] = option.IntParam1;
                    }
                    break;
                case StatusType.BaseDamage:
                case StatusType.IncreaseDamage:
                case StatusType.DecreaseSpellDelay:
                case StatusType.IncreaseEnergySpellDamage:
                case StatusType.IncreaseFireSpellDamage:
                case StatusType.IncreaseWaterSpellDamage:
                case StatusType.IncreaseLightningSpellDamage:
                case StatusType.IncreaseEarthSpellDamage:
                case StatusType.IncreaseAirSpellDamage:
                case StatusType.IncreaseLightSpellDamage:
                case StatusType.IncreaseDarkSpellDamage:
                    if (playerStatus.floatOptions.ContainsKey(option.OptionType))
                    {
                        playerStatus.floatOptions[option.OptionType] += option.FloatParam1;
                    }
                    else
                    {
                        playerStatus.floatOptions[option.OptionType] = option.FloatParam1;
                    }
                    break;
                default:
                    throw new System.ArgumentException($"Unknown OptionType: {option.OptionType}");
            }
        }
    }
}

public class RingSlot
{
    public Equipment Ring { get; private set; }

    public bool IsEmpty => Ring == null;

    public void EquipRing(Equipment ring)
    {
        if (ring.equipmentType != EquipmentType.Ring)
        {
            throw new System.ArgumentException("Only rings can be equipped in a RingSlot.");
        }
        Ring = ring;
    }

    public Equipment UnEquipRing()
    {
        Equipment unequippedRing = Ring;
        Ring = null;
        return unequippedRing;
    }
}

