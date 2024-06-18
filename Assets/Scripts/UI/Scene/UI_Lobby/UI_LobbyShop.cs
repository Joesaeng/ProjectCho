using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobbyShop : UI_Base
{
    enum Buttons
    {
        Button_Weapon,
        Button_Ring,
        Button_Spell,
        Button_Shop
    }

    enum Objects
    {
        UI_ShopWeapon,
        UI_ShopRing,
        UI_ShopSpell,
        UI_ShopShop,
        UI_Summon,
    }

    const int ShopUICount = 4;
    Button[] _tabButtons;
    Image[] _tabImages;
    UI_Base[] _shopUis;
    UI_Summon _summonUi;

    int _selectedIndex;
    Vector3 _originalPos;

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(Objects));

        _summonUi = GetObject((int)Objects.UI_Summon).GetComponent<UI_Summon>();
        _summonUi.Init();

        _tabButtons = new Button[]
        {
            GetButton((int)Buttons.Button_Weapon),
            GetButton((int)Buttons.Button_Ring),
            GetButton((int)Buttons.Button_Spell),
            GetButton((int)Buttons.Button_Shop),
        };
        _tabImages = new Image[]
        {
            GetButton((int)Buttons.Button_Weapon).GetComponent<Image>(),
            GetButton((int)Buttons.Button_Ring).GetComponent<Image>(),
            GetButton((int)Buttons.Button_Spell).GetComponent<Image>(),
            GetButton((int)Buttons.Button_Shop).GetComponent<Image>(),
        };
        _shopUis = new UI_Base[]
        {
            GetObject((int)Objects.UI_ShopWeapon).GetComponent<UI_ShopWeapon>(),
            GetObject((int)Objects.UI_ShopRing).GetComponent<UI_ShopRing>(),
            GetObject((int)Objects.UI_ShopSpell).GetComponent<UI_ShopSpell>(),
            GetObject((int)Objects.UI_ShopShop).GetComponent<UI_ShopShop>(),
        };
        for (int i = 0; i < ShopUICount; i++)
        {
            _tabButtons[i].gameObject.AddUIEvent(ClickedTabButton, i);
            _shopUis[i].Init();
        }

        _shopUis[0].GetComponent<UI_ShopWeapon>().OnClickedSummon += ClickedSummonListner;
        _shopUis[1].GetComponent<UI_ShopRing>().OnClickedSummon += ClickedSummonListner;
        _shopUis[2].GetComponent<UI_ShopSpell>().OnClickedSummon += ClickedSummonListner;
        _originalPos = _shopUis[0].GetComponent<RectTransform>().anchoredPosition;
        SetTab(0);
    }

    void SetTab(int selectedIndex)
    {
        for (int i = 0; i < _tabImages.Length; i++)
        {
            if (i == selectedIndex)
            {
                _tabImages[i].color = Color.white;
                _shopUis[i].gameObject.SetActive(true);
                RectTransform uiRect = _shopUis[i].GetComponent<RectTransform>();

                //RectTransform canvasRect = gameObject.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
                //float canvasHeight = canvasRect.rect.height;
                //uiRect.anchoredPosition = new Vector2(uiRect.anchoredPosition.x, canvasHeight * 0.5f);
                //
                //LeanTween.move(uiRect, _originalPos, 0.5f).setEase(LeanTweenType.easeOutBounce);

                uiRect.localScale = Vector3.one * 1.2f;
                
                LeanTween.scale(uiRect, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBounce);
            }
            else
            {
                _tabImages[i].color = new Color(0.7f, 0.7f, 0.7f);
                _shopUis[i].gameObject.SetActive(false);
                LeanTween.scale(_tabImages[i].rectTransform, Vector3.one, 0.2f).setEaseOutQuart();
            }
        }
    }

    void ClickedTabButton(int selectedIndex, PointerEventData data)
    {
        ButtonClickEffect(_tabImages[selectedIndex].rectTransform, 1.2f, 1.1f);
        if (_selectedIndex != selectedIndex)
        {
            _selectedIndex = selectedIndex;
            SetTab(_selectedIndex);
        }
    }

    void ClickedSummonListner(List<Item> summonItems)
    {
        _summonUi.OnSummonEquips(summonItems);
    }

    void ClickedSummonListner(Dictionary<int,int> summonSpells)
    {
        _summonUi.OnSummonSpells(summonSpells);
    }
}
