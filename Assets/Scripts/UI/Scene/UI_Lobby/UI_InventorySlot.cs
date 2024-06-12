using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InventorySlot : UI_Base, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Action<Item,ItemSlotUIType> OnClickedItem;

    ItemSlotUIType _slotType = ItemSlotUIType.Inventory;
    private bool _isDragging;
    private ScrollRect _scrollRect;
    string _itemSpriteFormat = "Textures/ItemIcons/{0}Icon/{1}_{2}";

    Image _itemImage;
    Item _item;

    public override void Init()
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        _scrollRect = GetComponentInParent<ScrollRect>();
        _itemImage = Util.FindChild<Image>(gameObject, "Image_Item");
    }

    public void SetItem(Item item)
    {
        _item = item;
        if (_item is Equipment equipment)
        {
            _itemImage.sprite = Managers.Resource.Load<Sprite>(
            string.Format(_itemSpriteFormat, equipment.equipmentType, equipment.rarity, equipment.ItemSpriteName));
        }
        else
        {

        }
        gameObject.AddUIEvent(ClickedItem);
    }

    void ClickedItem(PointerEventData data)
    {
        if (_isDragging == false)
            OnClickedItem.Invoke(_item,_slotType);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _scrollRect.OnBeginDrag(eventData);

    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        _scrollRect.OnDrag(eventData);
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        _scrollRect.OnEndDrag(eventData);
    }
}
