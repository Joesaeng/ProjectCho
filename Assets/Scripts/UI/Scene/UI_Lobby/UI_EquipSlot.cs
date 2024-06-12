using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EquipSlot : UI_Base
{
    [SerializeField] public EquipmentType _equipmentType;
    [SerializeField] public int _slotIndex;

    [SerializeField] ItemSlotUIType _slotType;

    Equipment _equipment;
    Image _itemImage;
    [SerializeField] Sprite _nullSprite;
    string _itemSpriteFormat = "Textures/ItemIcons/{0}Icon/{1}_{2}";

    public Action<Item,ItemSlotUIType> OnClickedEquipSlot;

    public override void Init()
    {
        _itemImage = Util.FindChild<Image>(gameObject, "Image_ItemImage");
        gameObject.AddUIEvent(ClickedItem);
    }

    public void SetEquipSlot(Equipment equipment = null)
    {
        _equipment = equipment;
        if (_equipment == null)
        {
            _itemImage.sprite = _nullSprite;
            _itemImage.SetNativeSize();
            return;
        }
        _itemImage.sprite = Managers.Resource.Load<Sprite>(
            string.Format(_itemSpriteFormat, equipment.equipmentType, equipment.rarity, equipment.ItemSpriteName));
        _itemImage.SetNativeSize();

    }

    void ClickedItem(PointerEventData data)
    {
        OnClickedEquipSlot.Invoke(_equipment, _slotType);
    }
}
