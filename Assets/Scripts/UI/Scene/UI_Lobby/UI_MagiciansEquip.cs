using Data;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ItemSlotUIType
{
    Inventory,
    Weapon,
    Ring,
}

public class UI_MagiciansEquip : UI_Base
{
    enum Objects
    {
        Content_Inventory,
        Content_StatusValue,
        UI_ItemDesc,
        Panel_EquipRingSlotButton,
        UI_ItemOptionFilter
    }
    enum Buttons
    {
        Button_WeaponTab,
        Button_RingTab,
        Button_ShowFilter,

        // EquipRingSlots
        Button_Outside,
        Button_RingSlot0,
        Button_RingSlot1,
        Button_RingSlot2,
        Button_RingSlot3,
    }
    enum Texts
    {
        Text_WeaponSpell,
        Text_RingTab,
        Text_WeaponTab,
        Text_ShowFilter
    }

    [SerializeField] Sprite[] _buttonTabSprites; // 0 : enabled, 1 : disenabled
    Dictionary<EquipmentType, Button> _buttons = new();
    EquipmentType _showItemType = EquipmentType.Ring;

    Transform _statusValueContentTf;
    List<TextMeshProUGUI> _statusValueTexts = new();
    TextMeshProUGUI _weaponSpellText;

    Transform _inventory;
    UI_ItemDesc _itemDescUi;
    List<UI_InventorySlot> _inventorySlots = new();
    UI_EquipSlot _weaponSlot;
    UI_EquipSlot[] _ringSlots;
    Equipment selectedRing;


    UI_ItemOptionFilter _ui_itemOptionFilter;
    Dictionary<StatusType,bool> _itemOptionFilter = new();
    bool _isItemOptionFiltering = false;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        InitStatus();
        InitItemDesc();
        InitInventorySlot();
        InitEquipmentSlot();
        InitEquipRing();
        InitItemFilter();

        GetText((int)Texts.Text_RingTab).text = Language.GetLanguage("Ring");
        GetText((int)Texts.Text_WeaponTab).text = Language.GetLanguage("Weapon");

