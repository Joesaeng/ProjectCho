using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemDesc : UI_Base
{
    enum Objects
    {
        Content_Attributes
    }

    enum Buttons
    {
        Button_OffItemDesc,
        Button_Equip,
        Button_Sell
    }

    ItemSlotUIType _selectSlotType;
    Transform _attributes;
    List<TextMeshProUGUI> _attributeTexts = new();

    Button _equipButton;
    Button _sellButton;

    Item _selectedItem;

    public Action<Equipment> OnRingEquip;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.Button_OffItemDesc).gameObject.AddUIEvent(ClickedOffItemDesc);
        _attributes = GetObject((int)Objects.Content_Attributes).transform;
        _equipButton = GetButton((int)Buttons.Button_Equip);
        _sellButton = GetButton((int)Buttons.Button_Sell);

        _equipButton.gameObject.AddUIEvent(ClickedEquipButton);

        // Content 내부의 텍스트들을 초기화
        for (int i = 0; i < _attributes.childCount; ++i)
        {
            TextMeshProUGUI text = _attributes.GetChild(i).GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                _attributeTexts.Add(text);
                text.gameObject.SetActive(false); // 초기에는 비활성화
            }
        }
    }

    void ClickedEquipButton(PointerEventData data)
    {
        if(_selectSlotType == ItemSlotUIType.Inventory)
        {
            if(_selectedItem is Equipment equipment)
            {
                if (equipment.equipmentType == EquipmentType.Weapon)
                    Managers.Player.Equip(equipment);
                else
                    OnRingEquip.Invoke(equipment);
            }
        }
        else if(_selectSlotType == ItemSlotUIType.Ring)
        {
            if (_selectedItem is Equipment equipment)
                Managers.Player.UnEquip(equipment);
        }
        OffItemDesc();
    }

    void ClickedOffItemDesc(PointerEventData data)
    {
        OffItemDesc();
    }

    void OffItemDesc()
    {
        gameObject.SetActive(false);
    }

    public void OnItemDesc(Item item,ItemSlotUIType slotType)
    {
        _selectSlotType = slotType;
        _selectedItem = item;
        if (slotType == ItemSlotUIType.Inventory)
        {
            _equipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Equip";
            _equipButton.gameObject.SetActive(true);
            _sellButton.gameObject.SetActive(true);
        }
        else if (slotType == ItemSlotUIType.Ring)
        {
            _equipButton.GetComponentInChildren<TextMeshProUGUI>().text = "UnEquip";
            _equipButton.gameObject.SetActive(true);
            _sellButton.gameObject.SetActive(false);
        }
        else
        {
            _equipButton.gameObject.SetActive(false);
            _sellButton.gameObject.SetActive(false);
        }
        LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.2f).setEaseOutQuart().setOnComplete(() =>
        {
            LeanTween.scale(gameObject, Vector3.one, 0.2f).setEaseOutQuart();
        });

        int index = 0;
        bool isWeapon = false;
        if (item is Equipment equipment)
        {
            _attributeTexts[index].text = $"{equipment.ItemName}[{equipment.rarity}]";
            _attributeTexts[index].color = ConstantData.TextColorsByRarity[(int)equipment.rarity];
            index++;
            List<EquipmentOption> options = equipment.equipmentOptions;
            if(equipment.equipmentType == EquipmentType.Weapon)
            {
                foreach(EquipmentOption option in options)
                {
                    if(option.OptionType == EquipmentOptionType.Spell)
                    {
                        _attributeTexts[index].text = Language.GetLanguage(Managers.Data.BaseSpellDataDict[option.IntParam1].spellName);
                        _attributeTexts[index].color = 
                           ConstantData.TextColorsByElementTypes[(int)Managers.Data.BaseSpellDataDict[option.IntParam1].elementType];
                    }
                    isWeapon = true;
                    index++;
                    break;
                }
            }
            if (index == 1)
            {
                _attributeTexts[index].gameObject.SetActive(false);
                index++;
            }
            foreach (EquipmentOption option in options)
            {
                if (option.OptionType == EquipmentOptionType.Spell)
                {
                    continue;
                }
                
                if (_attributeTexts.Count <= index)
                {
                    GameObject newText = GameObject.Instantiate(_attributeTexts[index - 1],_attributes).gameObject;
                    _attributeTexts.Add(newText.GetComponent<TextMeshProUGUI>());
                }
                _attributeTexts[index].text = $"{Language.GetLanguage($"{option.OptionType}")} : ";
                string attributeValue = "";
                switch (option.OptionType)
                {
                    case EquipmentOptionType.AddProjectile:
                    case EquipmentOptionType.IncreasePierce:
                        attributeValue = option.IntParam1.ToString();
                        break;
                    case EquipmentOptionType.BaseDamage:
                        attributeValue = option.FloatParam1.ToString();
                        break;
                    case EquipmentOptionType.IncreaseDamage:
                    case EquipmentOptionType.DecreaseSpellDelay:
                        attributeValue = (option.FloatParam1 * 100f).ToString() + "%";
                        break;
                    default:
                        throw new System.ArgumentException($"Unknown OptionType: {option.OptionType}");
                }
                _attributeTexts[index].text += attributeValue;

                index++;
            }
        }
        for(int i = 0; i < index; ++i)
        {
            _attributeTexts[i].gameObject.SetActive(true);
        }
        for(int i = index; i < _attributeTexts.Count; ++i)
        {
            _attributeTexts[i].gameObject.SetActive(false);
        }
        if (isWeapon == false)
            _attributeTexts[1].gameObject.SetActive(false);
    }
}
