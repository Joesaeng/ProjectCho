using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobbyMagician : UI_Base
{
    enum Buttons
    {
        Button_EquipTab,
        Button_SpellsTab,
    }
    [SerializeField] Sprite[] _buttonTabSprites; // 0 : enabled, 1 : disenabled
    List<Button> _buttonTabs = new();
    List<UI_Base> _tabMenu = new();
    public override void Init()
    {
        Bind<Button>(typeof(Buttons));

        _tabMenu.Add(Util.FindChild<UI_MagiciansEquip>(gameObject));
        _tabMenu.Add(Util.FindChild<UI_MagiciansSpell>(gameObject));
        
        _buttonTabs.Add(GetButton((int)Buttons.Button_EquipTab));
        _buttonTabs.Add(GetButton((int)Buttons.Button_SpellsTab));
        
        for(int menuIndex = 0; menuIndex < _buttonTabs.Count; menuIndex++)
        {
            _tabMenu[menuIndex].Init();
            _buttonTabs[menuIndex].gameObject.AddUIEvent(ClickedTab, menuIndex);
        }

        EnableTabMenu(0);
    }

    void EnableTabMenu(int menuIndex)
    {
        for (int i = 0; i < _buttonTabs.Count; i++)
        {
            if (i != menuIndex)
            {
                _buttonTabs[i].GetComponent<Image>().sprite = _buttonTabSprites[1];
                _tabMenu[i].gameObject.SetActive(false);
            }
            else
            {
                _buttonTabs[i].GetComponent<Image>().sprite = _buttonTabSprites[0];
                _tabMenu[i].gameObject.SetActive(true);
            }
        }
        
    }

    void ClickedTab(int menuIndex, PointerEventData data)
    {
        EnableTabMenu(menuIndex);
    }
}
