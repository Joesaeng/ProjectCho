using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentInventory
{
    private Dictionary<EquipmentType, Equipment> equipments = new();
    private List<RingSlot> ringSlots = new List<RingSlot>(ConstantData.MaxRingSlots);

    public Dictionary<EquipmentType, Equipment> Equipments { get => equipments; set => equipments = value; }
    public List<RingSlot> RingSlots { get => ringSlots; }

    public EquipmentInventory()
    {
        for (int slotIndex = 0; slotIndex < ConstantData.MaxRingSlots; slotIndex++)
        {
            ringSlots.Add(new RingSlot(slotIndex));
        }
    }
    
    public void Init(InventoryData data)
    {
        foreach(var equipmentData in data.equipmentDatas)
        {
            if (equipmentData == null)
                continue;

            Equipment equipment = ItemGenerator.GenerateItem(equipmentData);
            Equip(equipment, equipment.equipSlotIndex);
        }
        //for (int i = 0; i < data.equipmentDatas.Count; ++i)
        //{
        //    Equipment equipment = ItemGenerator.GenerateItem(data.equipmentDatas[i]);
        //    Equip(equipment, equipment.equipSlotIndex);
        //}
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
        Managers.Status.RemoveItem(equipment);
        ApplyEquipmentStatus();
    }

    public void UnEquip(Equipment equipment)
    {
        equipment.isEquip = false;
        if (equipment.equipmentType == EquipmentType.Weapon)
        {
            if (equipments.ContainsKey(equipment.equipmentType))
            {
                Managers.Status.AddItem(equipments[equipment.equipmentType]);
                equipment.equipSlotIndex = -1;
                equipments.Remove(equipment.equipmentType);
            }
        }
        else if (equipment.equipmentType == EquipmentType.Ring)
        {
            foreach (var slot in ringSlots)
            {
                if (slot.Ring == equipment)
                {
                    Managers.Status.AddItem(slot.UnEquipRing());
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
        }
        else
        {
            throw new System.ArgumentOutOfRangeException("Invalid ring slot index");
        }
    }

    public void ApplyEquipmentStatus()
    {
        EquipmentStatus equipmentStatus = new();
        float baseDamage = 0;

        foreach (var equipment in equipments.Values)
        {
            ApplyEquipmentOptions(equipment, equipmentStatus);
        }

        foreach (var slot in ringSlots)
        {
            if (!slot.IsEmpty)
            {
                ApplyEquipmentOptions(slot.Ring, equipmentStatus);
            }
        }

        if (equipmentStatus.integerOptions.ContainsKey(StatusType.Spell))
        {
            equipmentStatus.startingSpellId = equipmentStatus.integerOptions[StatusType.Spell];
        }

        if (equipmentStatus.floatOptions.ContainsKey(StatusType.BaseDamage))
        {
            baseDamage = equipmentStatus.floatOptions[StatusType.BaseDamage];
        }

        equipmentStatus.damage = baseDamage;
        if (equipmentStatus.floatOptions.TryGetValue(StatusType.IncreaseDamage, out float increaseDamage))
            equipmentStatus.damage *= (1 + increaseDamage);

        Managers.Status.ApplyEquipmentStatus(equipmentStatus);
        Managers.Status.ChangeEquipments();
    }

    private void ApplyEquipmentOptions(Equipment equipment, EquipmentStatus equipmentStatus)
    {
        foreach (var option in equipment.equipmentOptions)
        {
            switch (option.OptionType)
            {
                case StatusType.Spell:
                case StatusType.AddProjectile:
                case StatusType.IncreasePierce:
                    if (equipmentStatus.integerOptions.ContainsKey(option.OptionType))
                    {
                        equipmentStatus.integerOptions[option.OptionType] += option.IntParam1;
                    }
                    else
                    {
                        equipmentStatus.integerOptions[option.OptionType] = option.IntParam1;
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
                case StatusType.IncreaseWindSpellDamage:
                case StatusType.IncreaseLightSpellDamage:
                case StatusType.IncreaseDarkSpellDamage:
                    if (equipmentStatus.floatOptions.ContainsKey(option.OptionType))
                    {
                        equipmentStatus.floatOptions[option.OptionType] += option.FloatParam1;
                    }
                    else
                    {
                        equipmentStatus.floatOptions[option.OptionType] = option.FloatParam1;
                    }
                    break;
                default:
                    throw new System.ArgumentException($"Unknown OptionType: {option.OptionType}");
            }
        }
    }

    public List<Data.EquipmentData> ToData()
    {
        List<Data.EquipmentData> equipmentDatas = equipmentDatas = Equipments?
            .Select(item => item.Value.ToData()).ToList() ?? new List<Data.EquipmentData>();
        equipmentDatas.AddRange(RingSlots.Select(data => data.Ring?.ToData()).ToList());

        return equipmentDatas;
    }
}

public class RingSlot
{
    public Equipment Ring { get; private set; }

    public int slotIndex;
    public RingSlot(int slotIndex) => this.slotIndex = slotIndex;
    public bool IsEmpty => Ring == null;

    public void EquipRing(Equipment ring)
    {
        if (ring.equipmentType != EquipmentType.Ring)
        {
            throw new System.ArgumentException("Only rings can be equipped in a RingSlot.");
        }
        ring.equipSlotIndex = slotIndex;
        Ring = ring;
    }

    public Equipment UnEquipRing()
    {
        Equipment unequippedRing = Ring;
        Ring.equipSlotIndex = -1;
        Ring = null;
        return unequippedRing;
    }
}

