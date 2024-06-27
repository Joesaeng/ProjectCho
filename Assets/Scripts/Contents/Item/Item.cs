using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Item
{
    private string itemName;
    private string itemSpriteName;
    private Sprite itemIcon;

    public string ItemName { get => itemName; set => itemName = value; }
    public string ItemSpriteName { get => itemSpriteName; set => itemSpriteName = value; }
    public Sprite ItemIcon { get => itemIcon; set => itemIcon = value; }

    public abstract void ApplyItem();
    // public abstract ItemData ToData();
}

public class EquipmentOption
{
    private StatusType optionType;
    private int intParam1;
    private float floatParam1;

    public StatusType OptionType { get => optionType; set => optionType = value; }
    public int IntParam1 { get => intParam1; set => intParam1 = value; }
    public float FloatParam1 { get => floatParam1; set => this.floatParam1 = value; }

    public EquipmentOption(EquipmentOptionData data)
    {
        // 새로운 옵션 생성 시 저장된 데이터의 param1 부터 param2까지 값 중 랜덤한 값을 선택한다.
        optionType = data.optionType;
        if (data.intParam2 != 0)
            intParam1 = Random.Range(data.intParam1, data.intParam2 + 1);
        else
            intParam1 = data.intParam1;
        if (data.floatParam2 != 0)
        {
            if (data.floatParam1 >= 1)
                floatParam1 = Mathf.CeilToInt(Random.Range(data.floatParam1, data.floatParam2));
            else
                floatParam1 = (float)System.Math.Round(Random.Range(data.floatParam1, data.floatParam2),3);
        }
        else
            floatParam1 = data.floatParam1;
    }

    public EquipmentOptionData ToData()
    {
        // 이미 생성된 아이템을 저장할 때에는 param1만 저장함으로 데이터의 무결성을 보존한다.
        return new EquipmentOptionData
        {
            optionType = this.optionType,
            intParam1 = this.intParam1,
            floatParam1 = this.floatParam1,
        };
    }
}

public class Equipment : Item
{
    public EquipmentRarity rarity;
    public EquipmentType equipmentType;
    public List<EquipmentOption> equipmentOptions;
    public bool isEquip;
    public int equipSlotIndex;

    public Equipment(EquipmentData data)
    {
        ItemName = data.itemName;
        ItemSpriteName = data.itemSpriteName;
        rarity = data.rarity;
        equipmentType = data.equipmentType;
        isEquip = data.isEquip;
        equipSlotIndex = data.equipSlotIndex;
        equipmentOptions = new();
        for(int i = 0; i < data.equipmentOptions.Count; ++i)
        {
            equipmentOptions.Add(new EquipmentOption(data.equipmentOptions[i]));
        }
    }
    public override void ApplyItem()
    {
        Managers.Player.Equip(this);
    }

    public EquipmentData ToData()
    {
        return new EquipmentData
        {
            itemName = ItemName,
            itemSpriteName = ItemSpriteName,
            rarity = rarity,
            equipmentType = equipmentType,
            isEquip = isEquip,
            equipSlotIndex = equipSlotIndex,
            itemType = ItemType.Equipment,
            equipmentOptions = equipmentOptions.Select(option => option.ToData()).ToList()
        };
    }
}


