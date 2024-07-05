using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SpellIcon : UI_Base, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private int _spellId;
    private Image _spellImage;
    private Image _spellEdge;
    private Image _imageLock;
    private Slider _slider;
    private TextMeshProUGUI _ownedCountText;

    public bool _isLock;
    private bool _isDragging;
    private ScrollRect _scrollRect;

    public Action<int> OnClickedSkillIcon;

    public override void Init()
    {
        _spellImage = GetComponent<Image>();
        _spellEdge = Util.FindChild<Image>(gameObject, "Image_SpellEdge");
        _imageLock = Util.FindChild<Image>(gameObject, "Image_Lock");
        _slider = Util.FindChild<Slider>(gameObject, "Slider_LevelUpSlider");
        _ownedCountText = Util.FindChild<TextMeshProUGUI>(gameObject, "Text_OwnedCount");

        _scrollRect = GetComponentInParent<ScrollRect>(true);
        gameObject.AddUIEvent(ClickedItem);
    }

    void ClickedItem(PointerEventData data)
    {
        if (_isDragging == false)
        {
            OnClickedSkillIcon.Invoke(_spellId);
            Managers.Sound.Play("ui_click");
        }
    }

    public void SetId(int spellId)
    {
        _spellId = spellId;
    }

    public void SetOwnedCount(int spellId)
    {
        var spellData = Managers.Status.PlayerSpells.SpellDataDict[spellId];
        int ownedCount, requireCount;
        ownedCount = spellData.ownedSpellCount;
        requireCount = spellData.requireSpellCountToLevelup;

        if (requireCount == 0)
        {
            _slider.value = 0;
            _ownedCountText.text = $"{ownedCount} / 00";
        }
        else
        {
            _slider.value = (float)ownedCount / requireCount;
            _ownedCountText.text = $"{ownedCount} / {requireCount}";
        }
    }

    public void SetOwnedCount()
    {
        _slider.value = 0;
        _ownedCountText.text = "";
    }

    public void SetImages(int spellId, Sprite edgeImage)
    {
        _spellImage.sprite = Managers.Spell.SpellSpriteDict[spellId];
        SetEdge(edgeImage);
    }

    public void SetLock()
    {
        _imageLock.gameObject.SetActive(true);
        _isLock = true;
    }

    public void SetUnlock()
    {
        _imageLock.gameObject.SetActive(false);
        _isLock = false;
    }

    public void SetEdge(Sprite edgeImage)
    {
        _spellEdge.sprite = edgeImage;
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