        SetEquipSlots();
        SetInventoryTab();
    }
    #region Init
    void InitStatus()
    {
        _statusValueContentTf = GetObject((int)Objects.Content_StatusValue).transform;
        for (int i = 0; i < _statusValueContentTf.childCount; ++i)
        {
            if (_statusValueContentTf.GetChild(i).TryGetComponent<TextMeshProUGUI>(out var text))
            {
                _statusValueTexts.Add(text);
                text.gameObject.SetActive(false); // 초기에는 비활성화
            }
        }
        _weaponSpellText = GetText((int)Texts.Text_WeaponSpell);

        Managers.Status.OnApplyPlayerStatus += SetStatusValues;
        SetStatusValues();
    }

    void InitItemDesc()
    {
        _itemDescUi = GetObject((int)Objects.UI_ItemDesc).GetComponent<UI_ItemDesc>();
        _itemDescUi.Init();
        _itemDescUi.gameObject.SetActive(false);
    }

    void InitInventorySlot()
    {
        _inventory = GetObject((int)Objects.Content_Inventory).transform;
        for (int i = 0; i < _inventory.childCount; ++i)
        {
            if (_inventory.GetChild(i).TryGetComponent<UI_InventorySlot>(out var slot))
            {
                _inventorySlots.Add(slot);
                slot.Init();
                slot.gameObject.SetActive(false); // 초기에는 비활성화
            }
        }
    }

    void InitEquipmentSlot()
    {
        UI_EquipSlot[] equipSlots = GetComponentsInChildren<UI_EquipSlot>();
        _ringSlots = new UI_EquipSlot[ConstantData.MaxRingSlots];
        foreach (var slot in equipSlots)
        {
            slot.Init();
            slot.OnClickedEquipSlot += ClickedEquipSlotListner;
            if (slot._equipmentType == EquipmentType.Weapon)
                _weaponSlot = slot;
            else
                _ringSlots[slot._slotIndex] = slot;
        }
        Managers.Status.OnChangeEquipment += ChangeEquipmentsListner;
        Managers.Status.OnChangeInventory += ChangeInventoryListner;
    }

    void InitEquipRing()
    {
        _itemDescUi.OnRingEquip += EquipRingListner;

        GetObject((int)Objects.Panel_EquipRingSlotButton).SetActive(false);

        GetButton((int)Buttons.Button_Outside).gameObject.AddUIEvent(ClickedEquipRingOutside);

        GetButton((int)Buttons.Button_RingSlot0).gameObject.AddUIEvent(ClickedEquipRingSlot, 0);
        GetButton((int)Buttons.Button_RingSlot1).gameObject.AddUIEvent(ClickedEquipRingSlot, 1);
        GetButton((int)Buttons.Button_RingSlot2).gameObject.AddUIEvent(ClickedEquipRingSlot, 2);
        GetButton((int)Buttons.Button_RingSlot3).gameObject.AddUIEvent(ClickedEquipRingSlot, 3);

        GetButton((int)Buttons.Button_WeaponTab).gameObject.AddUIEvent(ClickedInventoryTab, EquipmentType.Weapon);
        _buttons.Add(EquipmentType.Weapon, GetButton((int)Buttons.Button_WeaponTab));
        GetButton((int)Buttons.Button_RingTab).gameObject.AddUIEvent(ClickedInventoryTab, EquipmentType.Ring);
        _buttons.Add(EquipmentType.Ring, GetButton((int)Buttons.Button_RingTab));
    }

    void InitItemFilter()
    {
        _ui_itemOptionFilter = GetObject((int)Objects.UI_ItemOptionFilter).GetComponent<UI_ItemOptionFilter>();
        _ui_itemOptionFilter.Init();
        _ui_itemOptionFilter.gameObject.SetActive(false);
        _ui_itemOptionFilter.OnApplyItemOptionFilter += ApplyItemOptionFilter;
        _ui_itemOptionFilter.OnResetFilter += ResetAndOffItemOptionFilter;

        GetText((int)Texts.Text_ShowFilter).text = Language.GetLanguage("Filter");
        GetButton((int)Buttons.Button_ShowFilter).gameObject.AddUIEvent(ShowItemOptionFilter);
        for (int i = 0; i < Enum.GetValues(typeof(StatusType)).Length; i++)
        {
            StatusType type = (StatusType)i;
            _itemOptionFilter[type] = type != StatusType.Spell && type != StatusType.BaseDamage;
        }

    }
    #endregion

    #region Status

    void SetStatusValues()
    {
        PlayerStatus playerStatus = Managers.Status.PlayerStatus;

        // 무기 주문 텍스트 설정
        _weaponSpellText.text = Language.GetLanguage(Managers.Data.BaseSpellDataDict[playerStatus.startingSpellId].spellName);
        _weaponSpellText.color = ConstantData.TextColorsByElementTypes[(int)Managers.Data.BaseSpellDataDict[playerStatus.startingSpellId].elementType];

        // 상태 값 텍스트 초기화
        foreach (var text in _statusValueTexts)
        {
            text.gameObject.SetActive(false); // 모든 텍스트 요소를 비활성화
        }

        int index = 0;

        // Damage 값을 첫 번째 텍스트 요소에 정수형으로 반올림하여 설정
        if (_statusValueTexts.Count <= index)
        {
            AddStatusValueTextObject();
        }
        _statusValueTexts[index].text = $"{Language.GetLanguage("Damage")}: {Mathf.RoundToInt(playerStatus.damage)}";
        _statusValueTexts[index].gameObject.SetActive(true);
        index++;

        // Integer Options 설정
        foreach (var option in playerStatus.integerOptions)
        {
            if (option.Key == StatusType.Spell)
                continue;
            if (index >= _statusValueTexts.Count)
            {
                AddStatusValueTextObject();
            }

            string optionName = Language.GetLanguage(option.Key.ToString());
            _statusValueTexts[index].text = $"{optionName}: {option.Value}";
            _statusValueTexts[index].gameObject.SetActive(true);
            index++;
        }

        // Float Options 설정
        foreach (var option in playerStatus.floatOptions)
        {
            if (option.Key == StatusType.IncreaseDamage || option.Key == StatusType.BaseDamage)
                continue;
            if (index >= _statusValueTexts.Count)
            {
                AddStatusValueTextObject();
            }

            string optionName = Language.GetLanguage(option.Key.ToString());
            _statusValueTexts[index].text = $"{optionName}: {option.Value * 100f:F2}%";
            _statusValueTexts[index].gameObject.SetActive(true);
            index++;
        }
    }

    void AddStatusValueTextObject()
    {
        GameObject textObject = Instantiate(_statusValueTexts[0].gameObject, _statusValueContentTf);
        if (textObject.TryGetComponent<TextMeshProUGUI>(out var text))
        {
            _statusValueTexts.Add(text);
            text.gameObject.SetActive(false); // 초기에는 비활성화
        }
    }
    #endregion

    #region Equip

    void EquipRingListner(Equipment ring)
    {
        if (Managers.Status.HasEmptyRingSlots())
        {
            Managers.Status.Equip(ring);
            LobbySceneManager.Instance.SaveDataOnLobbyScene();
        }
        else
        {
            GetObject((int)Objects.Panel_EquipRingSlotButton).SetActive(true);
            selectedRing = ring;
        }
    }

    void ClickedEquipRingOutside(PointerEventData data)
    {
        GetObject((int)Objects.Panel_EquipRingSlotButton).SetActive(false);
        selectedRing = null;
    }

    void ClickedEquipRingSlot(int slotIndex, PointerEventData data)
    {
        if (selectedRing == null)
            return;
        Managers.Status.Equip(selectedRing, slotIndex);
        LobbySceneManager.Instance.SaveDataOnLobbyScene();
        GetObject((int)Objects.Panel_EquipRingSlotButton).SetActive(false);
        selectedRing = null;
    }

    void SetEquipSlots()
    {
        Dictionary<EquipmentType,Equipment> equipments = Managers.Status.EquipmentInventory.Equipments;
        List<RingSlot> ringslots = Managers.Status.EquipmentInventory.RingSlots;
        foreach (var equip in equipments.Values)
        {
            _weaponSlot.SetEquipSlot(equip);
        }
        for (int i = 0; i < ringslots.Count; ++i)
        {
            _ringSlots[i].SetEquipSlot(ringslots[i].Ring);
        }
    }

    void ChangeEquipmentsListner()
    {
        SetStatusValues();
        SetEquipSlots();
        SetInventory();
    }

    void ClickedEquipSlotListner(Equipment item, ItemSlotUIType slotType)
    {
        if (item == null)
        {
            return;
        }
        _itemDescUi.gameObject.SetActive(true);
        _itemDescUi.OnItemDesc(item, slotType);
    }
    #endregion

    #region Inventory


    void ChangeInventoryListner()
    {
        SetInventory();
    }

    void ClickedItemListner(Equipment item, ItemSlotUIType slotType)
    {
        _itemDescUi.gameObject.SetActive(true);
        _itemDescUi.OnItemDesc(item, slotType);
    }

    void ClickedInventoryTab(EquipmentType type, PointerEventData data)
    {
        Managers.Sound.Play("ui_click");
        _showItemType = type;
        SetInventoryTab();
    }

    void SetInventoryTab()
    {
        foreach (var tabButton in _buttons)
        {
            if (tabButton.Key == _showItemType)
                tabButton.Value.GetComponent<Image>().sprite = _buttonTabSprites[0];
            else
                tabButton.Value.GetComponent<Image>().sprite = _buttonTabSprites[1];
        }
        SetInventory();
    }

    void SetInventory()
    {
        List<Equipment> inventoryItems = Managers.Status.Inventory.Items;

        if (inventoryItems == null)
        {
            Debug.LogError("Inventory items are null");
            return;
        }

        int slotIndex = 0;

        foreach (Equipment item in inventoryItems.Cast<Equipment>())
        {
            if (item.equipmentType == _showItemType)
            {
                // 아이템의 옵션 중 아이템 옵션 필터에 걸리는 옵션이 있는지 확인하여 표시
                if (_isItemOptionFiltering)
                {
                    if (!item.equipmentOptions.Any(option =>
                    _itemOptionFilter.ContainsKey(option.OptionType) &&
                    _itemOptionFilter[option.OptionType]))
                        continue;
                }


                if (slotIndex >= _inventorySlots.Count)
                {
                    // 슬롯이 부족하면 추가 생성
                    UI_InventorySlot newSlot = Managers.UI.MakeSubItem<UI_InventorySlot>(_inventory);
                    if (newSlot != null)
                    {
                        newSlot.Init();
                        _inventorySlots.Add(newSlot);
                    }
                    else
                    {
                        Debug.LogError("Failed to create a new inventory slot.");
                        continue;
                    }
                }

                _inventorySlots[slotIndex].SetItem(item); // 아이템 설정
                _inventorySlots[slotIndex].gameObject.SetActive(true); // 슬롯 활성화
                _inventorySlots[slotIndex].OnClickedItem = null;
                _inventorySlots[slotIndex].OnClickedItem += ClickedItemListner;
                slotIndex++;
            }
        }

        // 남은 슬롯 비활성화
        for (int i = slotIndex; i < _inventorySlots.Count; ++i)
        {
            _inventorySlots[i].OnClickedItem = null;
            _inventorySlots[i].gameObject.SetActive(false);
        }
    }
    #endregion

    #region Filter
    void ShowItemOptionFilter(PointerEventData data)
    {
        _ui_itemOptionFilter.gameObject.SetActive(true);
        RectTransform rect = _ui_itemOptionFilter.GetComponent<RectTransform>();
        rect.localScale = Vector3.one * 0.5f;

        LeanTween.scale(rect, Vector3.one, 0.5f).setEaseOutCubic();
    }

    void ApplyItemOptionFilter(Dictionary<StatusType, bool> filterDict)
    {
        _itemOptionFilter = filterDict;
        _isItemOptionFiltering = true;

        SetInventory();
    }

    void ResetAndOffItemOptionFilter()
    {
        _isItemOptionFiltering = false;

        SetInventory();
    }
    #endregion
}
